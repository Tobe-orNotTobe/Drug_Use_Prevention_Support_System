using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
	public class CourseRegistrationRequest
	{
		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "CourseId là bắt buộc")]
		public int CourseId { get; set; }
	}

	public class CourseRegistrationResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public int? UserCourseId { get; set; }
	}

	public class CourseCreateRequest
	{
		[Required(ErrorMessage = "Tiêu đề là bắt buộc")]
		[StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		[StringLength(50, ErrorMessage = "Đối tượng mục tiêu không được vượt quá 50 ký tự")]
		public string? TargetAudience { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0")]
		public int? DurationMinutes { get; set; }

		public bool IsActive { get; set; } = true;
	}

	public class CourseUpdateRequest
	{
		[Required(ErrorMessage = "CourseId là bắt buộc")]
		public int CourseId { get; set; }

		[Required(ErrorMessage = "Tiêu đề là bắt buộc")]
		[StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		[StringLength(50, ErrorMessage = "Đối tượng mục tiêu không được vượt quá 50 ký tự")]
		public string? TargetAudience { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0")]
		public int? DurationMinutes { get; set; }

		public bool IsActive { get; set; }
	}
}
