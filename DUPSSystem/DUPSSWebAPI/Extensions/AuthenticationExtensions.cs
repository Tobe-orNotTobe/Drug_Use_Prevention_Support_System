using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DUPSSWebAPI.Extensions
{
	public static class AuthenticationExtensions
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			var jwtSettings = configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["SecretKey"];
			if (string.IsNullOrEmpty(secretKey))
				throw new InvalidOperationException("Missing SecretKey in configuration");

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings["Issuer"],
					ValidAudience = jwtSettings["Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
					RoleClaimType = "Role"
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

	}

}
