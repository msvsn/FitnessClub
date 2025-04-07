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
            var (membershipType, validationResult) = await ValidateMembershipTypeAsync(membershipTypeId);
            if (validationResult.HasValue) return validationResult.Value;
            validationResult = await ValidateOneTimePassConflictAsync(userId, membershipType!);
            if (validationResult.HasValue) return validationResult.Value;
            validationResult = await ValidateUserExistsAsync(userId);
            if (validationResult.HasValue) return validationResult.Value;
            validationResult = await ValidateExistingMembershipConflictAsync(userId, membershipType!);
            if (validationResult.HasValue) return validationResult.Value;
            validationResult = await ValidateClubSelectionAsync(membershipType!, clubId);
            if (validationResult.HasValue) return validationResult.Value;
            return await CreateAndSaveMembershipAsync(userId, membershipType!, clubId);
        }

        public async Task<MembershipDto?> GetActiveMembershipAsync(int userId)
        {
            var activeMemberships = await GetActiveMembershipsBaseQueryAsync(userId);

            var latestEndingMembership = activeMemberships
                .Where(m => m.MembershipType != null && !m.MembershipType.IsOneTimePass) 
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();
            return latestEndingMembership == null ? null : _mapper.Map<MembershipDto>(latestEndingMembership);
        }

        public async Task<MembershipDto?> GetActiveOneTimePassAsync(int userId)
        {
            var activeMemberships = await GetActiveMembershipsBaseQueryAsync(userId);

            var latestEndingOneTimePass = activeMemberships
                .Where(m => m.MembershipType != null && m.MembershipType.IsOneTimePass)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();
             return latestEndingOneTimePass == null ? null : _mapper.Map<MembershipDto>(latestEndingOneTimePass);
        }

        public async Task<List<MembershipDto>> GetAllActiveMembershipsAsync(int userId)
        {
            var activeMemberships = await GetActiveMembershipsBaseQueryAsync(userId);
            
            var sortedMemberships = activeMemberships.OrderBy(m => m.EndDate).ToList();

            return _mapper.Map<List<MembershipDto>>(sortedMemberships);
        }

        public async Task ConsumeOneTimePassAsync(int membershipId)
        {
            await _membershipRepository.DeleteByIdAsync(membershipId);
        }
        private async Task<(MembershipType? Type, MembershipPurchaseResult? Result)> ValidateMembershipTypeAsync(int membershipTypeId)
        {
            var membershipType = await _membershipTypeRepository.GetByIdAsync(membershipTypeId);
            if (membershipType == null)
            {
                return (null, MembershipPurchaseResult.InvalidMembershipType);
            }
            return (membershipType, null);
        }

        private async Task<MembershipPurchaseResult?> ValidateOneTimePassConflictAsync(int userId, MembershipType membershipType)
        {
            if (membershipType.IsOneTimePass)
            {
                var existingOneTimePass = await GetActiveOneTimePassAsync(userId);
                if (existingOneTimePass != null)
                {
                    return MembershipPurchaseResult.AlreadyHasActiveOneTimePass;
                }
            }
            return null;
        }

        private async Task<MembershipPurchaseResult?> ValidateUserExistsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return MembershipPurchaseResult.UserNotFound;
            }
            return null;
        }

        private async Task<MembershipPurchaseResult?> ValidateExistingMembershipConflictAsync(int userId, MembershipType newMembershipType)
        {
            var existingActiveMembership = await GetActiveMembershipAsync(userId);
            
            if (existingActiveMembership != null)
            {
                if (existingActiveMembership.IsNetworkMembership) 
                {
                     return MembershipPurchaseResult.CannotPurchaseWithNetwork;
                }
                if (!newMembershipType.IsOneTimePass)
                {
                     return MembershipPurchaseResult.AlreadyHasActiveMembership;
                }
            }
            return null;
        }

        private async Task<MembershipPurchaseResult?> ValidateClubSelectionAsync(MembershipType membershipType, int? clubId)
        {
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
            return null;
        }

        private async Task<MembershipPurchaseResult> CreateAndSaveMembershipAsync(int userId, MembershipType membershipType, int? clubId)
        {
             var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(membershipType.DurationDays);

            var membership = new Membership
            {
                UserId = userId,
                MembershipTypeId = membershipType.MembershipTypeId,
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


        private async Task<IEnumerable<Membership>> GetActiveMembershipsBaseQueryAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _membershipRepository.FindAsync(
                 m => m.UserId == userId && m.StartDate <= now && m.EndDate >= now,
                 m => m.Club,
                 m => m.MembershipType
            );
        }
    }
}