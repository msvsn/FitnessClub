using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.BLL.Services
{
    public enum MembershipPurchaseResult
    {
        Success,
        InvalidMembershipType,
        ClubRequiredForSingleClub,
        ClubNotNeededForNetwork
    }

    public class MembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync() =>
            _mapper.Map<IEnumerable<MembershipTypeDto>>(await _unitOfWork.MembershipTypes.GetAllAsync());

        public async Task<MembershipPurchaseResult> PurchaseMembershipAsync(int userId, int membershipTypeId, int? clubId)
        {
            var membershipType = await _unitOfWork.MembershipTypes.GetByIdAsync(membershipTypeId);

            if (membershipType == null)
                return MembershipPurchaseResult.InvalidMembershipType;

            if (!membershipType.IsNetwork && clubId == null)
                return MembershipPurchaseResult.ClubRequiredForSingleClub;

            if (membershipType.IsNetwork && clubId != null)
                return MembershipPurchaseResult.ClubNotNeededForNetwork;

            var membership = new Membership
            {
                UserId = userId,
                MembershipTypeId = membershipTypeId,
                ClubId = clubId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(membershipType.DurationDays)
            };

            await _unitOfWork.Memberships.AddAsync(membership);
            await _unitOfWork.SaveAsync();
            return MembershipPurchaseResult.Success;
        }

        public async Task<MembershipDto> GetActiveMembershipAsync(int userId)
        {
            var membership = await _unitOfWork.Memberships.Query()
                .Include(m => m.Club)
                .Include(m => m.MembershipType)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.EndDate > DateTime.Now);

            if (membership == null)
                return null;

            return _mapper.Map<MembershipDto>(membership);
        }
    }
}