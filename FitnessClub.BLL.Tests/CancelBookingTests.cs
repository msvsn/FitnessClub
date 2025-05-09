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

namespace FitnessClub.BLL.Tests
{
    public class CancelBookingTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IBookingService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IRepository<Booking> _bookingRepositoryMock;
        private readonly IRepository<ClassSchedule> _scheduleRepositoryMock;
        private readonly IMapper _mapperMock; 
        private readonly IUserService _userServiceMock; 

        public CancelBookingTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _bookingRepositoryMock = _fixture.Freeze<IRepository<Booking>>();
            _scheduleRepositoryMock = _fixture.Freeze<IRepository<ClassSchedule>>();
            
            _unitOfWorkMock.GetRepository<Booking>().Returns(_bookingRepositoryMock);
            _unitOfWorkMock.GetRepository<ClassSchedule>().Returns(_scheduleRepositoryMock);

            _mapperMock = _fixture.Freeze<IMapper>();
            _userServiceMock = _fixture.Freeze<IUserService>();
            
            var membershipStrategyMock = _fixture.Freeze<MembershipBookingStrategy>();
            var guestStrategyMock = _fixture.Freeze<GuestBookingStrategy>();
            var bookingStrategies = new List<IBookingStrategy>
            {
                membershipStrategyMock,
                guestStrategyMock
            };

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _autoSubstitute.Provide(_userServiceMock);
            _autoSubstitute.Provide<IEnumerable<IBookingStrategy>>(bookingStrategies);

            _sut = _autoSubstitute.Resolve<BookingService>(); 
        }

        [Fact]
        public async Task CancelBookingAsync_ValidBookingAndUser_ShouldReturnTrueAndDecreaseBookedPlaces()
        {
            var bookingId = _fixture.Create<int>();
            var userId = _fixture.Create<int>();
            var initialBookedPlaces = 5;

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.BookedPlaces, initialBookedPlaces)
                .With(cs => cs.StartTime, TimeSpan.FromHours(15))
                .With(cs => cs.EndTime, TimeSpan.FromHours(16))
                .Create();

            var bookingToCancel = _fixture.Build<Booking>()
                .With(b => b.BookingId, bookingId)
                .With(b => b.UserId, userId)
                .With(b => b.ClassScheduleId, classSchedule.ClassScheduleId) 
                .With(b => b.ClassSchedule, classSchedule)
                .With(b => b.ClassDate, DateTime.Today.AddDays(1))
                .Create();

            _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<Booking, bool>>>(), Arg.Any<Expression<Func<Booking, object>>[]>()) 
                                .Returns(Task.FromResult(new List<Booking> { bookingToCancel }.AsEnumerable()));

            var result = await _sut.CancelBookingAsync(bookingId, userId);
            result.Should().BeTrue();
            _bookingRepositoryMock.Received(1).Delete(Arg.Is<Booking>(b => b.BookingId == bookingId));
            _scheduleRepositoryMock.Received(1).Update(Arg.Is<ClassSchedule>(cs => cs.ClassScheduleId == classSchedule.ClassScheduleId && cs.BookedPlaces == initialBookedPlaces - 1));
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task CancelBookingAsync_BookingNotFound_ShouldReturnFalse()
        {
            var bookingId = _fixture.Create<int>();
            var userId = _fixture.Create<int>();

            _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<Booking, bool>>>(), Arg.Any<Expression<Func<Booking, object>>[]>()) 
                                .Returns(Task.FromResult(Enumerable.Empty<Booking>()));

            var result = await _sut.CancelBookingAsync(bookingId, userId);

            result.Should().BeFalse();
            _bookingRepositoryMock.DidNotReceive().Delete(Arg.Any<Booking>());
            _scheduleRepositoryMock.DidNotReceive().Update(Arg.Any<ClassSchedule>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task CancelBookingAsync_UserDoesNotOwnBooking_ShouldReturnFalse()
        {
            var bookingId = _fixture.Create<int>();
            var ownerUserId = _fixture.Create<int>();
            var requestingUserId = _fixture.Create<int>();
            while (requestingUserId == ownerUserId) requestingUserId = _fixture.Create<int>();
            var initialBookedPlaces = 5;

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.BookedPlaces, initialBookedPlaces)
                .With(cs => cs.StartTime, TimeSpan.FromHours(15))
                .Create();

            var bookingToCancel = _fixture.Build<Booking>()
                .With(b => b.BookingId, bookingId)
                .With(b => b.UserId, ownerUserId)
                .With(b => b.ClassScheduleId, classSchedule.ClassScheduleId) 
                .With(b => b.ClassSchedule, classSchedule)
                .With(b => b.ClassDate, DateTime.Today.AddDays(1)) 
                .Create();

            _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<Booking, bool>>>(), Arg.Any<Expression<Func<Booking, object>>[]>()) 
                                .Returns(Task.FromResult(new List<Booking> { bookingToCancel }.AsEnumerable()));

            var result = await _sut.CancelBookingAsync(bookingId, requestingUserId);

            result.Should().BeFalse();
            _bookingRepositoryMock.DidNotReceive().Delete(Arg.Any<Booking>());
            _scheduleRepositoryMock.DidNotReceive().Update(Arg.Any<ClassSchedule>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task CancelBookingAsync_ClassIsInThePast_ShouldReturnFalse()
        {
            var bookingId = _fixture.Create<int>();
            var userId = _fixture.Create<int>();
            var initialBookedPlaces = 5;

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.BookedPlaces, initialBookedPlaces)
                .With(cs => cs.StartTime, TimeSpan.FromHours(9))
                .Create();

            var bookingToCancel = _fixture.Build<Booking>()
                .With(b => b.BookingId, bookingId)
                .With(b => b.UserId, userId)
                .With(b => b.ClassScheduleId, classSchedule.ClassScheduleId) 
                .With(b => b.ClassSchedule, classSchedule)
                .With(b => b.ClassDate, DateTime.Today.AddDays(-1))
                .Create();

            _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<Booking, bool>>>(), Arg.Any<Expression<Func<Booking, object>>[]>()) 
                                .Returns(Task.FromResult(new List<Booking> { bookingToCancel }.AsEnumerable()));
            var result = await _sut.CancelBookingAsync(bookingId, userId);

            result.Should().BeFalse();
            _bookingRepositoryMock.DidNotReceive().Delete(Arg.Any<Booking>());
            _scheduleRepositoryMock.DidNotReceive().Update(Arg.Any<ClassSchedule>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }
    }
} 