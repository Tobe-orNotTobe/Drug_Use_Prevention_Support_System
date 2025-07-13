using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using DataAccessObjects;
using System.IdentityModel.Tokens.Jwt;

namespace DUPSWebAPI.Controllers.Extensions
{
	public static class AuthenticationExtensions
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			var jwtSettings = configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["SecretKey"];
			var issuer = jwtSettings["Issuer"];
			var audience = jwtSettings["Audience"];

			if (string.IsNullOrEmpty(secretKey))
			{
				secretKey = "your-super-secret-key-that-is-at-least-32-characters-long";
				issuer = "DUPSSystem";
				audience = "DUPSUsers";
			}

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false; // Set to true in production
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
					ValidateIssuer = true,
					ValidIssuer = issuer,
					ValidateAudience = true,
					ValidAudience = audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				};
			});

			return services;
		}
	}

	public class JwtService
	{
		private readonly IConfiguration _configuration;

		public JwtService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string GenerateToken(User user, List<string> roles)
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
				expires: DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}