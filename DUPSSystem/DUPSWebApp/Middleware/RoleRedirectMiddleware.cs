using BusinessObjects.Constants;
using BusinessObjects.Extensions;

namespace DUPSWebApp.Middleware
{
	public class RoleRedirectMiddleware
	{
		private readonly RequestDelegate _next;

		public RoleRedirectMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Check if user is accessing restricted areas
			var path = context.Request.Path.Value?.ToLower();
			var user = context.User;

			if (user.Identity?.IsAuthenticated == true)
			{
				// Admin-only areas
				if (path?.StartsWith("/users") == true && !user.IsAdmin())
				{
					context.Response.Redirect("/Home/Index");
					return;
				}

				// Management areas
				if ((path?.StartsWith("/courses/manage") == true ||
					 path?.StartsWith("/surveys/manage") == true ||
					 path?.StartsWith("/appointments/manage") == true)
					&& !user.IsInAnyRole(Roles.Staff, Roles.Manager, Roles.Admin))
				{
					context.Response.Redirect("/Home/Index");
					return;
				}

				// Reports access
				if (path?.StartsWith("/home/reports") == true && !user.CanViewReports())
				{
					context.Response.Redirect("/Home/Index");
					return;
				}
			}

			await _next(context);
		}
	}

	public static class RoleRedirectMiddlewareExtensions
	{
		public static IApplicationBuilder UseRoleRedirect(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RoleRedirectMiddleware>();
		}
	}
}
