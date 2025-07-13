using BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class DashboardDAO
	{
		// Consultant Statistics
		public static int GetTotalConsultants()
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants.Count();
		}

		public static int GetActiveConsultants()
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants
				.Include(c => c.User)
				.Count(c => c.User.IsActive);
		}

		public static List<ConsultantAppointmentCount> GetTopConsultants(int topN)
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants
				.Include(c => c.User)
				.Include(c => c.Appointments)
				.Select(c => new ConsultantAppointmentCount
				{
					ConsultantId = c.ConsultantId,
					ConsultantName = c.User.FullName,
					Expertise = c.Expertise,
					AppointmentCount = c.Appointments.Count
				})
				.OrderByDescending(x => x.AppointmentCount)
				.Take(topN)
				.ToList();
		}

		// Appointment Statistics
		public static int GetTotalAppointments()
		{
			using var db = new DrugPreventionDbContext();
			return db.Appointments.Count();
		}

		public static AppointmentStatusCount GetAppointmentStatusCounts()
		{
			using var db = new DrugPreventionDbContext();
			var statusCounts = db.Appointments
				.GroupBy(a => a.Status)
				.Select(g => new { Status = g.Key, Count = g.Count() })
				.ToList();

			return new AppointmentStatusCount
			{
				Pending = statusCounts.FirstOrDefault(x => x.Status == "Pending")?.Count ?? 0,
				Confirmed = statusCounts.FirstOrDefault(x => x.Status == "Confirmed")?.Count ?? 0,
				Completed = statusCounts.FirstOrDefault(x => x.Status == "Completed")?.Count ?? 0,
				Cancelled = statusCounts.FirstOrDefault(x => x.Status == "Cancelled")?.Count ?? 0
			};
		}

		public static List<AppointmentByDate> GetAppointmentsByDate(int days)
		{
			using var db = new DrugPreventionDbContext();
			var startDate = DateTime.Now.AddDays(-days).Date;

			// First get the grouped data from database
			var groupedData = db.Appointments
				.Where(a => a.AppointmentDate >= startDate)
				.GroupBy(a => a.AppointmentDate.Date)
				.Select(g => new
				{
					Date = g.Key,
					Count = g.Count()
				})
				.ToList(); // Execute query first

			// Then format the date string in memory
			return groupedData
				.Select(g => new AppointmentByDate
				{
					Date = g.Date.ToString("yyyy-MM-dd"),
					Count = g.Count
				})
				.OrderBy(x => x.Date)
				.ToList();
		}

		public static List<AppointmentByConsultant> GetAppointmentsByConsultant()
		{
			using var db = new DrugPreventionDbContext();
			return db.Appointments
				.Include(a => a.Consultant)
				.ThenInclude(c => c.User)
				.GroupBy(a => a.Consultant.User.FullName)
				.Select(g => new AppointmentByConsultant
				{
					ConsultantName = g.Key,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToList();
		}

		public static List<UpcomingAppointment> GetUpcomingAppointments(int days)
		{
			using var db = new DrugPreventionDbContext();
			var endDate = DateTime.Now.AddDays(days);
			return db.Appointments
				.Include(a => a.User)
				.Include(a => a.Consultant)
				.ThenInclude(c => c.User)
				.Where(a => a.AppointmentDate >= DateTime.Now && a.AppointmentDate <= endDate)
				.Where(a => a.Status == "Pending" || a.Status == "Confirmed")
				.Select(a => new UpcomingAppointment
				{
					AppointmentId = a.AppointmentId,
					UserName = a.User.FullName,
					ConsultantName = a.Consultant.User.FullName,
					AppointmentDate = a.AppointmentDate,
					Status = a.Status
				})
				.OrderBy(a => a.AppointmentDate)
				.ToList();
		}

		// User Statistics
		public static int GetTotalUsers()
		{
			using var db = new DrugPreventionDbContext();
			return db.Users.Count();
		}

		public static int GetActiveUsers()
		{
			using var db = new DrugPreventionDbContext();
			return db.Users.Count(u => u.IsActive);
		}

		public static UserRoleCount GetUserRoleCounts()
		{
			using var db = new DrugPreventionDbContext();
			var roleCounts = db.Users
				.Include(u => u.Roles)
				.SelectMany(u => u.Roles)
				.GroupBy(r => r.RoleName)
				.Select(g => new { RoleName = g.Key, Count = g.Count() })
				.ToList();

			return new UserRoleCount
			{
				Admin = roleCounts.FirstOrDefault(x => x.RoleName == "Admin")?.Count ?? 0,
				Member = roleCounts.FirstOrDefault(x => x.RoleName == "Member")?.Count ?? 0,
				Consultant = roleCounts.FirstOrDefault(x => x.RoleName == "Consultant")?.Count ?? 0,
				Manager = roleCounts.FirstOrDefault(x => x.RoleName == "Manager")?.Count ?? 0,
				Staff = roleCounts.FirstOrDefault(x => x.RoleName == "Staff")?.Count ?? 0
			};
		}

		public static List<MonthlyUserRegistration> GetMonthlyUserRegistrations(int months)
		{
			using var db = new DrugPreventionDbContext();
			var startDate = DateTime.Now.AddMonths(-months);

			// First get the grouped data from database
			var groupedData = db.Users
				.Where(u => u.CreatedAt >= startDate)
				.GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
				.Select(g => new
				{
					Year = g.Key.Year,
					Month = g.Key.Month,
					Count = g.Count()
				})
				.ToList(); // Execute query first

			// Then format the month string in memory
			return groupedData
				.Select(g => new MonthlyUserRegistration
				{
					Month = $"{g.Year}-{g.Month:D2}",
					Count = g.Count
				})
				.OrderBy(x => x.Month)
				.ToList();
		}

		// Course Statistics
		public static int GetTotalCourses()
		{
			using var db = new DrugPreventionDbContext();
			return db.Courses.Count();
		}

		public static List<CourseByAudience> GetCoursesByAudience()
		{
			using var db = new DrugPreventionDbContext();
			return db.Courses
				.Where(c => !string.IsNullOrEmpty(c.TargetAudience))
				.GroupBy(c => c.TargetAudience)
				.Select(g => new CourseByAudience
				{
					TargetAudience = g.Key!,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToList();
		}

		public static List<PopularCourse> GetTopCourses(int topN)
		{
			using var db = new DrugPreventionDbContext();
			return db.Courses
				.Include(c => c.UserCourses)
				.Select(c => new PopularCourse
				{
					CourseId = c.CourseId,
					Title = c.Title,
					TargetAudience = c.TargetAudience,
					EnrollmentCount = c.UserCourses.Count
				})
				.OrderByDescending(x => x.EnrollmentCount)
				.Take(topN)
				.ToList();
		}

		public static List<CourseEnrollment> GetCourseEnrollments()
		{
			using var db = new DrugPreventionDbContext();
			return db.Courses
				.Include(c => c.UserCourses)
				.Select(c => new CourseEnrollment
				{
					CourseTitle = c.Title,
					EnrollmentCount = c.UserCourses.Count
				})
				.OrderByDescending(x => x.EnrollmentCount)
				.ToList();
		}

		// Survey Statistics
		public static int GetTotalSurveys()
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys.Count();
		}

		public static int GetTotalSurveyQuestions()
		{
			using var db = new DrugPreventionDbContext();
			return db.SurveyQuestions.Count();
		}

		public static int GetTotalSurveyResponses()
		{
			using var db = new DrugPreventionDbContext();
			return db.UserSurveyResults.Count();
		}

		public static List<SurveyParticipation> GetSurveyParticipations()
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.UserSurveyResults)
				.Select(s => new SurveyParticipation
				{
					SurveyName = s.Name,
					ParticipantCount = s.UserSurveyResults.Count
				})
				.OrderByDescending(x => x.ParticipantCount)
				.ToList();
		}

		public static List<PopularSurvey> GetPopularSurveys(int topN)
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.UserSurveyResults)
				.Select(s => new PopularSurvey
				{
					SurveyId = s.SurveyId,
					Name = s.Name,
					ParticipantCount = s.UserSurveyResults.Count
				})
				.OrderByDescending(x => x.ParticipantCount)
				.Take(topN)
				.ToList();
		}

		// Additional helper methods for specific dashboard needs
		public static int GetTotalActiveConsultants()
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants
				.Include(c => c.User)
				.Count(c => c.User.IsActive);
		}

		public static List<ConsultantAvailabilityItem> GetConsultantAvailability()
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants
				.Include(c => c.User)
				.Select(c => new ConsultantAvailabilityItem
				{
					ConsultantId = c.ConsultantId,
					ConsultantName = c.User.FullName,
					Expertise = c.Expertise,
					IsActive = c.User.IsActive
				})
				.ToList();
		}

		// Get appointment counts by status for last N days
		public static Dictionary<string, int> GetAppointmentStatusCountsByPeriod(int days)
		{
			using var db = new DrugPreventionDbContext();
			var startDate = DateTime.Now.AddDays(-days);

			return db.Appointments
				.Where(a => a.CreatedAt >= startDate)
				.GroupBy(a => a.Status)
				.ToDictionary(g => g.Key, g => g.Count());
		}

		// Get course enrollment trends by month
		public static List<MonthlyUserRegistration> GetCourseEnrollmentTrends(int months)
		{
			using var db = new DrugPreventionDbContext();
			var startDate = DateTime.Now.AddMonths(-months);

			// First get the grouped data from database
			var groupedData = db.UserCourses
				.Where(uc => uc.RegisteredAt >= startDate)
				.GroupBy(uc => new { uc.RegisteredAt.Year, uc.RegisteredAt.Month })
				.Select(g => new
				{
					Year = g.Key.Year,
					Month = g.Key.Month,
					Count = g.Count()
				})
				.ToList(); // Execute query first

			// Then format the month string in memory
			return groupedData
				.Select(g => new MonthlyUserRegistration
				{
					Month = $"{g.Year}-{g.Month:D2}",
					Count = g.Count
				})
				.OrderBy(x => x.Month)
				.ToList();
		}

		// Get survey completion rates
		public static List<SurveyParticipation> GetSurveyCompletionRates()
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.UserSurveyResults)
				.Select(s => new SurveyParticipation
				{
					SurveyName = s.Name,
					ParticipantCount = s.UserSurveyResults.Count
				})
				.OrderByDescending(x => x.ParticipantCount)
				.ToList();
		}
	}
}