using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public UserService(AuthDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.FailResponse("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.FailResponse("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            var response = new LoginResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                ApprovalStatus = user.ApprovalStatus,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                User = MapToUserResponse(user)
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful");
        }

        public async Task<ApiResponse<UserResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return ApiResponse<UserResponseDto>.FailResponse("Email already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                ApprovalStatus = request.Role == "Manager" ? "Pending" : "Approved",
                ContactNumber = request.ContactNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = MapToUserResponse(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(response, "User registered successfully");
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var response = users.Select(MapToUserResponse).ToList();
            return ApiResponse<List<UserResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || !user.IsActive)
            {
                return ApiResponse<UserResponseDto>.FailResponse("User not found");
            }

            var response = MapToUserResponse(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
            {
                return ApiResponse<UserResponseDto>.FailResponse("User not found");
            }

            var response = MapToUserResponse(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int userId, UpdateUserDto request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || !user.IsActive)
            {
                return ApiResponse<UserResponseDto>.FailResponse("User not found");
            }

            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;

            if (!string.IsNullOrEmpty(request.ContactNumber))
                user.ContactNumber = request.ContactNumber;

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = MapToUserResponse(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(response, "User updated successfully");
        }

        public async Task<ApiResponse> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return ApiResponse.FailResponse("User not found");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("User deleted successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var activeRole = user.Role;
            if (user.Role == "Manager" && user.ApprovalStatus != "Approved")
            {
                activeRole = "Guest";
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, activeRole)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserResponseDto MapToUserResponse(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ContactNumber = user.ContactNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                ApprovalStatus = user.ApprovalStatus
            };
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetManagersAsync()
        {
            var managers = await _context.Users
                .Where(u => u.Role == "Manager" && u.IsActive)
                .ToListAsync();

            var response = managers.Select(MapToUserResponse).ToList();
            return ApiResponse<List<UserResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateManagerStatusAsync(int userId, string approvalStatus)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive || user.Role != "Manager")
            {
                return ApiResponse<UserResponseDto>.FailResponse("Manager not found");
            }

            if (approvalStatus != "Approved" && approvalStatus != "Pending" && approvalStatus != "Rejected")
            {
                return ApiResponse<UserResponseDto>.FailResponse("Invalid approval status");
            }

            user.ApprovalStatus = approvalStatus;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = MapToUserResponse(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(response, $"Manager status updated to {approvalStatus} successfully");
        }
    }
}
