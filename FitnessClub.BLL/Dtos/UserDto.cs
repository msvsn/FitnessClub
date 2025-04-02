namespace FitnessClub.BLL.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string FullName => $"{FirstName} {LastName}"; // Calculated property
    }
}