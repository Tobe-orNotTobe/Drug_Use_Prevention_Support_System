using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using DUPSWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DUPSWebApp.Controllers
{
	public class AuthController : BaseController
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthController> _logger;
		private readonly string _apiBaseUrl;

		public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AuthController> logger)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
			_apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7008";
		}

		#region Login
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			if (User.IsAuthenticated())
			{
				return RedirectToAction("Index", "Home");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(new LoginViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					ViewBag.ReturnUrl = returnUrl;
					return View(model);
				}

				var httpClient = _httpClientFactory.CreateClient();

				var loginRequest = new LoginViewModel
				{
					Email = model.Email,
					Password = model.Password
				};

				var json = JsonConvert.SerializeObject(loginRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// Call API
				var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/AuthApi/login", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);

				if (response.IsSuccessStatusCode)
				{
					var loginResponse = JsonConvert.DeserializeObject<DUPSWebAPI.Models.LoginResponse>(responseContent);

					if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
					{
						// Parse JWT token to get user claims
						var handler = new JwtSecurityTokenHandler();
						var jsonToken = handler.ReadJwtToken(loginResponse.Token);

						var claims = new List<Claim>
						{
							new Claim(ClaimTypes.NameIdentifier, loginResponse.User?.UserId.ToString() ?? "0"),
							new Claim(ClaimTypes.Email, loginResponse.User?.Email ?? ""),
							new Claim(ClaimTypes.Name, loginResponse.User?.FullName ?? ""),
							new Claim("JwtToken", loginResponse.Token)
						};

						// Add roles from JWT token
						var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role);
						claims.AddRange(roleClaims);

						// Create identity and principal
						var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
						var authProperties = new AuthenticationProperties
						{
							IsPersistent = model.RememberMe,
							ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
						};

						// Sign in user
						await HttpContext.SignInAsync(
							CookieAuthenticationDefaults.AuthenticationScheme,
							new ClaimsPrincipal(claimsIdentity),
							authProperties);

						// Store additional data in session
						HttpContext.Session.SetString("UserToken", loginResponse.Token);
						HttpContext.Session.SetInt32("UserId", loginResponse.User?.UserId ?? 0);
						HttpContext.Session.SetString("UserName", loginResponse.User?.FullName ?? "");
						HttpContext.Session.SetString("UserEmail", loginResponse.User?.Email ?? "");

						SetSuccessMessage("Đăng nhập thành công!");

						// Redirect based on role or return URL
						if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
						{
							return Redirect(returnUrl);
						}

						// Role-based redirect
						var userRole = User.GetHighestRole();
						return RedirectToAction("Index", "Home");

					}
					else
					{
						ModelState.AddModelError("", loginResponse?.Message ?? "Đăng nhập không thành công");
					}
				}
				else
				{
					try
					{
						var errorResponse = JsonConvert.DeserializeObject<DUPSWebAPI.Models.LoginResponse>(responseContent);
						ModelState.AddModelError("", errorResponse?.Message ?? "Email hoặc mật khẩu không đúng");
					}
					catch
					{
						ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập");
					}
				}
			}
			catch (HttpRequestException)
			{
				ModelState.AddModelError("", "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during login");
				ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}
		#endregion

		#region Register
		[HttpGet]
		public IActionResult Register()
		{
			if (User.IsAuthenticated())
			{
				return RedirectToAction("Index", "Home");
			}

			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}

				var httpClient = _httpClientFactory.CreateClient();

				var registerRequest = new RegisterViewModel
				{
					Email = model.Email,
					Password = model.Password,
					FullName = model.FullName,
					Phone = model.Phone,
					Address = model.Address,
					Gender = model.Gender,
					DateOfBirth = model.DateOfBirth
				};

				var json = JsonConvert.SerializeObject(registerRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/AuthApi/register", content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var registerResponse = JsonConvert.DeserializeObject<Models.RegisterResponse>(responseContent);

					if (registerResponse?.Success == true)
					{
						SetSuccessMessage("Đăng ký thành công! Vui lòng đăng nhập.");
						return RedirectToAction("Login");
					}
					else
					{
						ModelState.AddModelError("", registerResponse?.Message ?? "Đăng ký không thành công");
					}
				}
				else
				{
					try
					{
						var errorResponse = JsonConvert.DeserializeObject<Models.RegisterResponse>(responseContent);
						ModelState.AddModelError("", errorResponse?.Message ?? "Đăng ký không thành công");
					}
					catch
					{
						ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng ký");
					}
				}
			}
			catch (HttpRequestException)
			{
				ModelState.AddModelError("", "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during registration");
				ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.");
			}

			return View(model);
		}
		#endregion

		#region Logout
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			// Clear authentication cookie
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			// Clear session
			HttpContext.Session.Clear();

			SetSuccessMessage("Đăng xuất thành công!");
			return RedirectToAction("Index", "Home");
		}
		#endregion

		#region Access Denied
		public IActionResult AccessDenied()
		{
			ViewBag.Message = "Bạn không có quyền truy cập trang này.";
			return View();
		}
		#endregion

		#region Profile Management
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Profile()
		{
			SetViewBagPermissions();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("Profile", model);
			}

			try
			{
				var httpClient = _httpClientFactory.CreateClient();
				var token = HttpContext.Session.GetString("UserToken");

				if (!string.IsNullOrEmpty(token))
				{
					httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var json = JsonConvert.SerializeObject(model);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await httpClient.PutAsync($"{_apiBaseUrl}/api/AuthApi/profile", content);

				if (response.IsSuccessStatusCode)
				{
					SetSuccessMessage("Cập nhật thông tin thành công!");
					return RedirectToAction("Profile");
				}
				else
				{
					SetErrorMessage("Không thể cập nhật thông tin. Vui lòng thử lại.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating profile");
				SetErrorMessage("Đã xảy ra lỗi khi cập nhật thông tin.");
			}

			return View("Profile", model);
		}
		#endregion
	}
}