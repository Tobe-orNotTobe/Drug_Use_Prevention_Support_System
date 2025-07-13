using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DUPSWebApp.Controllers
{
	public class AuthController : Controller
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
		}

		public IActionResult Login(string? returnUrl = null)
		{
			// Check if user is already logged in
			if (HttpContext.Session.GetString("UserToken") != null)
			{
				return RedirectToAction("Index", "Home");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					ViewBag.ReturnUrl = returnUrl;
					return View(model);
				}

				var httpClient = _httpClientFactory.CreateClient();
				var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];

				var json = JsonConvert.SerializeObject(model);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// Call API login endpoint
				var response = await httpClient.PostAsync($"{apiBaseUrl}/api/AuthApi/login", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

					if (loginResponse != null && loginResponse.Success)
					{
						// Store token and user info in session
						HttpContext.Session.SetString("UserToken", loginResponse.Token ?? "");
						HttpContext.Session.SetString("UserInfo", JsonConvert.SerializeObject(loginResponse.User));
						HttpContext.Session.SetInt32("UserId", loginResponse.User?.UserId ?? 0);
						HttpContext.Session.SetString("UserName", loginResponse.User?.FullName ?? "");
						HttpContext.Session.SetString("UserEmail", loginResponse.User?.Email ?? "");

						TempData["SuccessMessage"] = "Đăng nhập thành công!";

						// Redirect to return URL or home
						if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
						{
							return Redirect(returnUrl);
						}

						return RedirectToAction("Index", "Home");
					}
					else
					{
						ModelState.AddModelError("", loginResponse?.Message ?? "Đăng nhập không thành công");
					}
				}
				else
				{
					var errorResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
					ModelState.AddModelError("", errorResponse?.Message ?? "Email hoặc mật khẩu không đúng");
				}
			}
			catch (HttpRequestException ex)
			{
				ModelState.AddModelError("", "Không thể kết nối đến máy chủ API. Vui lòng kiểm tra kết nối.");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}

		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterRequest model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}

				var httpClient = _httpClientFactory.CreateClient();
				var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];

				var json = JsonConvert.SerializeObject(model);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// Call API register endpoint
				var response = await httpClient.PostAsync($"{apiBaseUrl}/api/AuthApi/register", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);

					if (registerResponse != null && registerResponse.Success)
					{
						TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
						return RedirectToAction("Login");
					}
					else
					{
						ModelState.AddModelError("", registerResponse?.Message ?? "Đăng ký không thành công");
					}
				}
				else
				{
					var errorResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);
					ModelState.AddModelError("", errorResponse?.Message ?? "Đã xảy ra lỗi trong quá trình đăng ký");
				}
			}
			catch (HttpRequestException ex)
			{
				ModelState.AddModelError("", "Không thể kết nối đến máy chủ API. Vui lòng kiểm tra kết nối.");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			// Clear session
			HttpContext.Session.Clear();

			TempData["SuccessMessage"] = "Đăng xuất thành công!";
			return RedirectToAction("Login");
		}

		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}