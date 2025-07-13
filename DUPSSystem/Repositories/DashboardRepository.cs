using BusinessObjects.DTOs;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class DashboardRepository : IDashboardRepository
	{
		// Consultant Stats
		public async Task<int> GetTotalConsultantsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalConsultants());
		}

		public async Task<int> GetActiveConsultantsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetActiveConsultants());
		}

		public async Task<List<ConsultantAppointmentCount>> GetTopConsultantsAsync(int topN)
		{
			return await Task.FromResult(DashboardDAO.GetTopConsultants(topN));
		}

		// Appointment Stats
		public async Task<int> GetTotalAppointmentsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalAppointments());
		}

		public async Task<AppointmentStatusCount> GetAppointmentStatusCountsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetAppointmentStatusCounts());
		}

		public async Task<List<AppointmentByDate>> GetAppointmentsByDateAsync(int days)
		{
			return await Task.FromResult(DashboardDAO.GetAppointmentsByDate(days));
		}

		public async Task<List<AppointmentByConsultant>> GetAppointmentsByConsultantAsync()
		{
			return await Task.FromResult(DashboardDAO.GetAppointmentsByConsultant());
		}

		public async Task<List<UpcomingAppointment>> GetUpcomingAppointmentsAsync(int days)
		{
			return await Task.FromResult(DashboardDAO.GetUpcomingAppointments(days));
		}

		// User Stats
		public async Task<int> GetTotalUsersAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalUsers());
		}

		public async Task<int> GetActiveUsersAsync()
		{
			return await Task.FromResult(DashboardDAO.GetActiveUsers());
		}

		public async Task<UserRoleCount> GetUserRoleCountsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetUserRoleCounts());
		}

		public async Task<List<MonthlyUserRegistration>> GetMonthlyUserRegistrationsAsync(int months)
		{
			return await Task.FromResult(DashboardDAO.GetMonthlyUserRegistrations(months));
		}

		// Course Stats
		public async Task<int> GetTotalCoursesAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalCourses());
		}

		public async Task<List<CourseByAudience>> GetCoursesByAudienceAsync()
		{
			return await Task.FromResult(DashboardDAO.GetCoursesByAudience());
		}

		public async Task<List<PopularCourse>> GetTopCoursesAsync(int topN)
		{
			return await Task.FromResult(DashboardDAO.GetTopCourses(topN));
		}

		public async Task<List<CourseEnrollment>> GetCourseEnrollmentsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetCourseEnrollments());
		}

		// Survey Stats
		public async Task<int> GetTotalSurveysAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalSurveys());
		}

		public async Task<int> GetTotalSurveyQuestionsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalSurveyQuestions());
		}

		public async Task<int> GetTotalSurveyResponsesAsync()
		{
			return await Task.FromResult(DashboardDAO.GetTotalSurveyResponses());
		}

		public async Task<List<SurveyParticipation>> GetSurveyParticipationsAsync()
		{
			return await Task.FromResult(DashboardDAO.GetSurveyParticipations());
		}

		public async Task<List<PopularSurvey>> GetPopularSurveysAsync(int topN)
		{
			return await Task.FromResult(DashboardDAO.GetPopularSurveys(topN));
		}
	}
}