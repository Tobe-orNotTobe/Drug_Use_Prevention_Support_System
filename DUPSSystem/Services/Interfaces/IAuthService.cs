using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IAuthService
	{
		Task<LoginResponse> LoginAsync(LoginRequest request);
		Task<RegisterResponse> RegisterAsync(RegisterRequest request);
		Task<UserInfo?> GetUserByIdAsync(int userId);
		string HashPassword(string password);
		bool VerifyPassword(string password, string hashedPassword);
		string GenerateJwtToken(User user, List<string> roles);

	}
}
