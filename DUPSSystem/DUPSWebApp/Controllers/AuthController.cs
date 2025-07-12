using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using DUPSWebApp.Models;

namespace DUPSWebApp.Controllers
{
	public class AuthController : Controller
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly string _apiBaseUrl;
		private readonly ILogger<AuthController> _logger;

		public AuthController(HttpClient httpClient, IConfiguration configuration, ILogger<AuthController> logger)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_logger = logger;
			_apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7008";
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "DUPSSystem.Web/1.0");
		}

		[HttpGet]
		public IActionResult Login()
		{
			_logger.LogInformation("GET Login action called");

			if (!string.IsNullOrEmpty(Request.Cookies["JWTToken"]))
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			_logger.LogInformation("POST Login action called with email: {Email}", model?.Email);

			if (!ModelState.IsValid)
			{
				_logger.LogWarning("ModelState is invalid");
				return View(model);
			}

			try
			{
				var requestData = new
				{
					email = model.Email,
					password = model.Password
				};

				var json = JsonSerializer.Serialize(requestData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				_logger.LogInformation("Calling API: {ApiUrl}", $"{_apiBaseUrl}/api/auth/login");

				var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/auth/login", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
				_logger.LogInformation("API Response Content: {Content}", responseContent);

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

						TempData["SuccessMessage"] = "Đăng nhập thành công!";
						return RedirectToAction("Index", "Home");
					}
				}

				ModelState.AddModelError("", $"Lỗi từ API: {response.StatusCode} - {responseContent}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during login");
				ModelState.AddModelError("", $"Lỗi: {ex.Message}");
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult Register()
		{
			_logger.LogInformation("GET Register action called");

			// Nếu đã login rồi thì redirect về Home
			if (!string.IsNullOrEmpty(Request.Cookies["JWTToken"]))
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}

		[HttpPost]
		public IActionResult Logout()
		{
			_logger.LogInformation("Logout action called");

			// Clear cookies
			Response.Cookies.Delete("JWTToken");
			Response.Cookies.Delete("UserInfo");
			TempData["SuccessMessage"] = "Đăng xuất thành công!";
			return RedirectToAction("Login");
		}
	}
}