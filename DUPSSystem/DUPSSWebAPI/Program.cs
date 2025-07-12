using BusinessObjects;
using DUPSSWebAPI.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

static IEdmModel GetEdmModel()
{
	ODataConventionModelBuilder builder = new();

	builder.EntitySet<Appointment>("Appointments");
	builder.EntitySet<AuditLog>("AuditLogs");
	builder.EntitySet<CommunicationProgram>("CommunicationPrograms");
	builder.EntitySet<Consultant>("Consultants");
	builder.EntitySet<Course>("Courses");
	builder.EntitySet<ProgramSurvey>("ProgramSurveys");
	builder.EntitySet<Role>("Roles");
	builder.EntitySet<Survey>("Surveys");
	builder.EntitySet<SurveyOption>("SurveyOptions");
	builder.EntitySet<SurveyQuestion>("SurveyQuestions");
	builder.EntitySet<User>("Users");
	builder.EntitySet<UserCourse>("UserCourses");
	builder.EntitySet<UserProgram>("UserPrograms");
	builder.EntitySet<UserSurveyAnswer>("UserSurveyAnswers");
	builder.EntitySet<UserSurveyResult>("UserSurveyResults");

	return builder.GetEdmModel();
}

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<JwtService>();

builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
	})
	.AddOData(options => options
		.Select()
		.Filter()
		.OrderBy()
		.Expand()
		.Count()
		.SetMaxTop(100)
		.AddRouteComponents("odata", GetEdmModel())
		.EnableQueryFeatures());

builder.Services.AddEndpointsApiExplorer();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Drug Use Prevention Support System API",
		Version = "v1",
		Description = "API for Drug Use Prevention Support System "
	});

	// Add JWT Bearer authentication to Swagger
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
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
			Array.Empty<string>()
		}
	});

	// This helps Swagger understand OData routes
	c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// Add CORS if needed
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		builder =>
		{
			builder.AllowAnyOrigin()
				   .AllowAnyMethod()
				   .AllowAnyHeader();
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Drug Use Prevention Support System API V1");
	});
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.Run();
