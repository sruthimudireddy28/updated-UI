namespace AuthService.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; } = "Approved";
    }

    public class UpdateManagerStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? ContactNumber { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = "Approved";
        public DateTime ExpiresAt { get; set; }
        public UserResponseDto User { get; set; } = new UserResponseDto();
    }

    public class RegisterRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest";
    }
}
