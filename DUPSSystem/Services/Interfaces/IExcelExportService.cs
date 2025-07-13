namespace Services.Interfaces
{
	public interface IExcelExportService
	{
		Task<byte[]> ExportDashboardToExcelAsync();
		Task<byte[]> ExportDetailedReportToExcelAsync();
		Task<byte[]> ExportConsultantReportToExcelAsync();
		Task<byte[]> ExportAppointmentReportToExcelAsync();
		Task<byte[]> ExportCourseReportToExcelAsync();
		Task<byte[]> ExportSurveyReportToExcelAsync();
	}
}
