using System.ComponentModel.DataAnnotations;

namespace DUPSWebApp.Models
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string Password { get; set; } = null!;

		[DataType(DataType.Password)]
		[Display(Name = "Xác nhận mật khẩu")]
		[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
		public string ConfirmPassword { get; set; } = null!;

		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		[StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự")]
		[Display(Name = "Họ tên")]
		public string FullName { get; set; } = null!;

		[Display(Name = "Ngày sinh")]
		[DataType(DataType.Date)]
		public DateOnly? DateOfBirth { get; set; }

		[Display(Name = "Giới tính")]
		public string? Gender { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[Display(Name = "Số điện thoại")]
		public string? Phone { get; set; }

		[Display(Name = "Địa chỉ")]
		public string? Address { get; set; }
	}
}