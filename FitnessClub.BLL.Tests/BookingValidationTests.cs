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
    public class BookingValidationTests
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

        public BookingValidationTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
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
        public async Task BookClassAsync_InvalidScheduleOrDate_ShouldReturnInvalidScheduleOrDate()
        {
            var userId = _fixture.Create<int>();
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(1);

            _scheduleRepositoryMock.FindAsync(Arg.Any<Expression<Func<ClassSchedule, bool>>>(), Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                                 .Returns(Task.FromResult(Enumerable.Empty<ClassSchedule>())); 

            var (result, returnedBookingId) = await _sut.BookClassAsync(userId, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.InvalidScheduleOrDate);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task BookClassAsync_DateMismatchWithSchedule_ShouldReturnInvalidScheduleOrDate()
        {
            var userId = _fixture.Create<int>();
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(1); 

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.ClassScheduleId, classScheduleId)
                .With(cs => cs.DayOfWeek, classDate.AddDays(1).DayOfWeek) 
                .Create();

            _scheduleRepositoryMock.FindAsync(Arg.Any<Expression<Func<ClassSchedule, bool>>>(), Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                                 .Returns(Task.FromResult(new List<ClassSchedule> { classSchedule }.AsEnumerable()));

            var (result, returnedBookingId) = await _sut.BookClassAsync(userId, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.InvalidScheduleOrDate);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task BookClassAsync_PastDate_ShouldReturnInvalidScheduleOrDate()
        {
            var userId = _fixture.Create<int>();
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(-1); 

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.ClassScheduleId, classScheduleId)
                .With(cs => cs.DayOfWeek, classDate.DayOfWeek)
                .Create();

            _scheduleRepositoryMock.FindAsync(Arg.Any<Expression<Func<ClassSchedule, bool>>>(), Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                                 .Returns(Task.FromResult(new List<ClassSchedule> { classSchedule }.AsEnumerable()));

            var (result, returnedBookingId) = await _sut.BookClassAsync(userId, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.InvalidScheduleOrDate);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task BookClassAsync_NoAvailablePlaces_ShouldReturnNoAvailablePlaces()
        {
            var userId = _fixture.Create<int>();
            var classScheduleId = _fixture.Create<int>();
            var classDate = DateTime.Today.AddDays(1);

            var classSchedule = _fixture.Build<ClassSchedule>()
                .With(cs => cs.ClassScheduleId, classScheduleId)
                .With(cs => cs.DayOfWeek, classDate.DayOfWeek)
                .With(cs => cs.Capacity, 5)
                .With(cs => cs.BookedPlaces, 5) 
                .Without(cs => cs.Club)
                .Without(cs => cs.Trainer)
                .Without(cs => cs.Bookings)
                .Create();

            _scheduleRepositoryMock.FindAsync(Arg.Any<Expression<Func<ClassSchedule, bool>>>(), Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                                 .Returns(Task.FromResult(new List<ClassSchedule> { classSchedule }.AsEnumerable()));

            var (result, returnedBookingId) = await _sut.BookClassAsync(userId, null, classScheduleId, classDate);

            result.Should().Be(BookingResult.NoAvailablePlaces);
            returnedBookingId.Should().BeNull();
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }
    }
} 