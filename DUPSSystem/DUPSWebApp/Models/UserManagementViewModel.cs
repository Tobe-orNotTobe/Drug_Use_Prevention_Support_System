using System.ComponentModel.DataAnnotations;

namespace DUPSWebApp.Models
{
	public class UserManagementViewModel
	{
		public PaginatedResult<UserDto> Users { get; set; } = new();
		public UserSearchFilterDto SearchFilter { get; set; } = new();
		public List<RoleOptionDto> RoleOptions { get; set; } = new();
		public List<StatusOptionDto> StatusOptions { get; set; } = new();
		public UserManagementStatsDto Stats { get; set; } = new();
	}

	public class UserDto
	{
		public int UserId { get; set; }

		[Display(Name = "Họ tên")]
		public string FullName { get; set; }

		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "Điện thoại")]
		public string Phone { get; set; }

		[Display(Name = "Role")]
		public string Role { get; set; }

		[Display(Name = "Trạng thái")]
		public string Status { get; set; }

		[Display(Name = "Ngày tạo")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Cập nhật cuối")]
		public DateTime? UpdatedAt { get; set; }

		[Display(Name = "Đăng nhập cuối")]
		public DateTime? LastLoginAt { get; set; }

		public string Address { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public int LoginCount { get; set; }

		// Computed properties
		public string StatusBadgeClass => Status == "Active" ? "bg-success" : "bg-secondary";
		public string RoleBadgeClass => GetRoleBadgeClass(Role);
		public string CreatedDateString => CreatedAt.ToString("dd/MM/yyyy");
		public string LastLoginString => LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập";

		private string GetRoleBadgeClass(string role)
		{
			return role switch
			{
				"Admin" => "bg-danger",
				"Manager" => "bg-warning text-dark",
				"Staff" => "bg-info",
				"Consultant" => "bg-success",
				"Member" => "bg-primary",
				_ => "bg-secondary"
			};
		}
	}

	public class UserSearchFilterDto
	{
		[Display(Name = "Họ tên")]
		public string FullName { get; set; } = string.Empty;

		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Display(Name = "Điện thoại")]
		public string Phone { get; set; } = string.Empty;

		[Display(Name = "Role")]
		public string Role { get; set; } = string.Empty;

		[Display(Name = "Trạng thái")]
		public string Status { get; set; } = string.Empty;

		[Display(Name = "Từ ngày")]
		public DateTime? FromDate { get; set; }

		[Display(Name = "Đến ngày")]
		public DateTime? ToDate { get; set; }

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string SortBy { get; set; } = "CreatedAt";
		public string SortDirection { get; set; } = "desc";

		public bool HasFilter => !string.IsNullOrEmpty(FullName) ||
								!string.IsNullOrEmpty(Email) ||
								!string.IsNullOrEmpty(Phone) ||
								!string.IsNullOrEmpty(Role) ||
								!string.IsNullOrEmpty(Status) ||
								FromDate.HasValue ||
								ToDate.HasValue;
	}

	public class CreateUserDto
	{
		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		[StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
		[Display(Name = "Họ tên")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không đúng định dạng")]
		[StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
		[Display(Name = "Mật khẩu")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
		[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
		[Display(Name = "Xác nhận mật khẩu")]
		public string ConfirmPassword { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
		[StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
		[Display(Name = "Điện thoại")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Role là bắt buộc")]
		[Display(Name = "Role")]
		public string Role { get; set; }

		[Display(Name = "Trạng thái")]
		public string Status { get; set; } = "Active";

		[StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
		[Display(Name = "Địa chỉ")]
		public string Address { get; set; }

		[Display(Name = "Ngày sinh")]
		public DateTime? DateOfBirth { get; set; }

		[Display(Name = "Giới tính")]
		public string Gender { get; set; }
	}

	public class UpdateUserDto
	{
		public int UserId { get; set; }

		[Required(ErrorMessage = "Họ tên là bắt buộc")]
		[StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
		[Display(Name = "Họ tên")]
		public string FullName { get; set; }

		[Display(Name = "Email")]
		public string Email { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
		[StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
		[Display(Name = "Điện thoại")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Role là bắt buộc")]
		[Display(Name = "Role")]
		public string Role { get; set; }

		[Display(Name = "Trạng thái")]
		public string Status { get; set; }

		[StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
		[Display(Name = "Địa chỉ")]
		public string Address { get; set; }

		[Display(Name = "Ngày sinh")]
		public DateTime? DateOfBirth { get; set; }

		[Display(Name = "Giới tính")]
		public string Gender { get; set; }
	}

	public class UserDetailDto : UserDto
	{
		public List<UserAppointmentDto> RecentAppointments { get; set; } = new();
		public List<UserCourseDto> EnrolledCourses { get; set; } = new();
		public List<UserSurveyDto> CompletedSurveys { get; set; } = new();
		public UserActivityStatsDto ActivityStats { get; set; } = new();
	}

	public class UserAppointmentDto
	{
		public int AppointmentId { get; set; }
		public string ConsultantName { get; set; }
		public DateTime AppointmentDate { get; set; }
		public string Status { get; set; }
		public string Notes { get; set; }
	}

	public class UserCourseDto
	{
		public int CourseId { get; set; }
		public string CourseName { get; set; }
		public DateTime EnrolledDate { get; set; }
		public string Status { get; set; }
		public int Progress { get; set; }
	}

	public class UserSurveyDto
	{
		public int SurveyId { get; set; }
		public string SurveyName { get; set; }
		public DateTime CompletedDate { get; set; }
		public string Result { get; set; }
		public int Score { get; set; }
	}

	public class UserActivityStatsDto
	{
		public int TotalAppointments { get; set; }
		public int TotalCoursesEnrolled { get; set; }
		public int TotalSurveysCompleted { get; set; }
		public int LoginCount { get; set; }
		public DateTime? LastActivity { get; set; }
	}

	public class RoleOptionDto
	{
		public string Value { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
		public bool IsSelected { get; set; }
	}

	public class StatusOptionDto
	{
		public string Value { get; set; }
		public string Text { get; set; }
		public string BadgeClass { get; set; }
		public bool IsSelected { get; set; }
	}

	public class UserManagementStatsDto
	{
		public int TotalUsers { get; set; }
		public int ActiveUsers { get; set; }
		public int InactiveUsers { get; set; }
		public int NewUsersThisMonth { get; set; }
		public Dictionary<string, int> UsersByRole { get; set; } = new();
		public Dictionary<string, int> UsersByStatus { get; set; } = new();
	}

	public class PaginatedResult<T>
	{
		public List<T> Data { get; set; } = new();
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
		public bool HasPreviousPage => Page > 1;
		public bool HasNextPage => Page < TotalPages;
		public int StartIndex => (Page - 1) * PageSize + 1;
		public int EndIndex => Math.Min(Page * PageSize, TotalCount);
	}

	public class UserManagementApiResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public object Data { get; set; }
		public List<string> Errors { get; set; } = new();
		public Dictionary<string, string> ValidationErrors { get; set; } = new();
	}

	public class BulkUserActionDto
	{
		public List<int> UserIds { get; set; } = new();
		public string Action { get; set; } // "activate", "deactivate", "delete", "changeRole"
		public string NewRole { get; set; }
		public string Reason { get; set; }
	}

	public class UserImportDto
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Role { get; set; }
		public string Status { get; set; }
		public string Address { get; set; }
		public string Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public int RowNumber { get; set; }
		public List<string> ValidationErrors { get; set; } = new();
		public bool IsValid => !ValidationErrors.Any();
	}

	public class UserExportDto
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Role { get; set; }
		public string Status { get; set; }
		public string CreatedDate { get; set; }
		public string LastLogin { get; set; }
		public int LoginCount { get; set; }
		public string Address { get; set; }
		public string Gender { get; set; }
		public string DateOfBirth { get; set; }
	}
}