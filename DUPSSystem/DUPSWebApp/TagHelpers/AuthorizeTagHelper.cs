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
	public class AuthorizeTagHelper : TagHelper
	{
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName("asp-authorize")]
		public string Roles { get; set; }

		[HtmlAttributeName("asp-require-permission")]
		public string Permission { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var user = ViewContext.HttpContext.User;

			bool isAuthorized = true;

			// Check roles
			if (!string.IsNullOrEmpty(Roles))
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
					"view-reports" => user.CanViewReports(),
					"register-courses" => user.CanRegisterCourses(),
					"take-surveys" => user.CanTakeSurveys(),
					"book-appointments" => user.CanBookAppointments(),
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
