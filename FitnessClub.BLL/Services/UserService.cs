using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Membership> _membershipRepository;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = _unitOfWork.GetRepository<User>();
            _membershipRepository = _unitOfWork.GetRepository<Membership>();
        }

        public async Task RegisterAsync(string firstName, string lastName, string username, string password)
        {
            _logger.LogInformation("Attempting registration for username: {Username}", username);
            if (!ValidationHelper.IsValidName(firstName))
                throw new ArgumentException("Невірне ім'я.", nameof(firstName));
            if (!ValidationHelper.IsValidName(lastName))
                throw new ArgumentException("Невірне прізвище.", nameof(lastName));
            if (!ValidationHelper.IsValidUsername(username))
                throw new ArgumentException("Невірне ім'я користувача.", nameof(username));
            if (!ValidationHelper.IsValidPassword(password))
                throw new ArgumentException("Невірний пароль (мінімум 6 символів).", nameof(password));

            var existingUsers = await _userRepository.FindAsync(u => u.Username == username);
            if (existingUsers.Any())
            {
                _logger.LogWarning("Registration failed: Username '{Username}' already exists.", username);
                throw new InvalidOperationException("Ім'я користувача вже існує. Будь ласка, виберіть інше.");
            }

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                PasswordHash = PasswordHasher.HashPassword(password)
            };

            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("User '{Username}' registered successfully with ID: {UserId}.", username, user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save new user '{Username}' to database.", username);
                throw new Exception("Сталася помилка під час реєстрації. Будь ласка, спробуйте ще раз пізніше.", ex);
            }
        }

        public async Task<UserDto?> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Attempting login for username: {Username}", username);
            var users = await _userRepository.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                _logger.LogWarning("Login failed: User '{Username}' not found.", username);
                return null;
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Incorrect password for user '{Username}'.", username);
                return null;
            }

            _logger.LogInformation("Login successful for user '{Username}' (ID: {UserId}).", username, user.UserId);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Fetching user by ID: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found.", id);
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> HasValidMembershipForClubAsync(int userId, int clubId, DateTime date)
        {
            _logger.LogDebug("Checking valid membership for User: {UserId}, Club: {ClubId}, Date: {Date}", userId, clubId, date.ToString("yyyy-MM-dd"));
            var dateUtc = date.Date;

            var memberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate.Date <= dateUtc &&
                     m.EndDate.Date >= dateUtc,
                m => m.MembershipType
            );

            bool hasValidMembership = memberships.Any(m => m.MembershipType.IsNetwork || m.ClubId == clubId);

            _logger.LogDebug("Membership check result for User: {UserId}, Club: {ClubId}, Date: {Date} -> {HasValidMembership}", userId, clubId, date.ToString("yyyy-MM-dd"), hasValidMembership);

            return hasValidMembership;
        }
    }
}