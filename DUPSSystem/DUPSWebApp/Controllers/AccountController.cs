using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace DUPSWebApp.Controllers
{
	public class AccountController : BaseController
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public AccountController(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public async Task<IActionResult> Profile()
		{
			try
			{
				var user = await GetCurrentUserProfileAsync();
				if (user == null)
				{
					return RedirectToAction("Login", "Auth");
				}

				ViewBag.UserProfile = user;
				return View(user);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = "Không thể tải thông tin hồ sơ";
				return View();
			}
		}

		[HttpPost]
		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/AuthApi/profile";

				var json = JsonSerializer.Serialize(new
				{
					userId = CurrentUserId,
					fullName = request.FullName,
					phone = request.Phone,
					address = request.Address,
					dateOfBirth = request.DateOfBirth,
					gender = request.Gender
				}); 
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var token = HttpContext.Session.GetString("Token");
				if (!string.IsNullOrEmpty(token))
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var response = await _httpClient.PutAsync(apiUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<BaseApiResponse>(responseContent,
						new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});

					if (!string.IsNullOrEmpty(request.FullName))
					{
						HttpContext.Session.SetString("FullName", request.FullName);
					}

					return Json(new
					{
						success = true,
						message = apiResponse?.Message ?? "Cập nhật hồ sơ thành công"
					});
				}
				else
				{
					var apiResponse = JsonSerializer.Deserialize<BaseApiResponse>(responseContent,
						new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});

					return Json(new
					{
						success = false,
						message = apiResponse?.Message ?? "Không thể cập nhật hồ sơ"
					});
				}
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Đã xảy ra lỗi khi cập nhật hồ sơ"
				});
			}
		}

		[HttpPost]
		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				if (request.NewPassword != request.ConfirmPassword)
				{
					return Json(new { success = false, message = "Mật khẩu xác nhận không khớp" });
				}

				var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/AuthApi/change-password";

				var json = JsonSerializer.Serialize(new 
				{
					UserId = CurrentUserId,
					CurrentPassword = request.CurrentPassword,
					NewPassword = request.NewPassword,
					ConfirmPassword = request.ConfirmPassword
				});

				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var token = HttpContext.Session.GetString("Token");
				if (!string.IsNullOrEmpty(token))
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var response = await _httpClient.PutAsync(apiUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var apiResponse = JsonSerializer.Deserialize<BaseApiResponse>(responseContent,
						new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});

					return Json(new
					{
						success = true,
						message = apiResponse?.Message ?? "Đổi mật khẩu thành công"
					});
				}
				else
				{
					var apiResponse = JsonSerializer.Deserialize<BaseApiResponse>(responseContent,
						new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});

					return Json(new
					{
						success = false,
						message = apiResponse?.Message ?? "Mật khẩu hiện tại không đúng"
					});
				}
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Đã xảy ra lỗi khi đổi mật khẩu"
				});
			}
		}


		[RoleAuthorization("Consultant", "Manager", "Admin")]
		public async Task<IActionResult> ConsultantProfile()
		{
			if (!IsConsultant && !IsManager && !IsAdmin)
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			try
			{
				var consultant = await GetConsultantProfileAsync();
				return View(consultant);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = "Không thể tải thông tin tư vấn viên";
				return View();
			}
		}

		[HttpPost]
		[RoleAuthorization("Consultant", "Manager", "Admin")]
		public async Task<IActionResult> UpdateConsultantProfile([FromBody] UpdateConsultantRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/odata/Consultants";
				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var token = HttpContext.Session.GetString("Token");
				if (!string.IsNullOrEmpty(token))
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var response = await _httpClient.PatchAsync($"{apiUrl}({request.ConsultantId})", content);

				if (response.IsSuccessStatusCode)
				{
					return Json(new
					{
						success = true,
						message = "Cập nhật hồ sơ tư vấn viên thành công"
					});
				}

				return Json(new
				{
					success = false,
					message = "Không thể cập nhật hồ sơ tư vấn viên"
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Đã xảy ra lỗi khi cập nhật hồ sơ"
				});
			}
		}

		private async Task<UserProfile> GetCurrentUserProfileAsync()
		{
			try
			{
				var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/AuthApi/profile/{CurrentUserId}";

				var token = HttpContext.Session.GetString("Token");
				if (!string.IsNullOrEmpty(token))
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonSerializer.Deserialize<ApiUserResponse>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (apiResponse.Success && apiResponse.Data != null)
					{
						return new UserProfile
						{
							UserId = apiResponse.Data.UserId,
							Email = apiResponse.Data.Email,
							FullName = apiResponse.Data.FullName,
							Phone = apiResponse.Data.Phone,
							Address = apiResponse.Data.Address,
							DateOfBirth = apiResponse.Data.DateOfBirth,
							Gender = apiResponse.Data.Gender,
							IsActive = apiResponse.Data.IsActive,
							Roles = apiResponse.Data.Roles,
						};
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		private async Task<ConsultantProfile> GetConsultantProfileAsync()
		{
			try
			{
				var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/odata/Consultants?$filter=UserId eq {CurrentUserId}&$expand=User";

				var token = HttpContext.Session.GetString("Token");
				if (!string.IsNullOrEmpty(token))
				{
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				}

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var odataResponse = JsonSerializer.Deserialize<ODataResponse<ConsultantProfile>>(responseContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					return odataResponse.Value?.FirstOrDefault();
				}

				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}

	// DTOs
	public class UserProfile
	{
		public int UserId { get; set; }
		public string Email { get; set; } = "";
		public string FullName { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; } = "";
		public bool IsActive { get; set; }
		public List<string> Roles { get; set; } = new List<string>();
		public DateTime CreatedAt { get; set; }
	}

	public class ConsultantProfile
	{
		public int ConsultantId { get; set; }
		public int UserId { get; set; }
		public string Qualification { get; set; } = "";
		public string Expertise { get; set; } = "";
		public string WorkSchedule { get; set; } = "";
		public string Bio { get; set; } = "";
		public UserProfile User { get; set; } = new UserProfile();
	}

	public class UpdateProfileRequest
	{
		public string FullName { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; } = "";
	}

	public class UpdateConsultantRequest
	{
		public int ConsultantId { get; set; }
		public string Qualification { get; set; } = "";
		public string Expertise { get; set; } = "";
		public string WorkSchedule { get; set; } = "";
		public string Bio { get; set; } = "";
	}

	public class ChangePasswordRequest
	{
		public string CurrentPassword { get; set; } = "";
		public string NewPassword { get; set; } = "";
		public string ConfirmPassword { get; set; } = "";
	}

	public class ApiUserResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = "";
		public UserProfile Data { get; set; } = new UserProfile();
	}

	public class ODataResponse<T>
	{
		public List<T> Value { get; set; } = new List<T>();
	}
	public class BaseApiResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = "";
	}
}