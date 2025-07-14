using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add Session support
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 minutes timeout
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.Cookie.Name = "DUPS.Session";
});

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add configuration for API settings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Authentication (optional, for direct token validation if needed)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-jwt-token-signing";

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
	options.LoginPath = "/Auth/Login";
	options.LogoutPath = "/Auth/Logout";
	options.AccessDeniedPath = "/Home/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
	options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
		ValidateIssuer = true,
		ValidIssuer = jwtSettings["Issuer"] ?? "DUPSSystem",
		ValidateAudience = true,
		ValidAudience = jwtSettings["Audience"] ?? "DUPSUsers",
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Add custom middleware for session-based authentication
app.Use(async (context, next) =>
{
	// Skip for login/register pages and static files
	var path = context.Request.Path.Value?.ToLower();
	if (path?.Contains("/auth/") == true ||
		path?.Contains("/home/accessdenied") == true ||
		path?.Contains("/css/") == true ||
		path?.Contains("/js/") == true ||
		path?.Contains("/lib/") == true ||
		path?.Contains("/images/") == true)
	{
		await next();
		return;
	}

	// Check if user has valid session
	var token = context.Session.GetString("Token");
	var role = context.Session.GetString("Role");

	if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(role))
	{
		// Set items in HttpContext for controllers to use
		context.Items["Token"] = token;
		context.Items["Role"] = role;
		context.Items["UserId"] = context.Session.GetString("UserId");
		context.Items["Email"] = context.Session.GetString("Email");
		context.Items["FullName"] = context.Session.GetString("FullName");
	}

	await next();
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ApiSettings
{
	public string BaseUrl { get; set; } = "https://localhost:7008";
}