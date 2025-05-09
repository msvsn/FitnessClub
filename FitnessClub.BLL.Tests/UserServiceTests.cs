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
using FitnessClub.BLL.Helpers;
using NSubstitute.Core;

namespace FitnessClub.BLL.Tests
{
    public class UserServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IUserService _sut; 
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IMapper _mapperMock;
        private readonly IRepository<User> _userRepositoryMock;
        private readonly IRepository<Membership> _membershipRepositoryMock;

        public UserServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _mapperMock = _fixture.Freeze<IMapper>();
            _userRepositoryMock = _fixture.Freeze<IRepository<User>>();
            _membershipRepositoryMock = _fixture.Freeze<IRepository<Membership>>();

            _unitOfWorkMock.GetRepository<User>().Returns(_userRepositoryMock);
            _unitOfWorkMock.GetRepository<Membership>().Returns(_membershipRepositoryMock);

            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);

            _sut = _autoSubstitute.Resolve<UserService>();
        }

        [Fact]
        public async Task RegisterAsync_ValidData_ShouldAddUserAndSave()
        {
            var firstName = "ValidName";
            var lastName = "ValidSurname";
            var username = "validuser123";
            var password = "ValidPassword123!";

            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(Enumerable.Empty<User>()));

            await _sut.RegisterAsync(firstName, lastName, username, password);

            await _userRepositoryMock.Received(1).AddAsync(Arg.Is<User>(u => 
                u.FirstName == firstName &&
                u.LastName == lastName &&
                u.Username == username &&
                PasswordHasher.VerifyPassword(password, u.PasswordHash)));
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RegisterAsync_UsernameExists_ShouldThrowInvalidOperationException()
        {
            var firstName = "ValidName";
            var lastName = "ValidSurname";
            var username = "existinguser";
            var password = "ValidPassword123!";
            var existingUser = _fixture.Build<User>().With(u => u.Username, username).Create();

            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(new List<User> { existingUser }.AsEnumerable()));

            Func<Task> act = async () => await _sut.RegisterAsync(firstName, lastName, username, password);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Ім'я користувача вже існує*");
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Theory]
        [InlineData("In1", "Valid", "validuser", "ValidPass1!")] 
        [InlineData("Valid", "In1v", "validuser", "ValidPass1!")]
        [InlineData("Valid", "Valid", "i", "ValidPass1!")]
        [InlineData("Valid", "Valid", "validuser", "short")]
        public async Task RegisterAsync_InvalidInput_ShouldThrowArgumentException(string fName, string lName, string uName, string pwd)
        { 
            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(Enumerable.Empty<User>()));

            Func<Task> act = async () => await _sut.RegisterAsync(fName, lName, uName, pwd);

            await act.Should().ThrowAsync<ArgumentException>();
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnUserDto()
        {
            var username = "testuser";
            var password = "Password123!";
            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = _fixture.Build<User>()
                               .With(u => u.Username, username)
                               .With(u => u.PasswordHash, hashedPassword)
                               .Create();
            var userDto = _fixture.Build<UserDto>()
                                  .With(dto => dto.Username, username)
                                  .Create();

            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(new List<User> { user }.AsEnumerable()));
            _mapperMock.Map<UserDto>(user).Returns(userDto);

            var result = await _sut.LoginAsync(username, password);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(userDto);
            _mapperMock.Received(1).Map<UserDto>(user);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ShouldReturnNull()
        {
            var username = "nonexistentuser";
            var password = "Password123!";

            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(Enumerable.Empty<User>()));

            var result = await _sut.LoginAsync(username, password);

            result.Should().BeNull();
            _mapperMock.DidNotReceive().Map<UserDto>(Arg.Any<User>());
        }

        [Fact]
        public async Task LoginAsync_IncorrectPassword_ShouldReturnNull()
        {
            var username = "testuser";
            var correctPassword = "Password123!";
            var incorrectPassword = "WrongPassword123!";
            var hashedPassword = PasswordHasher.HashPassword(correctPassword);
            var user = _fixture.Build<User>()
                               .With(u => u.Username, username)
                               .With(u => u.PasswordHash, hashedPassword)
                               .Create();

            _userRepositoryMock.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                             .Returns(Task.FromResult(new List<User> { user }.AsEnumerable()));

            var result = await _sut.LoginAsync(username, incorrectPassword);

            result.Should().BeNull();
            _mapperMock.DidNotReceive().Map<UserDto>(Arg.Any<User>());
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsUserDto()
        {
            var userId = _fixture.Create<int>();
            var user = _fixture.Build<User>().With(u => u.UserId, userId).Create();
            var userDto = _fixture.Build<UserDto>().With(ud => ud.UserId, userId).Create();

            _unitOfWorkMock.GetRepository<User>().GetByIdAsync(userId).Returns(Task.FromResult<User?>(user));
            _mapperMock.Map<UserDto>(user).Returns(userDto);

            var result = await _sut.GetUserByIdAsync(userId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(userDto);
            await _unitOfWorkMock.GetRepository<User>().Received(1).GetByIdAsync(userId);
            _mapperMock.Received(1).Map<UserDto>(user);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
        {
            var userId = _fixture.Create<int>();
            _unitOfWorkMock.GetRepository<User>().GetByIdAsync(userId).Returns(Task.FromResult<User?>(null));

            var result = await _sut.GetUserByIdAsync(userId);

            result.Should().BeNull();
            await _unitOfWorkMock.GetRepository<User>().Received(1).GetByIdAsync(userId);
            _mapperMock.DidNotReceive().Map<UserDto>(Arg.Any<User>());
        }
    }
} 