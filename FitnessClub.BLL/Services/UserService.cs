using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task RegisterAsync(string firstName, string lastName, string username, string password)
        {
             _logger.LogInformation("Attempting registration for username: {Username}", username);
            if (!ValidationHelper.IsValidName(firstName))
                throw new ArgumentException("Invalid first name provided.", nameof(firstName));
            if (!ValidationHelper.IsValidName(lastName))
                throw new ArgumentException("Invalid last name provided.", nameof(lastName));
            if (!ValidationHelper.IsValidUsername(username))
                throw new ArgumentException("Invalid username provided.", nameof(username));
            if (!ValidationHelper.IsValidPassword(password))
                throw new ArgumentException("Invalid password provided (minimum 6 characters required).", nameof(password));

            bool usernameExists = await _unitOfWork.Users.Query().AnyAsync(u => u.Username == username);
            if (usernameExists)
            {
                 _logger.LogWarning("Registration failed: Username '{Username}' already exists.", username);
                throw new InvalidOperationException("Username already exists. Please choose a different one."); // More specific exception
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
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveAsync();
                 _logger.LogInformation("User '{Username}' registered successfully with ID: {UserId}.", username, user.UserId);
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to save new user '{Username}' to database.", username);
                 throw new Exception("An error occurred during registration. Please try again later.", ex);
            }
        }

        public async Task<UserDto?> LoginAsync(string username, string password)
        {
             _logger.LogInformation("Attempting login for username: {Username}", username);
            var user = await _unitOfWork.Users.Query()
                            .FirstOrDefaultAsync(u => u.Username == username);

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
            var user = await _unitOfWork.Users.GetByIdAsync(id);
             if (user == null)
            {
                 _logger.LogWarning("User with ID: {UserId} not found.", id);
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> HasValidMembershipForClubAsync(int userId, int clubId, DateTime date)
        {
            _logger.LogDebug("Checking valid membership for User: {UserId}, Club: {ClubId}, Date: {Date}",
                             userId, clubId, date.ToString("yyyy-MM-dd"));
            var dateUtc = date.Date;

            bool hasValidMembership = await _unitOfWork.Memberships.Query()
                .Include(m => m.MembershipType)
                .AnyAsync(m =>
                    m.UserId == userId &&
                    m.StartDate.Date <= dateUtc &&
                    m.EndDate.Date >= dateUtc &&
                    (m.MembershipType.IsNetwork || m.ClubId == clubId)
                );

             _logger.LogDebug("Membership check result for User: {UserId}, Club: {ClubId}, Date: {Date} -> {HasValidMembership}",
                             userId, clubId, date.ToString("yyyy-MM-dd"), hasValidMembership);

            return hasValidMembership;
        }
    }
}