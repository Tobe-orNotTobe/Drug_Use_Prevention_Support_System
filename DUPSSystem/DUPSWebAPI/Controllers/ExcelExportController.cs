using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ExcelExportController : ControllerBase
	{
		private readonly IExcelExportService _excelExportService;

		public ExcelExportController(IExcelExportService excelExportService)
		{
			_excelExportService = excelExportService;
		}

		/// <summary>
		/// Export complete dashboard report to Excel
		/// </summary>
		[HttpGet("dashboard")]
		public async Task<IActionResult> ExportDashboard()
		{
			try
			{
				var excelData = await _excelExportService.ExportDashboardToExcelAsync();
				var fileName = $"Dashboard_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting dashboard: {ex.Message}" });
			}
		}

		/// <summary>
		/// Export detailed dashboard report to Excel
		/// </summary>
		[HttpGet("detailed-report")]
		public async Task<IActionResult> ExportDetailedReport()
		{
			try
			{
				var excelData = await _excelExportService.ExportDetailedReportToExcelAsync();
				var fileName = $"Detailed_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting detailed report: {ex.Message}" });
			}
		}

		/// <summary>
		/// Export consultant performance report to Excel
		/// </summary>
		[HttpGet("consultants")]
		public async Task<IActionResult> ExportConsultantReport()
		{
			try
			{
				var excelData = await _excelExportService.ExportConsultantReportToExcelAsync();
				var fileName = $"Consultant_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting consultant report: {ex.Message}" });
			}
		}

		/// <summary>
		/// Export appointment analysis report to Excel
		/// </summary>
		[HttpGet("appointments")]
		public async Task<IActionResult> ExportAppointmentReport()
		{
			try
			{
				var excelData = await _excelExportService.ExportAppointmentReportToExcelAsync();
				var fileName = $"Appointment_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting appointment report: {ex.Message}" });
			}
		}

		/// <summary>
		/// Export course statistics report to Excel
		/// </summary>
		[HttpGet("courses")]
		public async Task<IActionResult> ExportCourseReport()
		{
			try
			{
				var excelData = await _excelExportService.ExportCourseReportToExcelAsync();
				var fileName = $"Course_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting course report: {ex.Message}" });
			}
		}

		/// <summary>
		/// Export survey analytics report to Excel
		/// </summary>
		[HttpGet("surveys")]
		public async Task<IActionResult> ExportSurveyReport()
		{
			try
			{
				var excelData = await _excelExportService.ExportSurveyReportToExcelAsync();
				var fileName = $"Survey_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

				return File(excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error exporting survey report: {ex.Message}" });
			}
		}
	}
}