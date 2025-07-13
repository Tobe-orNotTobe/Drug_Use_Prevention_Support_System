using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IDashboardRepository _dashboardRepository;

		public DashboardService(IDashboardRepository dashboardRepository)
		{
			_dashboardRepository = dashboardRepository;
		}

		public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
		{
			try
			{
				var consultantStats = new ConsultantStats
				{
					TotalConsultants = await _dashboardRepository.GetTotalConsultantsAsync(),
					ActiveConsultants = await _dashboardRepository.GetActiveConsultantsAsync(),
					TopConsultants = await _dashboardRepository.GetTopConsultantsAsync(5)
				};

				var appointmentStats = new AppointmentStats
				{
					TotalAppointments = await _dashboardRepository.GetTotalAppointmentsAsync(),
					StatusCounts = await _dashboardRepository.GetAppointmentStatusCountsAsync(),
					AppointmentsByDate = await _dashboardRepository.GetAppointmentsByDateAsync(30),
					AppointmentsByConsultant = await _dashboardRepository.GetAppointmentsByConsultantAsync(),
					UpcomingAppointments = await _dashboardRepository.GetUpcomingAppointmentsAsync(7)
				};

				var userStats = new UserStats
				{
					TotalUsers = await _dashboardRepository.GetTotalUsersAsync(),
					ActiveUsers = await _dashboardRepository.GetActiveUsersAsync(),
					RoleCounts = await _dashboardRepository.GetUserRoleCountsAsync(),
					MonthlyRegistrations = await _dashboardRepository.GetMonthlyUserRegistrationsAsync(12)
				};

				var courseStats = new CourseStats
				{
					TotalCourses = await _dashboardRepository.GetTotalCoursesAsync(),
					CoursesByAudience = await _dashboardRepository.GetCoursesByAudienceAsync(),
					TopCourses = await _dashboardRepository.GetTopCoursesAsync(5),
					CourseEnrollments = await _dashboardRepository.GetCourseEnrollmentsAsync()
				};

				var surveyStats = new SurveyStats
				{
					TotalSurveys = await _dashboardRepository.GetTotalSurveysAsync(),
					TotalQuestions = await _dashboardRepository.GetTotalSurveyQuestionsAsync(),
					TotalResponses = await _dashboardRepository.GetTotalSurveyResponsesAsync(),
					SurveyParticipations = await _dashboardRepository.GetSurveyParticipationsAsync(),
					PopularSurveys = await _dashboardRepository.GetPopularSurveysAsync(5)
				};

				return new DashboardStatsResponse
				{
					Success = true,
					Message = "Dashboard statistics retrieved successfully",
					Data = new DashboardData
					{
						Consultants = consultantStats,
						Appointments = appointmentStats,
						Users = userStats,
						Courses = courseStats,
						Surveys = surveyStats
					}
				};
			}
			catch (Exception ex)
			{
				return new DashboardStatsResponse
				{
					Success = false,
					Message = $"Error retrieving dashboard statistics: {ex.Message}"
				};
			}
		}

		public async Task<ConsultantAvailabilityResponse> GetConsultantAvailabilityAsync()
		{
			try
			{
				var consultants = await _dashboardRepository.GetTopConsultantsAsync(100); 
				var availability = consultants.Select(c => new ConsultantAvailabilityItem
				{
					ConsultantId = c.ConsultantId,
					ConsultantName = c.ConsultantName,
					Expertise = c.Expertise,
					IsActive = true 
				}).ToList();

				return new ConsultantAvailabilityResponse
				{
					Success = true,
					Data = availability
				};
			}
			catch (Exception ex)
			{
				return new ConsultantAvailabilityResponse
				{
					Success = false,
					Data = new List<ConsultantAvailabilityItem>()
				};
			}
		}

		public async Task<AppointmentTrendsResponse> GetAppointmentTrendsAsync(int days = 30)
		{
			try
			{
				var trends = await _dashboardRepository.GetAppointmentsByDateAsync(days);
				var trendData = trends.Select(t => new DailyAppointmentTrend
				{
					Date = DateTime.Parse(t.Date),
					Count = t.Count,
					FormattedDate = DateTime.Parse(t.Date).ToString("MMM dd")
				}).ToList();

				return new AppointmentTrendsResponse
				{
					Success = true,
					Data = trendData
				};
			}
			catch (Exception ex)
			{
				return new AppointmentTrendsResponse
				{
					Success = false,
					Data = new List<DailyAppointmentTrend>()
				};
			}
		}

		public async Task<List<ConsultantAppointmentCount>> GetTopConsultantsAsync(int topN = 5)
		{
			return await _dashboardRepository.GetTopConsultantsAsync(topN);
		}

		public async Task<List<UpcomingAppointment>> GetUpcomingAppointmentsAsync(int days = 7)
		{
			return await _dashboardRepository.GetUpcomingAppointmentsAsync(days);
		}

		public async Task<List<PopularCourse>> GetTopCoursesAsync(int topN = 5)
		{
			return await _dashboardRepository.GetTopCoursesAsync(topN);
		}

		public async Task<List<PopularSurvey>> GetPopularSurveysAsync(int topN = 5)
		{
			return await _dashboardRepository.GetPopularSurveysAsync(topN);
		}

		public async Task<List<MonthlyUserRegistration>> GetMonthlyUserRegistrationsAsync(int months = 12)
		{
			return await _dashboardRepository.GetMonthlyUserRegistrationsAsync(months);
		}
	}
}