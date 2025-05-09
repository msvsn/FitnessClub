using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FitnessClub.BLL.Dtos;
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
    public class GetUserBookingsTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IBookingService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IRepository<Booking> _bookingRepositoryMock;
        private readonly IMapper _mapperMock; 
        private readonly IRepository<ClassSchedule> _scheduleRepositoryMock;
        private readonly IUserService _userServiceMock; 

        public GetUserBookingsTests()
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
        public async Task GetUserBookingsAsync_UserHasBookings_ShouldReturnMappedDtos()
        {
            var userId = _fixture.Create<int>();
            var bookings = _fixture.CreateMany<Booking>(3).ToList();
            bookings.ForEach(b => {
                b.UserId = userId;
                b.ClassSchedule = _fixture.Build<ClassSchedule>()
                                          .Without(cs => cs.Bookings)
                                          .Create(); 
            });

            var bookingDtos = _fixture.CreateMany<BookingDto>(3).ToList(); 

            _bookingRepositoryMock.FindAsync(
                Arg.Any<Expression<Func<Booking, bool>>>(),
                Arg.Any<Expression<Func<Booking, object>>[]>())
                .Returns(Task.FromResult(bookings.AsEnumerable()));
            
            _mapperMock.Map<List<BookingDto>>(Arg.Is<IEnumerable<Booking>>(list => list.Count() == 3))
                       .Returns(bookingDtos);

            var result = await _sut.GetUserBookingsAsync(userId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(bookingDtos);
            _mapperMock.Received(1).Map<List<BookingDto>>(Arg.Is<IEnumerable<Booking>>(list => list.All(b => b.UserId == userId)));
        }

        [Fact]
        public async Task GetUserBookingsAsync_UserHasNoBookings_ShouldReturnEmptyList()
        {
            var userId = _fixture.Create<int>();

            _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<Booking, bool>>>(), Arg.Any<Expression<Func<Booking, object>>[]>()) 
                                .Returns(Task.FromResult(Enumerable.Empty<Booking>()));

            _mapperMock.Map<List<BookingDto>>(Arg.Is<IEnumerable<Booking>>(list => !list.Any()))
                       .Returns(new List<BookingDto>());

            var result = await _sut.GetUserBookingsAsync(userId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mapperMock.Received(1).Map<List<BookingDto>>(Arg.Is<IEnumerable<Booking>>(list => !list.Any()));
        }
    }
} 