using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Dtos;
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
    public class ClubServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IClubService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IRepository<Club> _clubRepositoryMock;
        private readonly IMapper _mapperMock;

        public ClubServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _clubRepositoryMock = _fixture.Freeze<IRepository<Club>>();
            _unitOfWorkMock.GetRepository<Club>().Returns(_clubRepositoryMock);

            _mapperMock = _fixture.Freeze<IMapper>();

            _autoSubstitute = new AutoSubstitute(); 
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            
            _sut = _autoSubstitute.Resolve<ClubService>();
        }

        [Fact]
        public async Task GetAllClubsAsync_ClubsExist_ReturnsClubDtos()
        {
            var clubs = _fixture.CreateMany<Club>(3).ToList();
            var clubDtos = _fixture.CreateMany<ClubDto>(3).ToList();

            _clubRepositoryMock.GetAllAsync().Returns(Task.FromResult(clubs.AsEnumerable()));
            _mapperMock.Map<IEnumerable<ClubDto>>(clubs).Returns(clubDtos);

            var result = await _sut.GetAllClubsAsync();

            result.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(clubDtos);
            await _clubRepositoryMock.Received(1).GetAllAsync();
            _mapperMock.Received(1).Map<IEnumerable<ClubDto>>(clubs);
        }

        [Fact]
        public async Task GetAllClubsAsync_NoClubsExist_ReturnsEmptyList()
        {
            _clubRepositoryMock.GetAllAsync().Returns(Task.FromResult(Enumerable.Empty<Club>()));
            _mapperMock.Map<IEnumerable<ClubDto>>(Arg.Any<IEnumerable<Club>>()).Returns(Enumerable.Empty<ClubDto>());

            var result = await _sut.GetAllClubsAsync();

            result.Should().NotBeNull().And.BeEmpty();
            await _clubRepositoryMock.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetClubByIdAsync_ClubExists_ReturnsClubDto()
        {
            var clubId = _fixture.Create<int>();
            var club = _fixture.Build<Club>().With(c => c.ClubId, clubId).Create();
            var clubDto = _fixture.Build<ClubDto>().With(c => c.ClubId, clubId).Create();

            _clubRepositoryMock.GetByIdAsync(clubId).Returns(Task.FromResult<Club?>(club));
            _mapperMock.Map<ClubDto>(club).Returns(clubDto);

            var result = await _sut.GetClubByIdAsync(clubId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(clubDto);
            await _clubRepositoryMock.Received(1).GetByIdAsync(clubId);
            _mapperMock.Received(1).Map<ClubDto>(club);
        }

        [Fact]
        public async Task GetClubByIdAsync_ClubDoesNotExist_ReturnsNull()
        {
            var clubId = _fixture.Create<int>();
            _clubRepositoryMock.GetByIdAsync(clubId).Returns(Task.FromResult<Club?>(null));

            var result = await _sut.GetClubByIdAsync(clubId);

            result.Should().BeNull();
            await _clubRepositoryMock.Received(1).GetByIdAsync(clubId);
            _mapperMock.DidNotReceive().Map<ClubDto>(Arg.Any<Club>());
        }
    }
} 