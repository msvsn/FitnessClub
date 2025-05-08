namespace FitnessClub.BLL.Enums
{
    public enum BookingResult
    {
        Success,
        InvalidScheduleOrDate,
        NoAvailablePlaces,
        UserOrGuestRequired,
        BookingLimitExceeded,
        MembershipRequired,
        MembershipClubMismatch,
        StrategyNotFound,
        AlreadyBooked,
        UnknownError
    }
} 