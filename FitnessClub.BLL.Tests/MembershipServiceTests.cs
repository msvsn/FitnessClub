using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Enums;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using FitnessClub.Core.Abstractions;
using FitnessClub.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;
using AutoMapper;
using NSubstitute.Core;

namespace FitnessClub.BLL.Tests
{
    public class MembershipServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IMembershipService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IMapper _mapperMock;
        private readonly IRepository<User> _userRepositoryMock;
        private readonly IRepository<MembershipType> _membershipTypeRepositoryMock;
        private readonly IRepository<Club> _clubRepositoryMock;
        private readonly IRepository<Membership> _membershipRepositoryMock;

        public MembershipServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _mapperMock = _fixture.Freeze<IMapper>();
            _userRepositoryMock = _fixture.Freeze<IRepository<User>>();
            _membershipTypeRepositoryMock = _fixture.Freeze<IRepository<MembershipType>>();
            _clubRepositoryMock = _fixture.Freeze<IRepository<Club>>();
            _membershipRepositoryMock = _fixture.Freeze<IRepository<Membership>>();

            _unitOfWorkMock.GetRepository<User>().Returns(_userRepositoryMock);
            _unitOfWorkMock.GetRepository<MembershipType>().Returns(_membershipTypeRepositoryMock);
            _unitOfWorkMock.GetRepository<Club>().Returns(_clubRepositoryMock);
            _unitOfWorkMock.GetRepository<Membership>().Returns(_membershipRepositoryMock);

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);

            _sut = _autoSubstitute.Resolve<MembershipService>();
        }

        [Fact]
        public async Task GetAllMembershipTypesAsync_ShouldReturnMappedTypes()
        {
            var membershipTypes = _fixture.CreateMany<MembershipType>(3).ToList();
            var membershipTypeDtos = _fixture.CreateMany<MembershipTypeDto>(3).ToList();

            _membershipTypeRepositoryMock.GetAllAsync().Returns(Task.FromResult(membershipTypes.AsEnumerable()));
            _mapperMock.Map<IEnumerable<MembershipTypeDto>>(membershipTypes).Returns(membershipTypeDtos);

            var result = await _sut.GetAllMembershipTypesAsync();

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(membershipTypeDtos);
            await _membershipTypeRepositoryMock.Received(1).GetAllAsync();
            _mapperMock.Received(1).Map<IEnumerable<MembershipTypeDto>>(membershipTypes);
        }

        [Fact]
        public async Task PurchaseMembershipAsync_ValidClubMembership_ShouldReturnSuccess()
        {
            var userId = _fixture.Create<int>();
            var membershipTypeId = _fixture.Create<int>();
            var clubId = _fixture.Create<int>();
            var user = _fixture.Build<User>().With(u => u.UserId, userId).Create();
            var membershipType = _fixture.Build<MembershipType>()
                .With(mt => mt.MembershipTypeId, membershipTypeId)
                .With(mt => mt.IsNetwork, false)
                .With(mt => mt.IsSingleVisit, false)
                .Create();
            var club = _fixture.Build<Club>().With(c => c.ClubId, clubId).Create();

            _userRepositoryMock.GetByIdAsync(userId).Returns(Task.FromResult<User?>(user));
            _membershipTypeRepositoryMock.GetByIdAsync(membershipTypeId).Returns(Task.FromResult<MembershipType?>(membershipType));
            _clubRepositoryMock.GetByIdAsync(clubId).Returns(Task.FromResult<Club?>(club));
            _membershipRepositoryMock.FindAsync(Arg.Any<Expression<Func<Membership, bool>>>(), Arg.Any<Expression<Func<Membership, object>>[]>()) 
                                   .Returns(Task.FromResult(Enumerable.Empty<Membership>()));

            var result = await _sut.PurchaseMembershipAsync(userId, membershipTypeId, clubId);

            result.Should().Be(MembershipPurchaseResult.Success);
            await _membershipRepositoryMock.Received(1).AddAsync(Arg.Is<Membership>(m => 
                m.UserId == userId && 
                m.MembershipTypeId == membershipTypeId && 
                m.ClubId == clubId && 
                m.StartDate.Date == DateTime.UtcNow.Date));
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task PurchaseMembershipAsync_ValidNetworkMembership_ShouldReturnSuccess()
        {
            var userId = _fixture.Create<int>();
            var membershipTypeId = _fixture.Create<int>();
            var user = _fixture.Build<User>().With(u => u.UserId, userId).Create();
            var membershipType = _fixture.Build<MembershipType>()
                .With(mt => mt.MembershipTypeId, membershipTypeId)
                .With(mt => mt.IsNetwork, true)
                .With(mt => mt.IsSingleVisit, false)
                .Create();

            _userRepositoryMock.GetByIdAsync(userId).Returns(Task.FromResult<User?>(user));
            _membershipTypeRepositoryMock.GetByIdAsync(membershipTypeId).Returns(Task.FromResult<MembershipType?>(membershipType));
            _membershipRepositoryMock.FindAsync(Arg.Any<Expression<Func<Membership, bool>>>(), Arg.Any<Expression<Func<Membership, object>>[]>()) 
                                   .Returns(Task.FromResult(Enumerable.Empty<Membership>()));

            var result = await _sut.PurchaseMembershipAsync(userId, membershipTypeId, null); 

            result.Should().Be(MembershipPurchaseResult.Success);
            await _membershipRepositoryMock.Received(1).AddAsync(Arg.Is<Membership>(m => 
                m.UserId == userId && 
                m.MembershipTypeId == membershipTypeId && 
                m.ClubId == null && 
                m.StartDate.Date == DateTime.UtcNow.Date));
            await _unitOfWorkMock.Received(1).SaveAsync();
            await _clubRepositoryMock.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }
        private bool ExpressionContainsUserIdPredicate(Expression<Func<Membership, bool>> expression, int userId)
        {
            if (expression == null) return false;
            return expression.ToString().Contains($".UserId == {userId}");
        }
    }
} 