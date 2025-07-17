using System.ComponentModel.DataAnnotations;

namespace DUPSWebApp.Models
{
	public class AppointmentViewModel
	{
		public int AppointmentId { get; set; }
		public int UserId { get; set; }
		public int ConsultantId { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn ngày hẹn")]
		[DataType(DataType.Date)]
		[Display(Name = "Ngày hẹn")]
		public DateTime AppointmentDate { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn khung giờ")]
		[Display(Name = "Khung giờ")]
		public string TimeSlot { get; set; } = string.Empty;

		[Display(Name = "Trạng thái")]
		public string Status { get; set; } = "Pending";

		[Display(Name = "Ghi chú")]
		[StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; }

		// Navigation properties for display
		public string? UserName { get; set; }
		public string? UserEmail { get; set; }
		public string? ConsultantName { get; set; }
		public string? ConsultantExpertise { get; set; }
		public string? ConsultantAvatar { get; set; }
	}

	public class ConsultantViewModel
	{
		public int ConsultantId { get; set; }
		public int UserId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public string? Expertise { get; set; }
		public string? Qualification { get; set; }
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }
		public string? WorkSchedule { get; set; }
		public bool IsActive { get; set; } = true;
	}

	public class CreateAppointmentRequest
	{
		[Required(ErrorMessage = "Vui lòng chọn tư vấn viên")]
		public int ConsultantId { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn ngày hẹn")]
		[DataType(DataType.Date)]
		public DateTime Date { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn khung giờ")]
		public string TimeSlot { get; set; } = string.Empty;

		[StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
		public string? Notes { get; set; }
	}

	public class UpdateAppointmentRequest
	{
		[Required]
		public int AppointmentId { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn trạng thái")]
		public string Status { get; set; } = string.Empty;

		public string? Notes { get; set; }
	}

	public class AppointmentFilterViewModel
	{
		public string? Status { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public int? ConsultantId { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class AppointmentListViewModel
	{
		public List<AppointmentViewModel> Appointments { get; set; } = new();
		public AppointmentFilterViewModel Filter { get; set; } = new();
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public string CurrentUserRole { get; set; } = string.Empty;
		public bool CanManage { get; set; }
	}

	public static class AppointmentStatus
	{
		public const string Pending = "Pending";
		public const string Confirmed = "Confirmed";
		public const string Completed = "Completed";
		public const string Cancelled = "Cancelled";

		public static readonly Dictionary<string, string> StatusDisplayNames = new()
		{
			{ Pending, "Chờ xác nhận" },
			{ Confirmed, "Đã xác nhận" },
			{ Completed, "Hoàn thành" },
			{ Cancelled, "Đã hủy" }
		};

		public static readonly Dictionary<string, string> StatusBadgeClasses = new()
		{
			{ Pending, "bg-warning text-dark" },
			{ Confirmed, "bg-primary" },
			{ Completed, "bg-success" },
			{ Cancelled, "bg-danger" }
		};

		public static string GetDisplayName(string status)
		{
			return StatusDisplayNames.GetValueOrDefault(status, status);
		}

		public static string GetBadgeClass(string status)
		{
			return StatusBadgeClasses.GetValueOrDefault(status, "bg-secondary");
		}
	}
}