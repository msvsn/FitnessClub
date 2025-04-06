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
        AlreadyHasActiveMembership,
        MembershipAlreadyCoversClub,
        CannotPurchaseWithNetwork,
        AlreadyHasActiveOneTimePass,
        UnknownError
    }
} 