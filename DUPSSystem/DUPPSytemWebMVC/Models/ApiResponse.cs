namespace DUPSWebApp.Models
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public T? Data { get; set; }
	}

	public class LoginResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public string? Token { get; set; }
		public UserInfo? User { get; set; }
	}

	public class UserInfo
	{
		public int UserId { get; set; }
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;
		public bool IsActive { get; set; }
		public List<string> Roles { get; set; } = new List<string>();
	}

	public class RegisterResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public int? UserId { get; set; }
	}
}
