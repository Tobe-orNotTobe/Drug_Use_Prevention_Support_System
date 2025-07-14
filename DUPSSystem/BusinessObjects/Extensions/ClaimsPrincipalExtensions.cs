using BusinessObjects.Constants;
using System.Security.Claims;

namespace BusinessObjects.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		#region Basic User Info
		public static int GetUserId(this ClaimsPrincipal user)
		{
			var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
			return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
		}

		public static string GetUserEmail(this ClaimsPrincipal user)
		{
			return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
		}

		public static string GetUserFullName(this ClaimsPrincipal user)
		{
			return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
		}

		public static bool IsAuthenticated(this ClaimsPrincipal user)
		{
			return user.Identity?.IsAuthenticated == true;
		}
		#endregion

		#region Role Checking Methods
		public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
		{
			return roles.Any(role => user.IsInRole(role));
		}

		public static bool IsGuest(this ClaimsPrincipal user)
		{
			return !user.IsAuthenticated();
		}

		public static bool IsMember(this ClaimsPrincipal user)
		{
			return user.IsInRole(Roles.Member);
		}

		public static bool IsStaff(this ClaimsPrincipal user)
		{
			return user.IsInRole(Roles.Staff);
		}

		public static bool IsConsultant(this ClaimsPrincipal user)
		{
			return user.IsInRole(Roles.Consultant);
		}

		public static bool IsManager(this ClaimsPrincipal user)
		{
			return user.IsInRole(Roles.Manager);
		}

		public static bool IsAdmin(this ClaimsPrincipal user)
		{
			return user.IsInRole(Roles.Admin);
		}
		#endregion

		#region Permission Checking Methods

		// Course permissions
		public static bool CanViewCourses(this ClaimsPrincipal user)
		{
			return true;
		}

		public static bool CanRegisterCourses(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Member, Roles.Staff, Roles.Consultant, Roles.Manager, Roles.Admin);
		}

		public static bool CanManageCourses(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin);
		}

		// Survey permissions
		public static bool CanTakeSurveys(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Member, Roles.Staff, Roles.Consultant, Roles.Manager, Roles.Admin);
		}

		public static bool CanManageSurveys(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin);
		}

		public static bool CanViewSurveyResults(this ClaimsPrincipal user, int surveyOwnerId)
		{
			var currentUserId = user.GetUserId();
			return currentUserId == surveyOwnerId || user.CanViewAllReports();
		}

		// Appointment permissions
		public static bool CanBookAppointments(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Member, Roles.Staff, Roles.Consultant, Roles.Manager, Roles.Admin);
		}

		public static bool CanViewOwnAppointments(this ClaimsPrincipal user)
		{
			return user.IsAuthenticated();
		}

		public static bool CanViewAllAppointments(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin);
		}

		public static bool CanManageAppointments(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin);
		}

		public static bool CanViewAppointment(this ClaimsPrincipal user, int appointmentUserId, int? consultantId = null)
		{
			var currentUserId = user.GetUserId();

			// Owner can view
			if (currentUserId == appointmentUserId) return true;

			// Consultant can view their appointments
			if (consultantId.HasValue && currentUserId == consultantId.Value && user.IsConsultant()) return true;

			// Staff/Manager/Admin can view all
			return user.CanViewAllAppointments();
		}

		// Dashboard permissions
		public static bool CanViewDashboard(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Member, Roles.Staff, Roles.Consultant, Roles.Manager, Roles.Admin);
		}

		public static bool CanViewReports(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Manager, Roles.Admin);
		}

		public static bool CanViewAllReports(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Manager, Roles.Admin);
		}

		// User management permissions
		public static bool CanManageUsers(this ClaimsPrincipal user)
		{
			return user.IsAdmin();
		}

		public static bool CanManageConsultants(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Manager, Roles.Admin);
		}

		public static bool CanManagePrograms(this ClaimsPrincipal user)
		{
			return user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin);
		}

		// Resource ownership checking
		public static bool IsOwnerOrAdmin(this ClaimsPrincipal user, int resourceOwnerId)
		{
			var currentUserId = user.GetUserId();
			return currentUserId == resourceOwnerId || user.IsAdmin();
		}

		public static bool IsOwnerOrCanViewAll(this ClaimsPrincipal user, int resourceOwnerId)
		{
			var currentUserId = user.GetUserId();
			return currentUserId == resourceOwnerId || user.CanViewAllAppointments();
		}
		#endregion

		#region Helper Methods
		public static string GetHighestRole(this ClaimsPrincipal user)
		{
			if (user.IsAdmin()) return Roles.Admin;
			if (user.IsManager()) return Roles.Manager;
			if (user.IsStaff()) return Roles.Staff;
			if (user.IsConsultant()) return Roles.Consultant;
			return Roles.Member;
		}

		public static List<string> GetUserRoles(this ClaimsPrincipal user)
		{
			return user.Claims
				.Where(c => c.Type == ClaimTypes.Role)
				.Select(c => c.Value)
				.ToList();
		}

		public static bool HasAllRoles(this ClaimsPrincipal user, params string[] requiredRoles)
		{
			var userRoles = user.GetUserRoles();
			return requiredRoles.All(role => userRoles.Contains(role));
		}
		#endregion
	}
}
