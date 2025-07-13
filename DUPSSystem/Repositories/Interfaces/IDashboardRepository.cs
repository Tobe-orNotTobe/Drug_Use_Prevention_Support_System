using BusinessObjects.DTOs;

namespace Repositories.Interfaces
{
	public interface IDashboardRepository
	{
		// Consultant Stats
		Task<int> GetTotalConsultantsAsync();
		Task<int> GetActiveConsultantsAsync();
		Task<List<ConsultantAppointmentCount>> GetTopConsultantsAsync(int topN);

		// Appointment Stats
		Task<int> GetTotalAppointmentsAsync();
		Task<AppointmentStatusCount> GetAppointmentStatusCountsAsync();
		Task<List<AppointmentByDate>> GetAppointmentsByDateAsync(int days);
		Task<List<AppointmentByConsultant>> GetAppointmentsByConsultantAsync();
		Task<List<UpcomingAppointment>> GetUpcomingAppointmentsAsync(int days);

		// User Stats
		Task<int> GetTotalUsersAsync();
		Task<int> GetActiveUsersAsync();
		Task<UserRoleCount> GetUserRoleCountsAsync();
		Task<List<MonthlyUserRegistration>> GetMonthlyUserRegistrationsAsync(int months);

		// Course Stats
		Task<int> GetTotalCoursesAsync();
		Task<List<CourseByAudience>> GetCoursesByAudienceAsync();
		Task<List<PopularCourse>> GetTopCoursesAsync(int topN);
		Task<List<CourseEnrollment>> GetCourseEnrollmentsAsync();

		// Survey Stats
		Task<int> GetTotalSurveysAsync();
		Task<int> GetTotalSurveyQuestionsAsync();
		Task<int> GetTotalSurveyResponsesAsync();
		Task<List<SurveyParticipation>> GetSurveyParticipationsAsync();
		Task<List<PopularSurvey>> GetPopularSurveysAsync(int topN);
	}
}