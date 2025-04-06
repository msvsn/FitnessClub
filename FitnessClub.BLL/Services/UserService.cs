using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Helpers;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;

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
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var existingUser = (await _userRepository.FindAsync(u => u.Username == username)).FirstOrDefault();
            if (existingUser != null)
            {
                throw new InvalidOperationException("Користувач з таким іменем вже існує.");
            }

            var passwordHash = PasswordHasher.HashPassword(password);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                PasswordHash = passwordHash
            };

            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не вдалося зберегти дані користувача. Спробуйте ще раз.", ex);
            }
        }

        public async Task<UserDto?> LoginAsync(string username, string password)
        {
            var user = (await _userRepository.FindAsync(u => u.Username == username)).FirstOrDefault();

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
            var dateUtc = date.Date;

            var memberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate.Date <= dateUtc &&
                     m.EndDate.Date >= dateUtc,
                m => m.MembershipType
            );

            bool hasValidMembership = memberships.Any(m => m.MembershipType.IsNetwork || m.ClubId == clubId);

            return hasValidMembership;
        }
    }
}