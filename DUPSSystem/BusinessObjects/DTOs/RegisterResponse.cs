namespace BusinessObjects.DTOs
{
	public class RegisterResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public int? UserId { get; set; }
	}
}
