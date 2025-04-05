namespace FitnessClub.BLL.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
    }
}