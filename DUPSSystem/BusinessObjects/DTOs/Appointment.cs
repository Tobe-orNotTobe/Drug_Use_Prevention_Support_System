using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
	public class AppointmentCreateRequest
	{
		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "ConsultantId là bắt buộc")]
		public int ConsultantId { get; set; }

		[Required(ErrorMessage = "Ngày hẹn là bắt buộc")]
		public DateTime AppointmentDate { get; set; }

		[Range(15, 120, ErrorMessage = "Thời lượng phải từ 15 đến 120 phút")]
		public int? DurationMinutes { get; set; } = 60;

		[StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
		public string? Notes { get; set; }
	}

	public class AppointmentUpdateRequest
	{
		[Required(ErrorMessage = "AppointmentId là bắt buộc")]
		public int AppointmentId { get; set; }

		[Required(ErrorMessage = "Ngày hẹn là bắt buộc")]
		public DateTime AppointmentDate { get; set; }

		[Range(15, 120, ErrorMessage = "Thời lượng phải từ 15 đến 120 phút")]
		public int? DurationMinutes { get; set; }

		[StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
		public string? Notes { get; set; }

		[StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự")]
		public string? Status { get; set; }
	}

	public class AppointmentResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public int? AppointmentId { get; set; }
		public AppointmentDetail? Data { get; set; }
	}

	public class AppointmentListResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public List<AppointmentDetail> Data { get; set; } = new List<AppointmentDetail>();
	}

	public class AppointmentDetail
	{
		public int AppointmentId { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; } = null!;
		public string UserEmail { get; set; } = null!;
		public int ConsultantId { get; set; }
		public string ConsultantName { get; set; } = null!;
		public string ConsultantExpertise { get; set; } = null!;
		public DateTime AppointmentDate { get; set; }
		public int? DurationMinutes { get; set; }
		public string Status { get; set; } = null!;
		public string? Notes { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class ConsultantCreateRequest
	{
		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		[StringLength(255, ErrorMessage = "Bằng cấp không được vượt quá 255 ký tự")]
		public string? Qualification { get; set; }

		[StringLength(255, ErrorMessage = "Chuyên môn không được vượt quá 255 ký tự")]
		public string? Expertise { get; set; }

		public string? WorkSchedule { get; set; }

		public string? Bio { get; set; }
	}

	public class ConsultantUpdateRequest
	{
		[Required(ErrorMessage = "ConsultantId là bắt buộc")]
		public int ConsultantId { get; set; }

		[StringLength(255, ErrorMessage = "Bằng cấp không được vượt quá 255 ký tự")]
		public string? Qualification { get; set; }

		[StringLength(255, ErrorMessage = "Chuyên môn không được vượt quá 255 ký tự")]
		public string? Expertise { get; set; }

		public string? WorkSchedule { get; set; }

		public string? Bio { get; set; }
	}

	public class ConsultantResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public ConsultantDetail? Data { get; set; }
	}

	public class ConsultantListResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public List<ConsultantDetail> Data { get; set; } = new List<ConsultantDetail>();
	}

	public class ConsultantDetail
	{
		public int ConsultantId { get; set; }
		public int UserId { get; set; }
		public string FullName { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string? Phone { get; set; }
		public string? Qualification { get; set; }
		public string? Expertise { get; set; }
		public string? WorkSchedule { get; set; }
		public string? Bio { get; set; }
		public bool IsActive { get; set; }
		public int TotalAppointments { get; set; }
	}
}
