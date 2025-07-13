namespace BusinessObjects.DTOs
{
	public class DashboardStatsResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public DashboardData Data { get; set; } = new DashboardData();
	}

	public class DashboardData
	{
		public ConsultantStats Consultants { get; set; } = new ConsultantStats();
		public AppointmentStats Appointments { get; set; } = new AppointmentStats();
		public UserStats Users { get; set; } = new UserStats();
		public CourseStats Courses { get; set; } = new CourseStats();
		public SurveyStats Surveys { get; set; } = new SurveyStats();
	}

	// Consultant Statistics
	public class ConsultantStats
	{
		public int TotalConsultants { get; set; }
		public int ActiveConsultants { get; set; }
		public List<ConsultantAppointmentCount> TopConsultants { get; set; } = new List<ConsultantAppointmentCount>();
	}

	public class ConsultantAppointmentCount
	{
		public int ConsultantId { get; set; }
		public string ConsultantName { get; set; } = null!;
		public string? Expertise { get; set; }
		public int AppointmentCount { get; set; }
	}

	// Appointment Statistics
	public class AppointmentStats
	{
		public int TotalAppointments { get; set; }
		public AppointmentStatusCount StatusCounts { get; set; } = new AppointmentStatusCount();
		public List<AppointmentByDate> AppointmentsByDate { get; set; } = new List<AppointmentByDate>();
		public List<AppointmentByConsultant> AppointmentsByConsultant { get; set; } = new List<AppointmentByConsultant>();
		public List<UpcomingAppointment> UpcomingAppointments { get; set; } = new List<UpcomingAppointment>();
	}

	public class AppointmentStatusCount
	{
		public int Pending { get; set; }
		public int Confirmed { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
	}

	public class AppointmentByDate
	{
		public string Date { get; set; } = null!;
		public int Count { get; set; }
	}

	public class AppointmentByConsultant
	{
		public string ConsultantName { get; set; } = null!;
		public int Count { get; set; }
	}

	public class UpcomingAppointment
	{
		public int AppointmentId { get; set; }
		public string UserName { get; set; } = null!;
		public string ConsultantName { get; set; } = null!;
		public DateTime AppointmentDate { get; set; }
		public string Status { get; set; } = null!;
	}

	// User Statistics
	public class UserStats
	{
		public int TotalUsers { get; set; }
		public int ActiveUsers { get; set; }
		public UserRoleCount RoleCounts { get; set; } = new UserRoleCount();
		public List<MonthlyUserRegistration> MonthlyRegistrations { get; set; } = new List<MonthlyUserRegistration>();
	}

	public class UserRoleCount
	{
		public int Admin { get; set; }
		public int Member { get; set; }
		public int Consultant { get; set; }
		public int Manager { get; set; }
		public int Staff { get; set; }
	}

	public class MonthlyUserRegistration
	{
		public string Month { get; set; } = null!;
		public int Count { get; set; }
	}

	// Course Statistics
	public class CourseStats
	{
		public int TotalCourses { get; set; }
		public List<CourseByAudience> CoursesByAudience { get; set; } = new List<CourseByAudience>();
		public List<PopularCourse> TopCourses { get; set; } = new List<PopularCourse>();
		public List<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
	}

	public class CourseByAudience
	{
		public string TargetAudience { get; set; } = null!;
		public int Count { get; set; }
	}

	public class PopularCourse
	{
		public int CourseId { get; set; }
		public string Title { get; set; } = null!;
		public string? TargetAudience { get; set; }
		public int EnrollmentCount { get; set; }
	}

	public class CourseEnrollment
	{
		public string CourseTitle { get; set; } = null!;
		public int EnrollmentCount { get; set; }
	}

	// Survey Statistics
	public class SurveyStats
	{
		public int TotalSurveys { get; set; }
		public int TotalQuestions { get; set; }
		public int TotalResponses { get; set; }
		public List<SurveyParticipation> SurveyParticipations { get; set; } = new List<SurveyParticipation>();
		public List<PopularSurvey> PopularSurveys { get; set; } = new List<PopularSurvey>();
	}

	public class SurveyParticipation
	{
		public string SurveyName { get; set; } = null!;
		public int ParticipantCount { get; set; }
	}

	public class PopularSurvey
	{
		public int SurveyId { get; set; }
		public string Name { get; set; } = null!;
		public int ParticipantCount { get; set; }
	}

	// Additional specific request DTOs
	public class ConsultantAvailabilityResponse
	{
		public bool Success { get; set; }
		public List<ConsultantAvailabilityItem> Data { get; set; } = new List<ConsultantAvailabilityItem>();
	}

	public class ConsultantAvailabilityItem
	{
		public int ConsultantId { get; set; }
		public string ConsultantName { get; set; } = null!;
		public string? Expertise { get; set; }
		public bool IsActive { get; set; }
	}

	public class AppointmentTrendsResponse
	{
		public bool Success { get; set; }
		public List<DailyAppointmentTrend> Data { get; set; } = new List<DailyAppointmentTrend>();
	}

	public class DailyAppointmentTrend
	{
		public DateTime Date { get; set; }
		public int Count { get; set; }
		public string FormattedDate { get; set; } = null!;
	}
}