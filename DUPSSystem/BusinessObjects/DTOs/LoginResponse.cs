namespace BusinessObjects.DTOs
{
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
}
