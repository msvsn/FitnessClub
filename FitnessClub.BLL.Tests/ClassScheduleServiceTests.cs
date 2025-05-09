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
    public class ClassScheduleServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IClassScheduleService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IMapper _mapperMock;
        private readonly IRepository<ClassSchedule> _scheduleRepositoryMock;

        public ClassScheduleServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _mapperMock = _fixture.Freeze<IMapper>();
            _scheduleRepositoryMock = _fixture.Freeze<IRepository<ClassSchedule>>();

            _unitOfWorkMock.GetRepository<ClassSchedule>().Returns(_scheduleRepositoryMock);

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);

            _sut = _autoSubstitute.Resolve<ClassScheduleService>();
        }

        [Fact]
        public async Task GetSchedulesByClubAndDateAsync_ShouldReturnMappedAndSortedSchedules()
        {
            var clubId = _fixture.Create<int>();
            var date = _fixture.Create<DateTime>();
            var expectedDayOfWeek = date.DayOfWeek;

            var schedules = new List<ClassSchedule>
            {
                _fixture.Build<ClassSchedule>()
                        .With(cs => cs.ClubId, clubId)
                        .With(cs => cs.DayOfWeek, expectedDayOfWeek)
                        .With(cs => cs.StartTime, TimeSpan.FromHours(10))
                        .Without(cs => cs.Bookings)
                        .Create(),
                 _fixture.Build<ClassSchedule>()
                        .With(cs => cs.ClubId, clubId)
                        .With(cs => cs.DayOfWeek, expectedDayOfWeek)
                        .With(cs => cs.StartTime, TimeSpan.FromHours(9)) 
                        .Without(cs => cs.Bookings)
                        .Create(),
                 _fixture.Build<ClassSchedule>()
                        .With(cs => cs.ClubId, clubId + 1) 
                        .With(cs => cs.DayOfWeek, expectedDayOfWeek)
                        .Without(cs => cs.Bookings)
                        .Create(), 
                 _fixture.Build<ClassSchedule>()
                        .With(cs => cs.ClubId, clubId)
                        .With(cs => cs.DayOfWeek, expectedDayOfWeek.NextDay()) 
                        .Without(cs => cs.Bookings)
                        .Create(), 
            };
            
            var expectedSchedules = schedules
                .Where(cs => cs.ClubId == clubId && cs.DayOfWeek == expectedDayOfWeek)
                .OrderBy(cs => cs.StartTime)
                .ToList();
            
            var scheduleDtos = _fixture.CreateMany<ClassScheduleDto>(expectedSchedules.Count).ToList();

            _scheduleRepositoryMock.FindAsync(
                Arg.Any<Expression<Func<ClassSchedule, bool>>>(), 
                Arg.Any<Expression<Func<ClassSchedule, object>>[]>()) 
                .Returns(Task.FromResult(schedules.Where(cs => cs.ClubId == clubId && cs.DayOfWeek == expectedDayOfWeek).AsEnumerable()));
            
            _mapperMock.Map<IEnumerable<ClassScheduleDto>>(Arg.Is<IEnumerable<ClassSchedule>>(list => 
                list.SequenceEqual(expectedSchedules, new ScheduleComparer())))
                .Returns(scheduleDtos);

            var result = await _sut.GetSchedulesByClubAndDateAsync(clubId, date);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(scheduleDtos, options => options.WithStrictOrdering());
            _mapperMock.Received(1).Map<IEnumerable<ClassScheduleDto>>(Arg.Is<IEnumerable<ClassSchedule>>(list => 
                list.SequenceEqual(expectedSchedules, new ScheduleComparer())));
        }
    }
    public static class DayOfWeekExtensions
    {
        public static DayOfWeek NextDay(this DayOfWeek day)
        {
            return (DayOfWeek)(((int)day + 1) % 7);
        }
    }
    public class ScheduleComparer : IEqualityComparer<ClassSchedule>
    {
        public bool Equals(ClassSchedule? x, ClassSchedule? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.ClassScheduleId == y.ClassScheduleId; 
        }

        public int GetHashCode(ClassSchedule obj)
        {
            return obj.ClassScheduleId.GetHashCode();
        }
    }
} 