using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.Entities;

namespace FitnessClub.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Membership> _membershipRepository;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = _unitOfWork.GetRepository<User>();
            _membershipRepository = _unitOfWork.GetRepository<Membership>();
        }

        public async Task RegisterAsync(string firstName, string lastName, string username, string password)
        {
            if (!ValidationHelper.IsValidName(firstName))
                throw new ArgumentException("Неправильне ім'я.", nameof(firstName));
            if (!ValidationHelper.IsValidName(lastName))
                throw new ArgumentException("Неправильне прізвище.", nameof(lastName));
            if (!ValidationHelper.IsValidUsername(username))
                throw new ArgumentException("Неправильне ім'я користувача.", nameof(username));
            if (!ValidationHelper.IsValidPassword(password))
                throw new ArgumentException("Неправильний пароль.", nameof(password));

            var existingUsers = await _userRepository.FindAsync(u => u.Username == username);
            if (existingUsers.Any())
            {
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
            }
            catch (Exception ex)
            {
                throw new Exception("Під час реєстрації сталася помилка.", ex);
            }
        }

        public async Task<UserDto?> LoginAsync(string username, string password)
        {
            var users = await _userRepository.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> HasValidMembershipForClubAsync(int userId, int clubId, DateTime date)
        {
            var now = DateTime.UtcNow;

            var memberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate <= now &&
                     m.EndDate >= now,
                includeProperties: new System.Linq.Expressions.Expression<Func<Membership, object>>[] {
                     m => m.MembershipType, 
                     m => m.Club
                }
            );

            bool hasValidMembership = memberships.Any(m => 
                (m.MembershipType != null && m.MembershipType.IsNetwork) || 
                (m.ClubId.HasValue && m.ClubId.Value == clubId)
            );
            return hasValidMembership;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> CreateUserAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));

            var existingUsers = await _userRepository.FindAsync(u => u.Username == userDto.Username);
            if (existingUsers.Any())
            {
                throw new InvalidOperationException("Користувач з таким іменем вже існує.");
            }
            var user = _mapper.Map<User>(userDto);
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserAsync(int id, UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (id != userDto.UserId) return false;
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null) return false;
            if (existingUser.Username != userDto.Username) {
                var usersWithNewUsername = await _userRepository.FindAsync(u => u.Username == userDto.Username);
                if (usersWithNewUsername.Any(u => u.UserId != id)) {
                    throw new InvalidOperationException("Інший користувач з таким іменем вже існує.");
                }
            }

            var currentPasswordHash = existingUser.PasswordHash;
            _mapper.Map(userDto, existingUser);
            existingUser.PasswordHash = currentPasswordHash;
            _userRepository.Update(existingUser);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;
            _userRepository.Delete(user);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}