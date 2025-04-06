namespace FitnessClub.BLL.Enums
{
    public enum BookingResult
    {
        Success,
        InvalidScheduleOrDate,
        NoAvailablePlaces,
        UserRequired,
        BookingLimitExceeded,
        MembershipRequired,
        MembershipClubMismatch,
        StrategyNotFound,
        AlreadyBooked,
        UnknownError
    }
} 