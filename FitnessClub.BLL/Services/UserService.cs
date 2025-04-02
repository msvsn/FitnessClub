using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.BLL.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task RegisterAsync(string firstName, string lastName, string username, string password)
        {
            if (await _unitOfWork.Users.Query().AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists");

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task<UserDto> LoginAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByIdAsync(int id) => _mapper.Map<UserDto>(await _unitOfWork.Users.GetByIdAsync(id));

        public bool HasValidMembershipForClub(int userId, int clubId, DateTime date)
        {
            var memberships = _unitOfWork.Memberships.Query()
                .Include(m => m.MembershipType)
                .Where(m => m.UserId == userId && m.StartDate <= date && m.EndDate >= date);
            return memberships.Any(m => m.MembershipType.IsNetwork || m.ClubId == clubId);
        }
    }
}