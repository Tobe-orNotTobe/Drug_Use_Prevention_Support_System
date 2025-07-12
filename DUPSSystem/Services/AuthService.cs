using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly IConfiguration _configuration;

		public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration)
		{
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_configuration = configuration;
		}

		public async Task<LoginResponse> LoginAsync(LoginRequest request)
		{
			try
			{
				// Get user by email
				var user = _userRepository.GetUserByEmail(request.Email);
				if (user == null)
				{
					return new LoginResponse
					{
						Success = false,
						Message = "Email hoặc mật khẩu không đúng"
					};
				}

				if (!user.IsActive)
				{
					return new LoginResponse
					{
						Success = false,
						Message = "Tài khoản đã bị vô hiệu hóa"
					};
				}

				// Verify password
				if (!VerifyPassword(request.Password, user.PasswordHash))
				{
					return new LoginResponse
					{
						Success = false,
						Message = "Email hoặc mật khẩu không đúng"
					};
				}

				// Get user roles
				var roles = _userRepository.GetUserRoles(user.UserId);

				// Generate JWT token
				var token = GenerateJwtToken(user, roles);

				return new LoginResponse
				{
					Success = true,
					Message = "Đăng nhập thành công",
					Token = token,
					User = new UserInfo
					{
						UserId = user.UserId,
						Email = user.Email,
						FullName = user.FullName,
						IsActive = user.IsActive,
						Roles = roles
					}
				};
			}
			catch (Exception ex)
			{
				return new LoginResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng nhập"
				};
			}
		}

		public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
		{
			try
			{
				// Check if email already exists
				var existingUser = _userRepository.GetUserByEmail(request.Email);
				if (existingUser != null)
				{
					return new RegisterResponse
					{
						Success = false,
						Message = "Email đã được sử dụng"
					};
				}

				// Create new user
				var newUser = new User
				{
					Email = request.Email,
					PasswordHash = HashPassword(request.Password),
					FullName = request.FullName,
					DateOfBirth = request.DateOfBirth,
					Gender = request.Gender,
					Phone = request.Phone,
					Address = request.Address,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};

				// Save user
				_userRepository.SaveAccount(newUser);

				// Assign default role "Member" 
				var memberRole = _roleRepository.GetRoleByName("Member");
				if (memberRole != null)
				{
					_userRepository.AssignUserRole(newUser.UserId, memberRole.RoleId);
				}

				return new RegisterResponse
				{
					Success = true,
					Message = "Đăng ký thành công",
					UserId = newUser.UserId
				};
			}
			catch (Exception ex)
			{
				return new RegisterResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng ký"
				};
			}
		}

		public async Task<UserInfo?> GetUserByIdAsync(int userId)
		{
			try
			{
				var user = _userRepository.GetAccountById(userId);
				if (user == null) return null;

				var roles = _userRepository.GetUserRoles(userId);

				return new UserInfo
				{
					UserId = user.UserId,
					Email = user.Email,
					FullName = user.FullName,
					IsActive = user.IsActive,
					Roles = roles
				};
			}
			catch
			{
				return null;
			}
		}

		public string HashPassword(string password)
		{
			// Đơn giản hóa: chỉ lưu password trực tiếp (không recommend cho production)
			return password;
		}

		public bool VerifyPassword(string password, string hashedPassword)
		{
			// So sánh trực tiếp password
			return password == hashedPassword;
		}

		public string GenerateJwtToken(User user, List<string> roles)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["SecretKey"];
			var issuer = jwtSettings["Issuer"];
			var audience = jwtSettings["Audience"];

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.FullName),
				new Claim("IsActive", user.IsActive.ToString())
			};

			// Add roles
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: DateTime.UtcNow.AddHours(24),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}