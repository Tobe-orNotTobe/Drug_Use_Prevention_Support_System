using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DUPSWebApp.TagHelpers
{
	[HtmlTargetElement("div", Attributes = "asp-authorize")]
	[HtmlTargetElement("span", Attributes = "asp-authorize")]
	[HtmlTargetElement("section", Attributes = "asp-authorize")]
	[HtmlTargetElement("a", Attributes = "asp-authorize")]
	[HtmlTargetElement("button", Attributes = "asp-authorize")]
	[HtmlTargetElement("li", Attributes = "asp-authorize")]
	public class AuthorizeTagHelper : TagHelper
	{
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName("asp-authorize")]
		public string Roles { get; set; }

		[HtmlAttributeName("asp-require-permission")]
		public string Permission { get; set; }

		[HtmlAttributeName("asp-hide-if-guest")]
		public bool HideIfGuest { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var user = ViewContext.HttpContext.User;

			bool isAuthorized = true;

			// Hide if guest and HideIfGuest is true
			if (HideIfGuest && user.IsGuest())
			{
				isAuthorized = false;
			}

			// Check roles
			if (!string.IsNullOrEmpty(Roles) && isAuthorized)
			{
				var roles = Roles.Split(',').Select(r => r.Trim()).ToArray();
				isAuthorized = user.IsInAnyRole(roles);
			}

			// Check specific permissions
			if (!string.IsNullOrEmpty(Permission) && isAuthorized)
			{
				isAuthorized = Permission.ToLower() switch
				{
					"manage-courses" => user.CanManageCourses(),
					"manage-surveys" => user.CanManageSurveys(),
					"manage-users" => user.CanManageUsers(),
					"manage-consultants" => user.CanManageConsultants(),
					"manage-appointments" => user.CanManageAppointments(),
					"view-reports" => user.CanViewReports(),
					"view-all-reports" => user.CanViewAllReports(),
					"register-courses" => user.CanRegisterCourses(),
					"take-surveys" => user.CanTakeSurveys(),
					"book-appointments" => user.CanBookAppointments(),
					"view-dashboard" => user.CanViewDashboard(),
					"view-own-appointments" => user.CanViewOwnAppointments(),
					"view-all-appointments" => user.CanViewAllAppointments(),
					_ => true
				};
			}

			if (!isAuthorized)
			{
				output.SuppressOutput();
			}
		}
	}
}