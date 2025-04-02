using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessClub.BLL.Services
{
    public enum MembershipPurchaseResult
    {
        Success,
        InvalidMembershipType,
        ClubRequiredForSingleClub,
        ClubNotNeededForNetwork,
        UserNotFound,
        UnknownError
    }

    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipService> _logger;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MembershipService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync()
        {
             _logger.LogInformation("Fetching all membership types.");
            var types = await _unitOfWork.MembershipTypes.GetAllAsync();
            return _mapper.Map<IEnumerable<MembershipTypeDto>>(types);
        }

        public async Task<MembershipPurchaseResult> PurchaseMembershipAsync(int userId, int membershipTypeId, int? clubId)
        {
             _logger.LogInformation("Attempting purchase membership Type: {MembershipTypeId}, Club: {ClubId} for User: {UserId}",
                membershipTypeId, clubId?.ToString() ?? "N/A", userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Membership purchase failed: User {UserId} not found.", userId);
                return MembershipPurchaseResult.UserNotFound;
            }

            var membershipType = await _unitOfWork.MembershipTypes.GetByIdAsync(membershipTypeId);
            if (membershipType == null)
            {
                 _logger.LogWarning("Membership purchase failed: Membership Type {MembershipTypeId} not found.", membershipTypeId);
                return MembershipPurchaseResult.InvalidMembershipType;
            }

            if (!membershipType.IsNetwork && !clubId.HasValue)
            {
                _logger.LogWarning("Membership purchase failed: Club ID is required for non-network membership type {MembershipTypeId}.", membershipTypeId);
                return MembershipPurchaseResult.ClubRequiredForSingleClub;
            }
            if (membershipType.IsNetwork && clubId.HasValue)
            {
                _logger.LogWarning("Membership purchase failed: Club ID must be null for network membership type {MembershipTypeId}.", membershipTypeId);
                return MembershipPurchaseResult.ClubNotNeededForNetwork;
            }

            if (clubId.HasValue) {
                 var club = await _unitOfWork.Clubs.GetByIdAsync(clubId.Value);
                 if (club == null) {
                      _logger.LogWarning("Membership purchase failed: Specified Club ID {ClubId} not found.", clubId.Value);
                      return MembershipPurchaseResult.InvalidMembershipType;
                 }
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
                await _unitOfWork.Memberships.AddAsync(membership);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Membership Type {MembershipTypeId} purchased successfully for User {UserId}. Membership ID: {MembershipId}",
                    membershipTypeId, userId, membership.MembershipId);
                return MembershipPurchaseResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while purchasing membership Type {MembershipTypeId} for User {UserId}.", membershipTypeId, userId);
                return MembershipPurchaseResult.UnknownError;
            }
        }

        public async Task<MembershipDto?> GetActiveMembershipAsync(int userId)
        {
            _logger.LogInformation("Fetching active membership for User {UserId}.", userId);
            var now = DateTime.UtcNow;

            var membership = await _unitOfWork.Memberships.Query()
                .Include(m => m.Club)
                .Include(m => m.MembershipType)
                .Where(m => m.UserId == userId && m.StartDate <= now && m.EndDate >= now)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                 _logger.LogInformation("No active membership found for User {UserId}.", userId);
                return null;
            }

            return _mapper.Map<MembershipDto>(membership);
        }
    }
}