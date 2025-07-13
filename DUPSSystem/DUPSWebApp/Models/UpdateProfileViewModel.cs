using System.ComponentModel.DataAnnotations;

namespace DUPSWebApp.Models
{
	public class UpdateProfileViewModel
	{
		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		[StringLength(255, ErrorMessage = "Họ tên không được quá 255 ký tự")]
		[Display(Name = "Họ và tên")]
		public string FullName { get; set; } = string.Empty;

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[Display(Name = "Số điện thoại")]
		public string? Phone { get; set; }

		[StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
		[Display(Name = "Địa chỉ")]
		public string? Address { get; set; }

		[Display(Name = "Giới tính")]
		public string? Gender { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Ngày sinh")]
		public DateTime? DateOfBirth { get; set; }
	}
}
