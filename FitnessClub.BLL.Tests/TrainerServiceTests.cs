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
    public class TrainerServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly ITrainerService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IMapper _mapperMock;
        private readonly IRepository<Trainer> _trainerRepositoryMock;

        public TrainerServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _mapperMock = _fixture.Freeze<IMapper>();
            _trainerRepositoryMock = _fixture.Freeze<IRepository<Trainer>>();

            _unitOfWorkMock.GetRepository<Trainer>().Returns(_trainerRepositoryMock);

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);

            _sut = _autoSubstitute.Resolve<TrainerService>();
        }

        [Fact]
        public async Task GetTrainersByClubAsync_ShouldReturnMappedTrainersForClub()
        {
            var clubId = _fixture.Create<int>();
            var trainersInClub = _fixture.Build<Trainer>()
                                         .With(t => t.ClubId, clubId)
                                         .CreateMany(2)
                                         .ToList();
            var trainersInAnotherClub = _fixture.Build<Trainer>()
                                                .With(t => t.ClubId, clubId + 1)
                                                .CreateMany(1)
                                                .ToList();
            var allTrainers = trainersInClub.Concat(trainersInAnotherClub).ToList();
            
            var trainerDtos = _fixture.CreateMany<TrainerDto>(trainersInClub.Count).ToList();

            _trainerRepositoryMock.FindAsync(Arg.Any<Expression<Func<Trainer, bool>>>())
                                .Returns(Task.FromResult(trainersInClub.AsEnumerable())); 

            _mapperMock.Map<IEnumerable<TrainerDto>>(Arg.Is<IEnumerable<Trainer>>(list => 
                list.Count() == trainersInClub.Count && list.All(t => t.ClubId == clubId)))
                .Returns(trainerDtos);

            var result = await _sut.GetTrainersByClubAsync(clubId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(trainerDtos);
            await _trainerRepositoryMock.Received(1).FindAsync(Arg.Any<Expression<Func<Trainer, bool>>>()); 
            _mapperMock.Received(1).Map<IEnumerable<TrainerDto>>(Arg.Is<IEnumerable<Trainer>>(list => 
                list.Count() == trainersInClub.Count && list.All(t => t.ClubId == clubId)));
        }
    }
} 