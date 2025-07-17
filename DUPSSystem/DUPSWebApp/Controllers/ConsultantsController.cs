using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace DUPSWebApp.Controllers
{
	public class ConsultantController : BaseController
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly string _apiBaseUrl;

		public ConsultantController(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7008";
		}

		[RoleAuthorization("Consultant")]
		public async Task<IActionResult> MyProfile()
		{
			try
			{
				var consultantData = await GetMyConsultantProfileAsync();
				return View(consultantData);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = "Không thể tải thông tin hồ sơ";
				return View();
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetMyProfile()
		{
			try
			{
				var consultantData = await GetMyConsultantProfileAsync();
				return Json(new { success = true, data = consultantData });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Không thể tải thông tin hồ sơ" });
			}
		}

		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> Index()
		{
			return View();
		}

		[HttpPost]
		[RoleAuthorization("Consultant")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateConsultantProfileRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				var consultantData = await GetMyConsultantProfileAsync();
				if (consultantData == null)
				{
					return Json(new { success = false, message = "Không tìm thấy thông tin tư vấn viên" });
				}

				var patchUrl = $"{_apiBaseUrl}/odata/Consultants({consultantData.ConsultantId})";
				var patchData = new
				{
					Qualification = request.Qualification,
					Expertise = request.Expertise,
					WorkSchedule = request.WorkSchedule,
					Bio = request.Bio
				};

				var json = JsonSerializer.Serialize(patchData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				SetAuthorizationHeader();

				var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl)
				{
					Content = content
				};

				var patchResponse = await _httpClient.SendAsync(patchRequest);

				if (patchResponse.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Cập nhật hồ sơ thành công!" });
				}
				else
				{
					return Json(new { success = false, message = "Không thể cập nhật hồ sơ" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật hồ sơ" });
			}
		}

		[HttpGet]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> GetAvailableUsers()
		{
			try
			{
				var apiUrl = $"{_apiBaseUrl}/odata/Users?$expand=Roles";

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var odataResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

					if (odataResponse.TryGetProperty("value", out var usersArray))
					{
						var availableUsers = new List<object>();

						foreach (var userElement in usersArray.EnumerateArray())
						{
							var userId = userElement.GetProperty("UserId").GetInt32();
							var fullName = userElement.GetProperty("FullName").GetString();
							var email = userElement.GetProperty("Email").GetString();
							var isActive = userElement.GetProperty("IsActive").GetBoolean();

							bool hasConsultantRole = false;
							if (userElement.TryGetProperty("Roles", out var rolesArray))
							{
								foreach (var role in rolesArray.EnumerateArray())
								{
									if (role.GetProperty("RoleName").GetString() == "Consultant")
									{
										hasConsultantRole = true;
										break;
									}
								}
							}

							bool isAlreadyConsultant = await IsUserAlreadyConsultant(userId);

							if (isActive && !isAlreadyConsultant)
							{
								availableUsers.Add(new
								{
									UserId = userId,
									FullName = fullName,
									Email = email,
									HasConsultantRole = hasConsultantRole
								});
							}
						}

						return Json(new { success = true, data = availableUsers });
					}
				}

				return Json(new { success = false, message = "Không thể tải danh sách người dùng" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách người dùng" });
			}
		}

		[HttpGet]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> GetConsultants([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				var skip = (page - 1) * pageSize;
				var filter = "";

				if (!string.IsNullOrEmpty(search))
				{
					filter = $"&$filter=contains(tolower(User/FullName), '{search.ToLower()}') or contains(tolower(Expertise), '{search.ToLower()}')";
				}

				var apiUrl = $"{_apiBaseUrl}/odata/Consultants?$expand=User&$skip={skip}&$top={pageSize}&$count=true{filter}";

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync(apiUrl);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, data = responseContent });
				}
				else
				{
					return Json(new { success = false, message = "Không thể tải danh sách tư vấn viên" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách" });
			}
		}

		[HttpGet]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> GetConsultantDetails(int id)
		{
			try
			{
				var apiUrl = $"{_apiBaseUrl}/odata/Consultants({id})?$expand=User";

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var consultant = JsonSerializer.Deserialize<JsonElement>(responseContent);

					return Json(new { success = true, data = consultant });
				}
				else
				{
					return Json(new { success = false, message = "Không thể tải thông tin tư vấn viên" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi tải thông tin" });
			}
		}

		[HttpPost]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> CreateConsultant([FromBody] CreateConsultantRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				bool isAlreadyConsultant = await IsUserAlreadyConsultant(request.UserId);
				if (isAlreadyConsultant)
				{
					return Json(new { success = false, message = "Người dùng này đã là tư vấn viên" });
				}

				var apiUrl = $"{_apiBaseUrl}/odata/Consultants";

				SetAuthorizationHeader();

				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(apiUrl, content);

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Tạo tư vấn viên thành công!" });
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					return Json(new { success = false, message = "Không thể tạo tư vấn viên" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo tư vấn viên" });
			}
		}

		[HttpPut]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> UpdateConsultant([FromBody] UpdateConsultantRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				var apiUrl = $"{_apiBaseUrl}/odata/Consultants({request.ConsultantId})";

				SetAuthorizationHeader();

				var patchData = new
				{
					Qualification = request.Qualification,
					Expertise = request.Expertise,
					WorkSchedule = request.WorkSchedule,
					Bio = request.Bio
				};

				var json = JsonSerializer.Serialize(patchData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var patchRequest = new HttpRequestMessage(HttpMethod.Patch, apiUrl)
				{
					Content = content
				};

				var response = await _httpClient.SendAsync(patchRequest);

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Cập nhật tư vấn viên thành công!" });
				}
				else
				{
					return Json(new { success = false, message = "Không thể cập nhật tư vấn viên" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật tư vấn viên" });
			}
		}

		[HttpDelete]
		[RoleAuthorization("Manager", "Admin")]
		public async Task<IActionResult> DeleteConsultant(int id)
		{
			try
			{
				var apiUrl = $"{_apiBaseUrl}/odata/Consultants({id})";

				SetAuthorizationHeader();

				var response = await _httpClient.DeleteAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Xóa tư vấn viên thành công!" });
				}
				else
				{
					return Json(new { success = false, message = "Không thể xóa tư vấn viên" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa tư vấn viên" });
			}
		}

		private async Task<bool> IsUserAlreadyConsultant(int userId)
		{
			try
			{
				var apiUrl = $"{_apiBaseUrl}/odata/Consultants?$filter=UserId eq {userId}";

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var odataResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

					if (odataResponse.TryGetProperty("value", out var valueArray))
					{
						return valueArray.GetArrayLength() > 0;
					}
				}

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private async Task<dynamic?> GetMyConsultantProfileAsync()
		{
			try
			{
				var apiUrl = $"{_apiBaseUrl}/odata/Consultants?$filter=UserId eq {CurrentUserId}&$expand=User";

				SetAuthorizationHeader();

				var response = await _httpClient.GetAsync(apiUrl);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var odataResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

					if (odataResponse.TryGetProperty("value", out var valueArray) && valueArray.GetArrayLength() > 0)
					{
						return valueArray[0];
					}
				}

				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void SetAuthorizationHeader()
		{
			var token = HttpContext.Session.GetString("Token") ?? GetCookieValue("JWTToken");
			if (!string.IsNullOrEmpty(token))
			{
				_httpClient.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
			}
		}

		private string? GetCookieValue(string key)
		{
			return Request.Cookies[key];
		}
	}

	public class UpdateConsultantProfileRequest
	{
		public string? Qualification { get; set; }
		public string? Expertise { get; set; }
		public string? WorkSchedule { get; set; }
		public string? Bio { get; set; }
	}

	public class CreateConsultantRequest
	{
		public int UserId { get; set; }
		public string? Qualification { get; set; }
		public string? Expertise { get; set; }
		public string? WorkSchedule { get; set; }
		public string? Bio { get; set; }
	}
}
