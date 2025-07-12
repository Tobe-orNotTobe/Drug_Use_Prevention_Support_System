using System.ComponentModel.DataAnnotations;

namespace DUPSWebAPI.Models
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		public string Password { get; set; } = string.Empty;
	}

	public class LoginResponse
	{
		public bool Success { get; set; }
		public string Token { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public UserInfo? User { get; set; }
	}

	public class UserInfo
	{
		public int UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public List<string> Roles { get; set; } = new();
	}

	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public T? Data { get; set; }
	}
}
