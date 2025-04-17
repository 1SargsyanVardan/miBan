namespace WebApplication1.Models.MyModels
{
    public class UserModel
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string Description { get; set; }
        public int GroupId { get; set; }
        public string? VerificationCode { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
    }
}
