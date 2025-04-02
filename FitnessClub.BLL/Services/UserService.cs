using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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

        public void Register(string firstName, string lastName, string username, string password)
        {
            if (_unitOfWork.Users.Query().Any(u => u.Username == username))
                throw new Exception("Username already exists");

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            _unitOfWork.Users.Add(user);
            _unitOfWork.Save();
        }

        public UserDto Login(string username, string password)
        {
            var user = _unitOfWork.Users.Query().FirstOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;
            return _mapper.Map<UserDto>(user);
        }

        public UserDto GetUserById(int id) => _mapper.Map<UserDto>(_unitOfWork.Users.GetById(id));

        public bool HasValidMembershipForClub(int userId, int clubId, DateTime date)
        {
            var memberships = _unitOfWork.Memberships.Query()
                .Include(m => m.MembershipType)
                .Where(m => m.UserId == userId && m.StartDate <= date && m.EndDate >= date);
            return memberships.Any(m => m.MembershipType.IsNetwork || m.ClubId == clubId);
        }
    }
}