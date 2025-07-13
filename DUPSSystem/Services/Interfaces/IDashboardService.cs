using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IDashboardService
	{
		Task<DashboardStatsResponse> GetDashboardStatsAsync();
		Task<ConsultantAvailabilityResponse> GetConsultantAvailabilityAsync();
		Task<AppointmentTrendsResponse> GetAppointmentTrendsAsync(int days = 30);
		Task<List<ConsultantAppointmentCount>> GetTopConsultantsAsync(int topN = 5);
		Task<List<UpcomingAppointment>> GetUpcomingAppointmentsAsync(int days = 7);
		Task<List<PopularCourse>> GetTopCoursesAsync(int topN = 5);
		Task<List<PopularSurvey>> GetPopularSurveysAsync(int topN = 5);
		Task<List<MonthlyUserRegistration>> GetMonthlyUserRegistrationsAsync(int months = 12);
	}
}
