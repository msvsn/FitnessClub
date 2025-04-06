using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessClub.BLL.Enums;

namespace FitnessClub.BLL.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<MembershipType> _membershipTypeRepository;
        private readonly IRepository<Club> _clubRepository;
        private readonly IRepository<Membership> _membershipRepository;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = _unitOfWork.GetRepository<User>();
            _membershipTypeRepository = _unitOfWork.GetRepository<MembershipType>();
            _clubRepository = _unitOfWork.GetRepository<Club>();
            _membershipRepository = _unitOfWork.GetRepository<Membership>();
        }

        public async Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync()
        {
            var types = await _membershipTypeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MembershipTypeDto>>(types);
        }

        public async Task<MembershipPurchaseResult> PurchaseMembershipAsync(int userId, int membershipTypeId, int? clubId)
        {
            var membershipType = await _membershipTypeRepository.GetByIdAsync(membershipTypeId);
            if (membershipType == null)
            {
                return MembershipPurchaseResult.InvalidMembershipType;
            }

            if (membershipType.IsOneTimePass)
            {
                var existingOneTimePass = await GetActiveOneTimePassAsync(userId);
                if (existingOneTimePass != null)
                {
                    return MembershipPurchaseResult.AlreadyHasActiveOneTimePass;
                }
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return MembershipPurchaseResult.UserNotFound;
            }

            var existingActiveMembership = await GetActiveMembershipAsync(userId);
            
            if (existingActiveMembership != null && existingActiveMembership.IsNetworkMembership)
            {
                return MembershipPurchaseResult.CannotPurchaseWithNetwork;
            }

            if (existingActiveMembership != null && !membershipType.IsOneTimePass)
            {
                return MembershipPurchaseResult.AlreadyHasActiveMembership;
            }

            if (!membershipType.IsNetwork)
            {
                if (!clubId.HasValue)
                {
                    return MembershipPurchaseResult.ClubRequiredForSingleClub;
                }
                var club = await _clubRepository.GetByIdAsync(clubId.Value);
                if (club == null)
                {
                    return MembershipPurchaseResult.ClubNotFound;
                }
            }
            else if (clubId.HasValue)
            {
                return MembershipPurchaseResult.ClubNotNeededForNetwork;
            }

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(membershipType.DurationDays);

            var membership = new Membership
            {
                UserId = userId,
                MembershipTypeId = membershipTypeId,
                ClubId = membershipType.IsNetwork ? null : clubId,
                StartDate = startDate,
                EndDate = endDate
            };

            try
            {
                await _membershipRepository.AddAsync(membership);
                await _unitOfWork.SaveAsync();
                return MembershipPurchaseResult.Success;
            }
            catch (Exception)
            {
                return MembershipPurchaseResult.UnknownError;
            }
        }

        public async Task<MembershipDto?> GetActiveMembershipAsync(int userId)
        {
            var now = DateTime.UtcNow;
            
            var memberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId && m.StartDate <= now && m.EndDate >= now,
                m => m.Club,
                m => m.MembershipType
            );
            
            var latestEndingMembership = memberships.OrderByDescending(m => m.EndDate).FirstOrDefault();

            if (latestEndingMembership == null)
            {
                return null;
            }
            else
            {
                return _mapper.Map<MembershipDto>(latestEndingMembership);
            }
        }

        public async Task<MembershipDto?> GetActiveOneTimePassAsync(int userId)
        {
            var now = DateTime.UtcNow;

            var oneTimePasses = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate <= now &&
                     m.EndDate >= now &&
                     m.MembershipType.IsOneTimePass,
                m => m.MembershipType,
                m => m.Club
            );

            var latestEndingOneTimePass = oneTimePasses.OrderByDescending(m => m.EndDate).FirstOrDefault();

            if (latestEndingOneTimePass == null)
            {
                return null;
            }
            else
            {
                return _mapper.Map<MembershipDto>(latestEndingOneTimePass);
            }
        }

        public async Task<List<MembershipDto>> GetAllActiveMembershipsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            
            var activeMemberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId && m.StartDate <= now && m.EndDate >= now,
                m => m.Club,
                m => m.MembershipType
            );
            
            var sortedMemberships = activeMemberships.OrderBy(m => m.EndDate).ToList();

            return _mapper.Map<List<MembershipDto>>(sortedMemberships);
        }

        public async Task ConsumeOneTimePassAsync(int membershipId)
        {
            await _membershipRepository.DeleteByIdAsync(membershipId);
        }
    }
}