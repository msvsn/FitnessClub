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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return MembershipPurchaseResult.UserNotFound;

            var typeToPurchase = await _membershipTypeRepository.GetByIdAsync(membershipTypeId);
            if (typeToPurchase == null) return MembershipPurchaseResult.InvalidMembershipType;

            var activeMainMembership = await GetActiveMainMembershipEntityAsync(userId);
            var activeNetworkMemberships = await GetActiveNetworkMembershipEntitiesAsync(userId);
            var activeSingleVisitMembership = await GetActiveSingleVisitMembershipEntityAsync(userId);
            bool hasActiveMainNetwork = activeMainMembership?.MembershipType?.IsNetwork ?? false;
            bool hasActiveMainClub = activeMainMembership != null && !hasActiveMainNetwork;

            bool buyingNetwork = typeToPurchase.IsNetwork;
            bool buyingSingleVisit = typeToPurchase.IsSingleVisit;

            if (!buyingSingleVisit && activeMainMembership != null) {
                return MembershipPurchaseResult.AlreadyHasActiveMainMembership;
            }

            if (buyingSingleVisit && activeSingleVisitMembership != null) {
                return MembershipPurchaseResult.AlreadyHasActiveSingleVisitMembership;
            }
            
            if (hasActiveMainNetwork) {
                return MembershipPurchaseResult.NetworkMembershipIsExclusive; 
            }
            if (buyingNetwork && !buyingSingleVisit) {
                if (hasActiveMainClub || activeSingleVisitMembership != null) {
                     return MembershipPurchaseResult.NetworkMembershipIsExclusive;
                }
            }

            if (!buyingNetwork && !buyingSingleVisit) {
            }
            if (!buyingNetwork) {
                if (!clubId.HasValue) {
                     return MembershipPurchaseResult.ClubRequiredForSingleClub;
                } 
            } else {
                if (clubId.HasValue) {
                    return MembershipPurchaseResult.ClubNotNeededForNetwork;
                }
            }

            if (clubId.HasValue) {
                var club = await _clubRepository.GetByIdAsync(clubId.Value);
                if (club == null) return MembershipPurchaseResult.ClubNotFound;
            }
            
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(typeToPurchase.DurationDays);
            
            var membership = new Membership
            {
                UserId = userId,
                MembershipTypeId = membershipTypeId,
                ClubId = buyingNetwork ? null : clubId,
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

            if (latestEndingMembership == null || latestEndingMembership.MembershipType == null)
            {
                return null;
            }

            MembershipDto? mappedDto = _mapper.Map<MembershipDto>(latestEndingMembership);
            return mappedDto;
        }

        private async Task<Membership?> GetActiveMainMembershipEntityAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var activeMemberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate <= now &&
                     m.EndDate >= now &&
                     m.MembershipType != null &&
                     !m.MembershipType.IsSingleVisit,
                includeProperties: new System.Linq.Expressions.Expression<Func<Membership, object>>[] { 
                    m => m.MembershipType, 
                    m => m.Club 
                }
            );
            Membership? result = activeMemberships.OrderByDescending(m => m.EndDate).FirstOrDefault();
            return result;
        }

        private async Task<IEnumerable<Membership>> GetActiveNetworkMembershipEntitiesAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate <= now &&
                     m.EndDate >= now &&
                     m.MembershipType != null &&
                     m.MembershipType.IsNetwork,
                includeProperties: new System.Linq.Expressions.Expression<Func<Membership, object>>[] { 
                    m => m.MembershipType 
                }
            );
        }

        private async Task<Membership?> GetActiveSingleVisitMembershipEntityAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var activeMemberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId &&
                     m.StartDate <= now &&
                     m.EndDate >= now &&
                     m.MembershipType != null &&
                     m.MembershipType.IsSingleVisit &&
                     !m.IsUsed,
                includeProperties: new System.Linq.Expressions.Expression<Func<Membership, object>>[] {
                    m => m.MembershipType,
                    m => m.Club
                }
            );
            return activeMemberships.OrderByDescending(m => m.EndDate).FirstOrDefault();
        }

        public async Task<IEnumerable<MembershipDto>> GetAllUserMembershipsAsync(int userId)
        {
            var memberships = await _membershipRepository.FindAsync(
                m => m.UserId == userId,
                includeProperties: new System.Linq.Expressions.Expression<Func<Membership, object>>[] {
                    m => m.MembershipType,
                    m => m.Club
                }
            );

            var sortedMemberships = memberships.OrderByDescending(m => m.EndDate);
            
            return _mapper.Map<IEnumerable<MembershipDto>>(sortedMemberships);
        }
    }
}