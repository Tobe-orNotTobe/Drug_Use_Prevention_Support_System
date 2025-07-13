using BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// EDM Model for OData
static IEdmModel GetEdmModel()
{
	var builder = new ODataConventionModelBuilder();

	builder.EntitySet<Course>("Courses");
	builder.EntitySet<UserCourse>("UserCourses");
	builder.EntitySet<User>("Users");
	builder.EntitySet<Role>("Roles"); 
	builder.EntitySet<Consultant>("Consultants").EntityType.HasMany(c => c.Appointments);
	builder.EntitySet<Appointment>("Appointments");
	builder.EntitySet<Survey>("Surveys");
	builder.EntitySet<SurveyQuestion>("SurveyQuestions").EntityType.HasMany(q => q.SurveyOptions);
	builder.EntitySet<SurveyOption>("SurveyOptions");
	builder.EntitySet<UserSurveyAnswer>("UserSurveyAnswers");
	builder.EntitySet<UserSurveyResult>("UserSurveyResults");

	return builder.GetEdmModel();
}

// Add controllers with OData
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	})
	.AddOData(options => options
		.Select()
		.Filter()
		.OrderBy()
		.Expand()
		.Count()
		.SetMaxTop(100)
		.AddRouteComponents("odata", GetEdmModel()));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserCourseRepository, UserCourseRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
builder.Services.AddScoped<ISurveyOptionRepository, SurveyOptionRepository>();
builder.Services.AddScoped<ISurveyResultRepository, SurveyResultRepository>();
builder.Services.AddScoped<ISurveyAnswerRepository, SurveyAnswerRepository>();
builder.Services.AddScoped<IConsultantRepository, ConsultantRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserCourseService, UserCourseService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<ISurveyQuestionService, SurveyQuestionService>();
builder.Services.AddScoped<ISurveyOptionService, SurveyOptionService>();
builder.Services.AddScoped<ISurveyResultService, SurveyResultService>();
builder.Services.AddScoped<IConsultantService, ConsultantService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-jwt-token-signing";
var issuer = jwtSettings["Issuer"] ?? "DUPSSystem";
var audience = jwtSettings["Audience"] ?? "DUPSUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
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

builder.Services.AddAuthorization();

// Configure Swagger - CRITICAL: This order matters
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "DUPS API",
		Version = "v1"
	});

	// IMPORTANT: Handle OData route conflicts
	c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

	// Custom schema IDs to prevent naming conflicts
	c.CustomSchemaIds(type =>
	{
		var name = type.Name;
		if (type.IsGenericType)
		{
			var genericArgs = string.Join("", type.GetGenericArguments().Select(t => t.Name));
			name = $"{name.Split('`')[0]}{genericArgs}";
		}
		return name;
	});

	// JWT Security
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});

	// Filter to exclude problematic OData operations
	c.OperationFilter<ODataOperationFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "DUPS API V1");
	});
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Custom operation filter to handle OData conflicts
public class ODataOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		// Remove problematic OData parameters that cause Swagger issues
		if (operation.Parameters != null)
		{
			var parametersToRemove = operation.Parameters
				.Where(p => p.Name.StartsWith("$") || p.Name.Contains("odata"))
				.ToList();

			foreach (var param in parametersToRemove)
			{
				operation.Parameters.Remove(param);
			}
		}

		// Clean up operation IDs to prevent conflicts
		if (!string.IsNullOrEmpty(operation.OperationId))
		{
			operation.OperationId = operation.OperationId.Replace("odata.", "").Replace("$", "");
		}
	}
}