using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthApiController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthApiController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new LoginResponse
					{
						Success = false,
						Message = "Dữ liệu không hợp lệ"
					});
				}

				var result = await _authService.LoginAsync(request);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new LoginResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng nhập"
				});
			}
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new RegisterResponse
					{
						Success = false,
						Message = "Dữ liệu không hợp lệ"
					});
				}

				var result = await _authService.RegisterAsync(request);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new RegisterResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng ký"
				});
			}
		}

		[HttpGet("user/{id}")]
		public async Task<IActionResult> GetUser(int id)
		{
			try
			{
				var user = await _authService.GetUserByIdAsync(id);

				if (user != null)
				{
					return Ok(new
					{
						success = true,
						message = "Lấy thông tin người dùng thành công",
						data = user
					});
				}
				else
				{
					return NotFound(new
					{
						success = false,
						message = "Không tìm thấy người dùng"
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Đã xảy ra lỗi khi lấy thông tin người dùng"
				});
			}
		}

		[HttpPost("validate-token")]
		public IActionResult ValidateToken([FromBody] string token)
		{
			try
			{
				if (!string.IsNullOrEmpty(token))
				{
					return Ok(new
					{
						success = true,
						message = "Token hợp lệ"
					});
				}
				else
				{
					return BadRequest(new
					{
						success = false,
						message = "Token không hợp lệ"
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Đã xảy ra lỗi khi xác thực token"
				});
			}
		}

		[HttpGet("profile/{userId}")]
		public async Task<IActionResult> GetProfile(int userId)
		{
			try
			{
				var profile = await _authService.GetProfileAsync(userId);

				if (profile != null)
				{
					return Ok(new
					{
						success = true,
						message = "Lấy thông tin hồ sơ thành công",
						data = profile
					});
				}
				else
				{
					return NotFound(new
					{
						success = false,
						message = "Không tìm thấy hồ sơ người dùng"
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Đã xảy ra lỗi khi lấy hồ sơ người dùng"
				});
			}
		}

		[HttpPut("profile")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new BaseResponse
					{
						Success = false,
						Message = "Dữ liệu không hợp lệ"
					});
				}

				var result = await _authService.UpdateProfileAsync(request.UserId, request);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi cập nhật hồ sơ"
				});
			}
		}

		[HttpPut("change-password")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new BaseResponse
					{
						Success = false,
						Message = "Dữ liệu không hợp lệ"
					});
				}

				var result = await _authService.ChangePasswordAsync(request.UserId, request);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new BaseResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi đổi mật khẩu"
				});
			}
		}

	}
}