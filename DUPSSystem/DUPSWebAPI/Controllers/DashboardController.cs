using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] 
	public class DashboardController : ODataController
	{
		private readonly IDashboardService _dashboardService;

		public DashboardController(IDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}

		/// <summary>
		/// Get comprehensive dashboard statistics
		/// </summary>
		/// <returns>Complete dashboard data including all statistics</returns>
		[HttpGet("stats")]
		public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats()
		{
			try
			{
				var result = await _dashboardService.GetDashboardStatsAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new DashboardStatsResponse
				{
					Success = false,
					Message = $"Error retrieving dashboard stats: {ex.Message}"
				});
			}
		}

		/// <summary>
		/// Get consultant availability information
		/// </summary>
		/// <returns>List of consultants with their availability status</returns>
		[HttpGet("consultants/availability")]
		public async Task<ActionResult<ConsultantAvailabilityResponse>> GetConsultantAvailability()
		{
			try
			{
				var result = await _dashboardService.GetConsultantAvailabilityAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new ConsultantAvailabilityResponse
				{
					Success = false,
					Data = new List<ConsultantAvailabilityItem>()
				});
			}
		}

		/// <summary>
		/// Get appointment trends over specified number of days
		/// </summary>
		/// <param name="days">Number of days to look back (default: 30)</param>
		/// <returns>Daily appointment counts for the specified period</returns>
		[HttpGet("appointments/trends")]
		public async Task<ActionResult<AppointmentTrendsResponse>> GetAppointmentTrends([FromQuery] int days = 30)
		{
			try
			{
				var result = await _dashboardService.GetAppointmentTrendsAsync(days);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new AppointmentTrendsResponse
				{
					Success = false,
					Data = new List<DailyAppointmentTrend>()
				});
			}
		}

		/// <summary>
		/// Get top consultants by appointment count
		/// </summary>
		/// <param name="topN">Number of top consultants to return (default: 5)</param>
		/// <returns>Top consultants ranked by appointment count</returns>
		[HttpGet("consultants/top")]
		public async Task<ActionResult<List<ConsultantAppointmentCount>>> GetTopConsultants([FromQuery] int topN = 5)
		{
			try
			{
				var result = await _dashboardService.GetTopConsultantsAsync(topN);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new List<ConsultantAppointmentCount>());
			}
		}

		/// <summary>
		/// Get upcoming appointments within specified number of days
		/// </summary>
		/// <param name="days">Number of days to look ahead (default: 7)</param>
		/// <returns>List of upcoming appointments</returns>
		[HttpGet("appointments/upcoming")]
		public async Task<ActionResult<List<UpcomingAppointment>>> GetUpcomingAppointments([FromQuery] int days = 7)
		{
			try
			{
				var result = await _dashboardService.GetUpcomingAppointmentsAsync(days);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new List<UpcomingAppointment>());
			}
		}

		/// <summary>
		/// Get top courses by enrollment count
		/// </summary>
		/// <param name="topN">Number of top courses to return (default: 5)</param>
		/// <returns>Top courses ranked by enrollment</returns>
		[HttpGet("courses/top")]
		public async Task<ActionResult<List<PopularCourse>>> GetTopCourses([FromQuery] int topN = 5)
		{
			try
			{
				var result = await _dashboardService.GetTopCoursesAsync(topN);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new List<PopularCourse>());
			}
		}

		/// <summary>
		/// Get popular surveys by participation count
		/// </summary>
		/// <param name="topN">Number of popular surveys to return (default: 5)</param>
		/// <returns>Popular surveys ranked by participation</returns>
		[HttpGet("surveys/popular")]
		public async Task<ActionResult<List<PopularSurvey>>> GetPopularSurveys([FromQuery] int topN = 5)
		{
			try
			{
				var result = await _dashboardService.GetPopularSurveysAsync(topN);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new List<PopularSurvey>());
			}
		}

		/// <summary>
		/// Get monthly user registration statistics
		/// </summary>
		/// <param name="months">Number of months to look back (default: 12)</param>
		/// <returns>Monthly user registration counts</returns>
		[HttpGet("users/registrations")]
		public async Task<ActionResult<List<MonthlyUserRegistration>>> GetMonthlyUserRegistrations([FromQuery] int months = 12)
		{
			try
			{
				var result = await _dashboardService.GetMonthlyUserRegistrationsAsync(months);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new List<MonthlyUserRegistration>());
			}
		}
	}
}