using System.Text.Json;

namespace DUPSWebApp.Extensions
{
	public static class CookieHelper
	{
		public static void SetObject<T>(this IResponseCookies cookies, string key, T value, CookieOptions options = null)
		{
			var json = JsonSerializer.Serialize(value);
			cookies.Append(key, json, options ?? new CookieOptions());
		}

		public static T GetObject<T>(this IRequestCookieCollection cookies, string key)
		{
			var value = cookies[key];
			return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
		}

		public static bool IsLoggedIn(this HttpContext context)
		{
			return !string.IsNullOrEmpty(context.Request.Cookies["JWTToken"]);
		}

		public static string GetUserName(this HttpContext context)
		{
			try
			{
				var userInfoCookie = context.Request.Cookies["UserInfo"];
				if (!string.IsNullOrEmpty(userInfoCookie))
				{
					var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoCookie);
					return userInfo.GetProperty("fullName").GetString() ?? "User";
				}
			}
			catch { }
			return "User";
		}
	}
}
