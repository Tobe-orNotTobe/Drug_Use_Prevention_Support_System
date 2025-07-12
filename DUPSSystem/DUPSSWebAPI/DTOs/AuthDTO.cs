using System.ComponentModel.DataAnnotations;

namespace DUPSWebAPI.DTOs
{
	public class LoginRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}

	public class RegisterRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		[MinLength(6)]
		public string Password { get; set; } = string.Empty;

		[Required]
		public string FullName { get; set; } = string.Empty;

		public DateOnly? DateOfBirth { get; set; }

		public string? Gender { get; set; }

		public string? Phone { get; set; }

		public string? Address { get; set; }
	}

	public class AuthResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public string? Token { get; set; }
		public UserInfo? User { get; set; }
	}

	public class UserInfo
	{
		public int UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public List<string> Roles { get; set; } = new List<string>();
	}
}