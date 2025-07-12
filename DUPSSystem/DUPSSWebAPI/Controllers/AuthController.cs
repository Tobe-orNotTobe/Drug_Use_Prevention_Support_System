using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace DUPSWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new LoginResponse
				{
					Success = false,
					Message = "Dữ liệu không hợp lệ"
				});
			}

			var response = await _authService.LoginAsync(request);

			if (!response.Success)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpPost("register")]
		public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				return BadRequest(new RegisterResponse
				{
					Success = false,
					Message = string.Join("; ", errors)
				});
			}

			var response = await _authService.RegisterAsync(request);

			if (!response.Success)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<UserInfo>> GetCurrentUser()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
			{
				return Unauthorized();
			}

			var userInfo = await _authService.GetUserByIdAsync(userId);
			if (userInfo == null)
			{
				return NotFound();
			}

			return Ok(userInfo);
		}
	}
}