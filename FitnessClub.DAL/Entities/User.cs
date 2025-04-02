using System.Collections.Generic;

namespace FitnessClub.DAL.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<Membership> Memberships { get; set; } = new List<Membership>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}