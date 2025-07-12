using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
	public class RegisterRequest
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string Password { get; set; } = null!;

		[Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
		[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
		public string ConfirmPassword { get; set; } = null!;

		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		[StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự")]
		public string FullName { get; set; } = null!;

		public DateOnly? DateOfBirth { get; set; }

		[StringLength(20, ErrorMessage = "Giới tính không được vượt quá 20 ký tự")]
		public string? Gender { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string? Phone { get; set; }

		[StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
		public string? Address { get; set; }
	}
}
