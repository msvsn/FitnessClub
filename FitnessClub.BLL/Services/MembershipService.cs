using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<MembershipTypeDto> GetAllMembershipTypes() =>
            _mapper.Map<IEnumerable<MembershipTypeDto>>(_unitOfWork.MembershipTypes.GetAll());

        public MembershipPurchaseResult PurchaseMembership(int userId, int membershipTypeId, int? clubId)
        {
            var membershipType = _unitOfWork.MembershipTypes.GetById(membershipTypeId);
            
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
            
            _unitOfWork.Memberships.Add(membership);
            _unitOfWork.Save();
            return MembershipPurchaseResult.Success;
        }

        public MembershipDto GetActiveMembership(int userId)
        {
            var membership = _unitOfWork.Memberships.Query()
                .Include(m => m.Club)
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.UserId == userId && m.EndDate > DateTime.Now);
                
            if (membership == null)
                return null;
                
            return _mapper.Map<MembershipDto>(membership);
        }
    }
}