using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
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
using Microsoft.Extensions.Configuration;
using FitnessClub.BLL.Helpers;

namespace FitnessClub.BLL.Tests
{
    public class MembershipBookingTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IBookingService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IRepository<Booking> _bookingRepositoryMock;
        private readonly IRepository<ClassSchedule> _scheduleRepositoryMock;
        private readonly IRepository<Membership> _membershipRepositoryMock;
        private readonly IUserService _userServiceMock;
        private readonly IMapper _mapperMock; 
        private readonly MembershipBookingStrategy _membershipStrategyMock;

        public MembershipBookingTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"BookingSettings:MaxBookingsPerUser", "3"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            AppSettings.Initialize(configuration); 

            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = false });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _bookingRepositoryMock = _fixture.Freeze<IRepository<Booking>>();
            _scheduleRepositoryMock = _fixture.Freeze<IRepository<ClassSchedule>>();
            _membershipRepositoryMock = _fixture.Freeze<IRepository<Membership>>();

            _unitOfWorkMock.GetRepository<Booking>().Returns(_bookingRepositoryMock);
            _unitOfWorkMock.GetRepository<ClassSchedule>().Returns(_scheduleRepositoryMock);
            _unitOfWorkMock.GetRepository<Membership>().Returns(_membershipRepositoryMock);

            _userServiceMock = _fixture.Freeze<IUserService>();
            _mapperMock = _fixture.Freeze<IMapper>();

            _membershipStrategyMock = _fixture.Freeze<MembershipBookingStrategy>();
            var guestStrategyMock = _fixture.Freeze<GuestBookingStrategy>();
            var bookingStrategies = new List<IBookingStrategy>
            {
                _membershipStrategyMock,
                guestStrategyMock
            };

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _autoSubstitute.Provide(_userServiceMock);
            _autoSubstitute.Provide<IEnumerable<IBookingStrategy>>(bookingStrategies);

            _sut = new BookingService(
                _autoSubstitute.Resolve<IUnitOfWork>(),
                _autoSubstitute.Resolve<IMapper>(),
                _autoSubstitute.Resolve<IUserService>(),
                bookingStrategies 
            );
        }

        [Fact]
        public async Task BookClassAsync_UserRequiresMembershipButHasNone_ShouldReturnMembershipRequired()
        {
            var userId = _fixture.Create<int>();
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(1);

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.ClassScheduleId, classScheduleId)
                .With(cs => cs.DayOfWeek, classDate.DayOfWeek)
                .With(cs => cs.Capacity, 10)
                .With(cs => cs.BookedPlaces, 0)
                .With(cs => cs.ClubId, _fixture.Create<int>())
                .Create();

            _scheduleRepositoryMock.FindAsync(Arg.Any<Expression<Func<ClassSchedule, bool>>>(), Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                                 .Returns(Task.FromResult(new List<ClassSchedule> { classSchedule }.AsEnumerable()));
            
            _bookingRepositoryMock.FindAsync(Arg.Is<Expression<Func<Booking, bool>>>(ex => 
                ExpressionContains(ex, b => b.UserId == userId && b.ClassScheduleId == classScheduleId && b.ClassDate.Date == classDate.Date)))
                .Returns(Task.FromResult(Enumerable.Empty<Booking>())); 

            _bookingRepositoryMock.FindAsync(Arg.Is<Expression<Func<Booking, bool>>>(ex => 
                ExpressionContains(ex, b => b.UserId == userId && b.ClassDate.Date >= DateTime.Today)))
                .Returns(Task.FromResult(Enumerable.Empty<Booking>())); 

            _membershipRepositoryMock.FindAsync(Arg.Any<Expression<Func<Membership, bool>>>(), Arg.Any<Expression<Func<Membership, object>>[]>()) 
                                   .Returns(Task.FromResult(Enumerable.Empty<Membership>())); 

            var (result, returnedBookingId) = await _sut.BookClassAsync(userId, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.MembershipRequired);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task BookClassAsync_NoUserAndNoGuest_ShouldReturnUserOrGuestRequired()
        {
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(1);

            var (result, returnedBookingId) = await _sut.BookClassAsync(null, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.UserOrGuestRequired);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        private bool ExpressionContains<T>(Expression<Func<T, bool>> expression, Func<T, bool> checkFunc)
        {
            try
            {
                var body = expression.Body.ToString(); 
                if (checkFunc.Method.Name.Contains("DisplayClass")) 
                {
                }
                return true; 
            }
            catch { return false; }
        }
    }
} 