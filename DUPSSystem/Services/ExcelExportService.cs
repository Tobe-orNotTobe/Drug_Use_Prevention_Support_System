using BusinessObjects.DTOs;
using ClosedXML.Excel;
using Services.Interfaces;

namespace Services
{
	public class ExcelExportService : IExcelExportService
	{
		private readonly IDashboardService _dashboardService;

		public ExcelExportService(IDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}

		public async Task<byte[]> ExportDashboardToExcelAsync()
		{
			var dashboardData = await _dashboardService.GetDashboardStatsAsync();

			using var workbook = new XLWorkbook();

			// Summary Sheet
			CreateSummarySheet(workbook, dashboardData.Data);

			// Consultants Sheet
			await CreateConsultantsSheet(workbook);

			// Appointments Sheet
			await CreateAppointmentsSheet(workbook);

			// Users Sheet
			CreateUsersSheet(workbook, dashboardData.Data.Users);

			// Courses Sheet
			await CreateCoursesSheet(workbook);

			// Surveys Sheet
			CreateSurveysSheet(workbook, dashboardData.Data.Surveys);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public async Task<byte[]> ExportDetailedReportToExcelAsync()
		{
			var dashboardData = await _dashboardService.GetDashboardStatsAsync();

			using var workbook = new XLWorkbook();

			// Detailed Summary
			CreateDetailedSummarySheet(workbook, dashboardData.Data);

			// Monthly Trends
			await CreateMonthlyTrendsSheet(workbook);

			// Performance Metrics
			await CreatePerformanceMetricsSheet(workbook);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public async Task<byte[]> ExportConsultantReportToExcelAsync()
		{
			using var workbook = new XLWorkbook();
			await CreateConsultantsSheet(workbook);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public async Task<byte[]> ExportAppointmentReportToExcelAsync()
		{
			using var workbook = new XLWorkbook();
			await CreateAppointmentsSheet(workbook);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public async Task<byte[]> ExportCourseReportToExcelAsync()
		{
			using var workbook = new XLWorkbook();
			await CreateCoursesSheet(workbook);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public async Task<byte[]> ExportSurveyReportToExcelAsync()
		{
			var dashboardData = await _dashboardService.GetDashboardStatsAsync();

			using var workbook = new XLWorkbook();
			CreateSurveysSheet(workbook, dashboardData.Data.Surveys);

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		private void CreateSummarySheet(XLWorkbook workbook, DashboardData data)
		{
			var worksheet = workbook.Worksheets.Add("Tổng Quan");

			// Title
			worksheet.Cell("A1").Value = "BÁO CÁO TỔNG QUAN HỆ THỐNG PHÒNG NGỪA SỬ DỤNG MA TÚY";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 16;
			worksheet.Range("A1:D1").Merge();

			worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
			worksheet.Range("A2:D2").Merge();

			// Summary Statistics
			int row = 4;
			worksheet.Cell($"A{row}").Value = "THỐNG KÊ TỔNG QUAN";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"A{row}").Style.Font.FontSize = 14;

			row += 2;
			var summaryData = new[]
			{
				new { Metric = "Tổng số người dùng", Value = data.Users.TotalUsers },
				new { Metric = "Người dùng đang hoạt động", Value = data.Users.ActiveUsers },
				new { Metric = "Tổng số tư vấn viên", Value = data.Consultants.TotalConsultants },
				new { Metric = "Tư vấn viên đang hoạt động", Value = data.Consultants.ActiveConsultants },
				new { Metric = "Tổng số cuộc hẹn", Value = data.Appointments.TotalAppointments },
				new { Metric = "Tổng số khóa học", Value = data.Courses.TotalCourses },
				new { Metric = "Tổng số khảo sát", Value = data.Surveys.TotalSurveys },
				new { Metric = "Tổng số câu hỏi khảo sát", Value = data.Surveys.TotalQuestions },
				new { Metric = "Tổng số phản hồi khảo sát", Value = data.Surveys.TotalResponses }
			};

			worksheet.Cell($"A{row}").Value = "Chỉ số";
			worksheet.Cell($"B{row}").Value = "Giá trị";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"B{row}").Style.Font.Bold = true;

			row++;
			foreach (var item in summaryData)
			{
				worksheet.Cell($"A{row}").Value = item.Metric;
				worksheet.Cell($"B{row}").Value = item.Value;
				row++;
			}

			// Appointment Status Breakdown
			row += 2;
			worksheet.Cell($"A{row}").Value = "PHÂN BỔ TRẠNG THÁI CUỘC HẸN";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			row++;

			worksheet.Cell($"A{row}").Value = "Trạng thái";
			worksheet.Cell($"B{row}").Value = "Số lượng";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"B{row}").Style.Font.Bold = true;
			row++;

			worksheet.Cell($"A{row}").Value = "Chờ xác nhận";
			worksheet.Cell($"B{row}").Value = data.Appointments.StatusCounts.Pending;
			row++;
			worksheet.Cell($"A{row}").Value = "Đã xác nhận";
			worksheet.Cell($"B{row}").Value = data.Appointments.StatusCounts.Confirmed;
			row++;
			worksheet.Cell($"A{row}").Value = "Đã hoàn thành";
			worksheet.Cell($"B{row}").Value = data.Appointments.StatusCounts.Completed;
			row++;
			worksheet.Cell($"A{row}").Value = "Đã hủy";
			worksheet.Cell($"B{row}").Value = data.Appointments.StatusCounts.Cancelled;

			// Format columns
			worksheet.Column("A").Width = 35;
			worksheet.Column("B").Width = 15;

			// Add borders and styling
			var dataRange = worksheet.Range($"A4:B{row}");
			dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
		}

		private async Task CreateConsultantsSheet(XLWorkbook workbook)
		{
			var worksheet = workbook.Worksheets.Add("Tư Vấn Viên");

			// Get data
			var topConsultants = await _dashboardService.GetTopConsultantsAsync(100); // Get all

			// Headers
			worksheet.Cell("A1").Value = "DANH SÁCH TƯ VẤN VIÊN VÀ HIỆU SUẤT";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;
			worksheet.Range("A1:D1").Merge();

			int row = 3;
			worksheet.Cell($"A{row}").Value = "STT";
			worksheet.Cell($"B{row}").Value = "Tên tư vấn viên";
			worksheet.Cell($"C{row}").Value = "Chuyên môn";
			worksheet.Cell($"D{row}").Value = "Số cuộc hẹn";

			// Style headers
			var headerRange = worksheet.Range($"A{row}:D{row}");
			headerRange.Style.Font.Bold = true;
			headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

			row++;
			int index = 1;
			foreach (var consultant in topConsultants)
			{
				worksheet.Cell($"A{row}").Value = index++;
				worksheet.Cell($"B{row}").Value = consultant.ConsultantName;
				worksheet.Cell($"C{row}").Value = consultant.Expertise ?? "Chưa cập nhật";
				worksheet.Cell($"D{row}").Value = consultant.AppointmentCount;
				row++;
			}

			// Format columns
			worksheet.Column("A").Width = 8;
			worksheet.Column("B").Width = 30;
			worksheet.Column("C").Width = 25;
			worksheet.Column("D").Width = 15;

			// Add borders
			var dataRange = worksheet.Range($"A3:D{row - 1}");
			dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
		}

		private async Task CreateAppointmentsSheet(XLWorkbook workbook)
		{
			var worksheet = workbook.Worksheets.Add("Cuộc Hẹn");

			// Get data
			var upcomingAppointments = await _dashboardService.GetUpcomingAppointmentsAsync(30);
			var appointmentTrends = await _dashboardService.GetAppointmentTrendsAsync(30);

			// Upcoming Appointments
			worksheet.Cell("A1").Value = "CUỘC HẸN SẮP TỚI (30 NGÀY)";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;
			worksheet.Range("A1:E1").Merge();

			int row = 3;
			worksheet.Cell($"A{row}").Value = "STT";
			worksheet.Cell($"B{row}").Value = "Ngày & Giờ";
			worksheet.Cell($"C{row}").Value = "Người dùng";
			worksheet.Cell($"D{row}").Value = "Tư vấn viên";
			worksheet.Cell($"E{row}").Value = "Trạng thái";

			var headerRange = worksheet.Range($"A{row}:E{row}");
			headerRange.Style.Font.Bold = true;
			headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

			row++;
			int index = 1;
			foreach (var appointment in upcomingAppointments)
			{
				worksheet.Cell($"A{row}").Value = index++;
				worksheet.Cell($"B{row}").Value = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm");
				worksheet.Cell($"C{row}").Value = appointment.UserName;
				worksheet.Cell($"D{row}").Value = appointment.ConsultantName;
				worksheet.Cell($"E{row}").Value = appointment.Status;
				row++;
			}

			// Appointment Trends
			row += 2;
			worksheet.Cell($"A{row}").Value = "XU HƯỚNG CUỘC HẸN (30 NGÀY QUA)";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"A{row}").Style.Font.FontSize = 14;

			row += 2;
			worksheet.Cell($"A{row}").Value = "Ngày";
			worksheet.Cell($"B{row}").Value = "Số cuộc hẹn";

			var trendsHeaderRange = worksheet.Range($"A{row}:B{row}");
			trendsHeaderRange.Style.Font.Bold = true;
			trendsHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

			row++;
			foreach (var trend in appointmentTrends.Data)
			{
				worksheet.Cell($"A{row}").Value = trend.FormattedDate;
				worksheet.Cell($"B{row}").Value = trend.Count;
				row++;
			}

			// Format columns
			worksheet.Column("A").Width = 8;
			worksheet.Column("B").Width = 20;
			worksheet.Column("C").Width = 25;
			worksheet.Column("D").Width = 25;
			worksheet.Column("E").Width = 15;
		}

		private void CreateUsersSheet(XLWorkbook workbook, UserStats userStats)
		{
			var worksheet = workbook.Worksheets.Add("Người Dùng");

			worksheet.Cell("A1").Value = "THỐNG KÊ NGƯỜI DÙNG";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;

			// User Role Distribution
			int row = 3;
			worksheet.Cell($"A{row}").Value = "PHÂN BỔ THEO VAI TRÒ";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			row++;

			worksheet.Cell($"A{row}").Value = "Vai trò";
			worksheet.Cell($"B{row}").Value = "Số lượng";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"B{row}").Style.Font.Bold = true;
			row++;

			var roleData = new[]
			{
				new { Role = "Quản trị viên", Count = userStats.RoleCounts.Admin },
				new { Role = "Thành viên", Count = userStats.RoleCounts.Member },
				new { Role = "Tư vấn viên", Count = userStats.RoleCounts.Consultant },
				new { Role = "Quản lý", Count = userStats.RoleCounts.Manager },
				new { Role = "Nhân viên", Count = userStats.RoleCounts.Staff }
			};

			foreach (var role in roleData)
			{
				worksheet.Cell($"A{row}").Value = role.Role;
				worksheet.Cell($"B{row}").Value = role.Count;
				row++;
			}

			// Monthly Registration Trends
			row += 2;
			worksheet.Cell($"A{row}").Value = "XU HƯỚNG ĐĂNG KÝ THEO THÁNG";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			row++;

			worksheet.Cell($"A{row}").Value = "Tháng";
			worksheet.Cell($"B{row}").Value = "Số người đăng ký";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			worksheet.Cell($"B{row}").Style.Font.Bold = true;
			row++;

			foreach (var registration in userStats.MonthlyRegistrations)
			{
				worksheet.Cell($"A{row}").Value = registration.Month;
				worksheet.Cell($"B{row}").Value = registration.Count;
				row++;
			}

			worksheet.Column("A").Width = 25;
			worksheet.Column("B").Width = 15;
		}

		private async Task CreateCoursesSheet(XLWorkbook workbook)
		{
			var worksheet = workbook.Worksheets.Add("Khóa Học");

			var topCourses = await _dashboardService.GetTopCoursesAsync(100);

			worksheet.Cell("A1").Value = "THỐNG KÊ KHÓA HỌC";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;
			worksheet.Range("A1:D1").Merge();

			int row = 3;
			worksheet.Cell($"A{row}").Value = "STT";
			worksheet.Cell($"B{row}").Value = "Tên khóa học";
			worksheet.Cell($"C{row}").Value = "Đối tượng";
			worksheet.Cell($"D{row}").Value = "Số người đăng ký";

			var headerRange = worksheet.Range($"A{row}:D{row}");
			headerRange.Style.Font.Bold = true;
			headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

			row++;
			int index = 1;
			foreach (var course in topCourses)
			{
				worksheet.Cell($"A{row}").Value = index++;
				worksheet.Cell($"B{row}").Value = course.Title;
				worksheet.Cell($"C{row}").Value = course.TargetAudience ?? "Tất cả";
				worksheet.Cell($"D{row}").Value = course.EnrollmentCount;
				row++;
			}

			worksheet.Column("A").Width = 8;
			worksheet.Column("B").Width = 40;
			worksheet.Column("C").Width = 20;
			worksheet.Column("D").Width = 18;
		}

		private void CreateSurveysSheet(XLWorkbook workbook, SurveyStats surveyStats)
		{
			var worksheet = workbook.Worksheets.Add("Khảo Sát");

			worksheet.Cell("A1").Value = "THỐNG KÊ KHẢO SÁT";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;

			int row = 3;
			worksheet.Cell($"A{row}").Value = "Tổng số khảo sát:";
			worksheet.Cell($"B{row}").Value = surveyStats.TotalSurveys;
			row++;
			worksheet.Cell($"A{row}").Value = "Tổng số câu hỏi:";
			worksheet.Cell($"B{row}").Value = surveyStats.TotalQuestions;
			row++;
			worksheet.Cell($"A{row}").Value = "Tổng số phản hồi:";
			worksheet.Cell($"B{row}").Value = surveyStats.TotalResponses;

			// Popular Surveys
			row += 3;
			worksheet.Cell($"A{row}").Value = "KHẢO SÁT PHỔ BIẾN";
			worksheet.Cell($"A{row}").Style.Font.Bold = true;
			row++;

			worksheet.Cell($"A{row}").Value = "STT";
			worksheet.Cell($"B{row}").Value = "Tên khảo sát";
			worksheet.Cell($"C{row}").Value = "Số người tham gia";

			var headerRange = worksheet.Range($"A{row}:C{row}");
			headerRange.Style.Font.Bold = true;
			headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

			row++;
			int index = 1;
			foreach (var survey in surveyStats.PopularSurveys)
			{
				worksheet.Cell($"A{row}").Value = index++;
				worksheet.Cell($"B{row}").Value = survey.Name;
				worksheet.Cell($"C{row}").Value = survey.ParticipantCount;
				row++;
			}

			worksheet.Column("A").Width = 8;
			worksheet.Column("B").Width = 40;
			worksheet.Column("C").Width = 18;
		}

		private void CreateDetailedSummarySheet(XLWorkbook workbook, DashboardData data)
		{
			var worksheet = workbook.Worksheets.Add("Báo Cáo Chi Tiết");

			worksheet.Cell("A1").Value = "BÁO CÁO CHI TIẾT HỆ THỐNG";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 16;
			worksheet.Range("A1:F1").Merge();

			// Add comprehensive metrics, KPIs, and analysis here
			// This would include calculated metrics like conversion rates, 
			// efficiency metrics, growth rates, etc.
		}

		private async Task CreateMonthlyTrendsSheet(XLWorkbook workbook)
		{
			var worksheet = workbook.Worksheets.Add("Xu Hướng Tháng");

			var monthlyRegistrations = await _dashboardService.GetMonthlyUserRegistrationsAsync(12);

			// Create charts and trend analysis
			worksheet.Cell("A1").Value = "XU HƯỚNG THEO THÁNG";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;

			// Add monthly trends data and analysis
		}

		private async Task CreatePerformanceMetricsSheet(XLWorkbook workbook)
		{
			var worksheet = workbook.Worksheets.Add("Chỉ Số Hiệu Suất");

			// Add performance metrics and KPIs
			worksheet.Cell("A1").Value = "CHỈ SỐ HIỆU SUẤT HOẠT ĐỘNG";
			worksheet.Cell("A1").Style.Font.Bold = true;
			worksheet.Cell("A1").Style.Font.FontSize = 14;
		}
	}
}
