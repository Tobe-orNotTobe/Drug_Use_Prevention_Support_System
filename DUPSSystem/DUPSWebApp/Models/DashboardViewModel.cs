using System.ComponentModel.DataAnnotations;

namespace DUPSWebApp.Models
{
	public class DashboardViewModel
	{
		public DashboardStatsDto Stats { get; set; } = new();
		public UsersByRoleChartDto UsersByRoleChart { get; set; } = new();
		public AppointmentsByStatusChartDto AppointmentsByStatusChart { get; set; } = new();
		public List<RecentActivityDto> RecentActivities { get; set; } = new();
		public DateTime LastUpdated { get; set; } = DateTime.Now;
	}

	public class DashboardStatsDto
	{
		public int TotalUsers { get; set; }
		public int TotalConsultants { get; set; }
		public int TotalAppointments { get; set; }
		public int TotalCourses { get; set; }
		public int TotalSurveys { get; set; }
		public int ActiveUsers { get; set; }
		public int InactiveUsers { get; set; }
		public int PendingAppointments { get; set; }
		public int CompletedAppointments { get; set; }
	}

	public class UsersByRoleChartDto
	{
		public List<string> Labels { get; set; } = new() { "Member", "Staff", "Consultant", "Manager", "Admin" };
		public List<int> Values { get; set; } = new() { 0, 0, 0, 0, 0 };
		public List<string> Colors { get; set; } = new()
		{
			"#4e73df", "#1cc88a", "#36b9cc", "#f6c23e", "#e74a3b"
		};
		public int TotalCount => Values.Sum();
	}

	public class AppointmentsByStatusChartDto
	{
		public List<string> Labels { get; set; } = new() { "Pending", "Confirmed", "Completed", "Cancelled" };
		public List<int> Values { get; set; } = new() { 0, 0, 0, 0 };
		public List<string> Colors { get; set; } = new()
		{
			"#f6c23e", "#1cc88a", "#36b9cc", "#e74a3b"
		};
		public int TotalCount => Values.Sum();
	}

	public class RecentActivityDto
	{
		public int ActivityId { get; set; }
		public string Action { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public string Details { get; set; }
		public string EntityType { get; set; }
		public int EntityId { get; set; }
		public DateTime CreatedAt { get; set; }
		public string TimeAgo => GetTimeAgo(CreatedAt);

		private string GetTimeAgo(DateTime dateTime)
		{
			var timeSpan = DateTime.Now - dateTime;
			if (timeSpan.TotalMinutes < 1) return "Vừa xong";
			if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
			if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
			if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} ngày trước";
			return dateTime.ToString("dd/MM/yyyy HH:mm");
		}
	}

	public class ExportDashboardRequest
	{
		public bool IncludeUserStats { get; set; } = true;
		public bool IncludeAppointmentStats { get; set; } = true;
		public bool IncludeCourseStats { get; set; } = true;
		public bool IncludeSurveyStats { get; set; } = true;
		public bool IncludeRecentActivities { get; set; } = true;
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string Format { get; set; } = "Excel"; // Excel, PDF, CSV
	}

	public class DashboardApiResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public DashboardViewModel Data { get; set; }
		public List<string> Errors { get; set; } = new();
	}
}