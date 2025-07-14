using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text;

namespace DUPSWebApp.Controllers
{
	public class AuthController : BaseController
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public AuthController(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		[HttpGet]
		public IActionResult Login(string returnUrl = null)
		{
			if (!IsGuest)
			{
				return RedirectToAction("Index", "Home");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			try
			{
				var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/AuthApi/login";
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(apiUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (loginResponse.Success && loginResponse.User != null)
					{
						var handler = new JwtSecurityTokenHandler();
						var jwtToken = handler.ReadJwtToken(loginResponse.Token);

						var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value
								   ?? jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
						var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
								  ?? jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
						var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
									 ?? jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

						var rolesFromToken = jwtToken.Claims.Where(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role)
														  .Select(c => c.Value).ToList();

						var rolesFromResponse = loginResponse.User.Roles ?? new List<string>();

						var allRoles = rolesFromToken.Concat(rolesFromResponse).Distinct().ToList();

						var primaryRole = GetPrimaryRole(allRoles);

						HttpContext.Session.SetString("Token", loginResponse.Token);
						HttpContext.Session.SetString("UserId", userId ?? loginResponse.User.UserId.ToString());
						HttpContext.Session.SetString("Email", email ?? loginResponse.User.Email);
						HttpContext.Session.SetString("FullName", fullName ?? loginResponse.User.FullName);
						HttpContext.Session.SetString("Role", primaryRole);
						HttpContext.Session.SetString("Roles", string.Join(",", allRoles));

						return Json(new
						{
							success = true,
							message = "Đăng nhập thành công",
							redirectUrl = GetRedirectUrlByRole(primaryRole),
							userInfo = new
							{
								userId = userId,
								email = email,
								fullName = fullName,
								role = primaryRole,
								roles = allRoles
							}
						});
					}
				}

				return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
				return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình đăng nhập" });
			}
		}

		private string GetPrimaryRole(List<string> roles)
		{
			if (roles == null || !roles.Any()) return "Member";

			var rolePriority = new Dictionary<string, int>
			{
				{ "Admin", 6 },
				{ "Manager", 5 },
				{ "Staff", 4 },
				{ "Consultant", 3 },
				{ "Member", 2 },
				{ "Guest", 1 }
			};

			return roles
				.Where(r => rolePriority.ContainsKey(r))
				.OrderByDescending(r => rolePriority[r])
				.FirstOrDefault() ?? "Member";
		}

		[HttpGet]
		public IActionResult Register()
		{
			if (!IsGuest)
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			try
			{
				var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/AuthApi/register";
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(apiUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (registerResponse.Success)
					{
						return Json(new
						{
							success = true,
							message = "Đăng ký thành công! Vui lòng đăng nhập."
						});
					}
					else
					{
						return Json(new { success = false, message = registerResponse.Message });
					}
				}

				return Json(new { success = false, message = "Đăng ký thất bại" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình đăng ký" });
			}
		}

		[HttpPost]
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public IActionResult UserInfo()
		{
			var userInfo = new
			{
				UserId = HttpContext.Session.GetString("UserId"),
				Email = HttpContext.Session.GetString("Email"),
				FullName = HttpContext.Session.GetString("FullName"),
				Role = HttpContext.Session.GetString("Role"),
				Roles = HttpContext.Session.GetString("Roles"),
				Token = HttpContext.Session.GetString("Token")?.Substring(0, 50) + "..." 
			};

			return Json(userInfo);
		}

		private int GetRolePriority(string role)
		{
			return role switch
			{
				"Admin" => 6,
				"Manager" => 5,
				"Staff" => 4,
				"Consultant" => 3,
				"Member" => 2,
				"Guest" => 1,
				_ => 0
			};
		}

		private string GetRedirectUrlByRole(string role)
		{
			return role switch
			{
				"Admin" => Url.Action("Dashboard", "Admin"),
				"Manager" => Url.Action("Dashboard", "Manager"),
				"Staff" => Url.Action("Index", "Courses"),
				"Consultant" => Url.Action("MyAppointments", "Appointments"),
				"Member" => Url.Action("Index", "Home"),
				_ => Url.Action("Index", "Home")
			};
		}
	}

	// DTOs
	public class LoginRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}

	public class LoginResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string Token { get; set; }
		public UserInfo User { get; set; }
	}

	public class UserInfo
	{
		public int UserId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public bool IsActive { get; set; }
		public List<string> Roles { get; set; }
	}

	public class RegisterRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
		public string FullName { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
	}

	public class RegisterResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int? UserId { get; set; }
	}
}