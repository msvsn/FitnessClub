namespace FitnessClub.BLL.Enums
{
    public enum MembershipPurchaseResult
    {
        Success,
        InvalidMembershipType,
        ClubRequiredForSingleClub,
        ClubNotNeededForNetwork,
        UserNotFound,
        ClubNotFound,
        UnknownError,
        AlreadyHasActiveMainMembership,
        NetworkMembershipConflict,
        SameClubSingleVisitConflict,
        ClubRequiredForSingleVisit,
        NetworkMembershipIsExclusive,
        AlreadyHasActiveSingleVisitMembership
    }
} 