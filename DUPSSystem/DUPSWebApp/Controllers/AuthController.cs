using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using DUPSWebApp.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace DUPSWebApp.Controllers
{
	public class AuthController : Controller
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly string _apiBaseUrl;
		private readonly IAntiforgery _antiforgery;

		public AuthController(HttpClient httpClient, IConfiguration configuration, IAntiforgery antiforgery)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7008";
			_antiforgery = antiforgery;
		}

		[HttpGet]
		public IActionResult Login()
		{
			if (!string.IsNullOrEmpty(Request.Cookies["JWTToken"]))
			{
				return RedirectToAction("Index", "Home");
			}
			return View(new LoginViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> LoginAjax([FromBody] LoginAjaxRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
				{
					return Json(new { success = false, message = "Email và mật khẩu không được để trống" });
				}

				var apiRequest = new
				{
					email = request.Email,
					password = request.Password
				};

				var json = JsonSerializer.Serialize(apiRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/auth/login", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
					{
						var cookieOptions = new CookieOptions
						{
							HttpOnly = true,
							Secure = false,
							SameSite = SameSiteMode.Lax,
							Expires = DateTime.UtcNow.AddHours(24)
						};

						Response.Cookies.Append("JWTToken", loginResponse.Token, cookieOptions);
						Response.Cookies.Append("UserInfo", JsonSerializer.Serialize(loginResponse.User), cookieOptions);

						return Json(new
						{
							success = true,
							message = "Đăng nhập thành công!",
							redirectUrl = Url.Action("Index", "Home")
						});
					}
				}

				var errorResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return Json(new
				{
					success = false,
					message = errorResponse?.Message ?? "Đăng nhập thất bại"
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Không thể kết nối đến server. Vui lòng thử lại."
				});
			}
		}

		[HttpGet]
		public IActionResult GetAntiForgeryToken()
		{
			var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
			return Json(new { token = tokens.RequestToken });
		}

		[HttpGet]
		public IActionResult Register()
		{
			if (!string.IsNullOrEmpty(Request.Cookies["JWTToken"]))
			{
				return RedirectToAction("Index", "Home");
			}
			return View(new RegisterViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> RegisterAjax([FromBody] RegisterAjaxRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.FullName))
				{
					return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc" });
				}

				if (request.Password != request.ConfirmPassword)
				{
					return Json(new { success = false, message = "Mật khẩu xác nhận không khớp" });
				}

				var apiRequest = new
				{
					email = request.Email,
					password = request.Password,
					confirmPassword = request.ConfirmPassword,
					fullName = request.FullName,
					dateOfBirth = request.DateOfBirth,
					gender = request.Gender,
					phone = request.Phone,
					address = request.Address
				};

				var json = JsonSerializer.Serialize(apiRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/auth/register", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (registerResponse?.Success == true)
					{
						return Json(new
						{
							success = true,
							message = "Đăng ký thành công! Vui lòng đăng nhập.",
							redirectUrl = Url.Action("Login", "Auth")
						});
					}
				}

				var errorResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return Json(new
				{
					success = false,
					message = errorResponse?.Message ?? "Đăng ký thất bại"
				});
			}
			catch (Exception)
			{
				return Json(new
				{
					success = false,
					message = "Không thể kết nối đến server. Vui lòng thử lại."
				});
			}
		}

		[HttpGet]
		public IActionResult Logout()
		{
			Response.Cookies.Delete("JWTToken");
			Response.Cookies.Delete("UserInfo");

			TempData["SuccessMessage"] = "Đăng xuất thành công!";
			return RedirectToAction("Login");
		}
	}

	// Request models for AJAX
	public class LoginAjaxRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool RememberMe { get; set; }
	}

	public class RegisterAjaxRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string? DateOfBirth { get; set; }
		public string? Gender { get; set; }
		public string? Phone { get; set; }
		public string? Address { get; set; }

	}
}