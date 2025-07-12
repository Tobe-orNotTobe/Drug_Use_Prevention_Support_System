using System;
using System.Collections.Generic;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessObjects;

public partial class DrugPreventionDbContext : DbContext
{
	public DrugPreventionDbContext()
	{
	}

	public DrugPreventionDbContext(DbContextOptions<DrugPreventionDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<Appointment> Appointments { get; set; }

	public virtual DbSet<AuditLog> AuditLogs { get; set; }

	public virtual DbSet<CommunicationProgram> CommunicationPrograms { get; set; }

	public virtual DbSet<Consultant> Consultants { get; set; }

	public virtual DbSet<Course> Courses { get; set; }

	public virtual DbSet<ProgramSurvey> ProgramSurveys { get; set; }

	public virtual DbSet<Role> Roles { get; set; }

	public virtual DbSet<Survey> Surveys { get; set; }

	public virtual DbSet<SurveyOption> SurveyOptions { get; set; }

	public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }

	public virtual DbSet<User> Users { get; set; }

	public virtual DbSet<UserCourse> UserCourses { get; set; }

	public virtual DbSet<UserProgram> UserPrograms { get; set; }

	public virtual DbSet<UserSurveyAnswer> UserSurveyAnswers { get; set; }

	public virtual DbSet<UserSurveyResult> UserSurveyResults { get; set; }

	private string GetConnectionString()
	{
		IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", true, true).Build();
		return configuration["ConnectionStrings:MyStockDB"];
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlServer(GetConnectionString());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Appointment>(entity =>
		{
			entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC277055774");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.Status)
				.HasMaxLength(50)
				.HasDefaultValue("Pending");

			entity.HasOne(d => d.Consultant).WithMany(p => p.Appointments)
				.HasForeignKey(d => d.ConsultantId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__Appointme__Consu__5070F446");

			entity.HasOne(d => d.User).WithMany(p => p.Appointments)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__Appointme__UserI__4F7CD00D");
		});

		modelBuilder.Entity<AuditLog>(entity =>
		{
			entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5486489C654B69");

			entity.Property(e => e.Action).HasMaxLength(255);
			entity.Property(e => e.LogDate).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.TableName).HasMaxLength(255);

			entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
				.HasForeignKey(d => d.UserId)
				.HasConstraintName("FK__AuditLogs__UserI__5FB337D6");
		});

		modelBuilder.Entity<CommunicationProgram>(entity =>
		{
			entity.HasKey(e => e.ProgramId).HasName("PK__Communic__75256058808ACE51");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.Title).HasMaxLength(255);
		});

		modelBuilder.Entity<Consultant>(entity =>
		{
			entity.HasKey(e => e.ConsultantId).HasName("PK__Consulta__E5B83F59080B905B");

			entity.Property(e => e.Expertise).HasMaxLength(255);
			entity.Property(e => e.Qualification).HasMaxLength(255);

			entity.HasOne(d => d.User).WithMany(p => p.Consultants)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__Consultan__UserI__4AB81AF0");
		});

		modelBuilder.Entity<Course>(entity =>
		{
			entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7132BAF8B");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.IsActive).HasDefaultValue(true);
			entity.Property(e => e.TargetAudience).HasMaxLength(50);
			entity.Property(e => e.Title).HasMaxLength(255);
		});

		modelBuilder.Entity<ProgramSurvey>(entity =>
		{
			entity.HasKey(e => e.ProgramSurveyId).HasName("PK__ProgramS__A9FC9B0ECD72E6DB");

			entity.Property(e => e.SurveyType).HasMaxLength(50);

			entity.HasOne(d => d.Program).WithMany(p => p.ProgramSurveys)
				.HasForeignKey(d => d.ProgramId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__ProgramSu__Progr__5AEE82B9");

			entity.HasOne(d => d.Survey).WithMany(p => p.ProgramSurveys)
				.HasForeignKey(d => d.SurveyId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__ProgramSu__Surve__5BE2A6F2");
		});

		modelBuilder.Entity<Role>(entity =>
		{
			entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A9107C7BA");

			entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160E978BEDD").IsUnique();

			entity.Property(e => e.RoleName).HasMaxLength(50);
		});

		modelBuilder.Entity<Survey>(entity =>
		{
			entity.HasKey(e => e.SurveyId).HasName("PK__Surveys__A5481F7DBA01E94D");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.Name).HasMaxLength(255);
		});

		modelBuilder.Entity<SurveyOption>(entity =>
		{
			entity.HasKey(e => e.OptionId).HasName("PK__SurveyOp__92C7A1FF60711EEB");

			entity.HasOne(d => d.Question).WithMany(p => p.SurveyOptions)
				.HasForeignKey(d => d.QuestionId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__SurveyOpt__Quest__3E52440B");
		});

		modelBuilder.Entity<SurveyQuestion>(entity =>
		{
			entity.HasKey(e => e.QuestionId).HasName("PK__SurveyQu__0DC06FAC99E30BCD");

			entity.Property(e => e.QuestionType).HasMaxLength(50);

			entity.HasOne(d => d.Survey).WithMany(p => p.SurveyQuestions)
				.HasForeignKey(d => d.SurveyId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__SurveyQue__Surve__3B75D760");
		});

		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C69BEF9C2");

			entity.HasIndex(e => e.Email, "UQ__Users__A9D10534AFDD309C").IsUnique();

			entity.Property(e => e.Address).HasMaxLength(255);
			entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
			entity.Property(e => e.Email).HasMaxLength(255);
			entity.Property(e => e.FullName).HasMaxLength(255);
			entity.Property(e => e.Gender).HasMaxLength(20);
			entity.Property(e => e.IsActive).HasDefaultValue(true);
			entity.Property(e => e.PasswordHash).HasMaxLength(512);
			entity.Property(e => e.Phone).HasMaxLength(50);

			entity.HasMany(d => d.Roles).WithMany(p => p.Users)
				.UsingEntity<Dictionary<string, object>>(
					"UserRole",
					r => r.HasOne<Role>().WithMany()
						.HasForeignKey("RoleId")
						.OnDelete(DeleteBehavior.ClientSetNull)
						.HasConstraintName("FK__UserRoles__RoleI__2D27B809"),
					l => l.HasOne<User>().WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.ClientSetNull)
						.HasConstraintName("FK__UserRoles__UserI__2C3393D0"),
					j =>
					{
						j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760ADACDC85ED");
						j.ToTable("UserRoles");
					});
		});

		modelBuilder.Entity<UserCourse>(entity =>
		{
			entity.HasKey(e => e.UserCourseId).HasName("PK__UserCour__58886ED4D83E3038");

			entity.Property(e => e.RegisteredAt).HasDefaultValueSql("(getdate())");

			entity.HasOne(d => d.Course).WithMany(p => p.UserCourses)
				.HasForeignKey(d => d.CourseId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserCours__Cours__35BCFE0A");

			entity.HasOne(d => d.User).WithMany(p => p.UserCourses)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserCours__UserI__34C8D9D1");
		});

		modelBuilder.Entity<UserProgram>(entity =>
		{
			entity.HasKey(e => e.UserProgramId).HasName("PK__UserProg__2DA04025C5272DEF");

			entity.Property(e => e.JoinedAt).HasDefaultValueSql("(getdate())");

			entity.HasOne(d => d.Program).WithMany(p => p.UserPrograms)
				.HasForeignKey(d => d.ProgramId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserProgr__Progr__5812160E");

			entity.HasOne(d => d.User).WithMany(p => p.UserPrograms)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserProgr__UserI__571DF1D5");
		});

		modelBuilder.Entity<UserSurveyAnswer>(entity =>
		{
			entity.HasKey(e => e.AnswerId).HasName("PK__UserSurv__D4825004743CA516");

			entity.HasOne(d => d.Option).WithMany(p => p.UserSurveyAnswers)
				.HasForeignKey(d => d.OptionId)
				.HasConstraintName("FK__UserSurve__Optio__47DBAE45");

			entity.HasOne(d => d.Question).WithMany(p => p.UserSurveyAnswers)
				.HasForeignKey(d => d.QuestionId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserSurve__Quest__46E78A0C");

			entity.HasOne(d => d.Result).WithMany(p => p.UserSurveyAnswers)
				.HasForeignKey(d => d.ResultId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserSurve__Resul__45F365D3");
		});

		modelBuilder.Entity<UserSurveyResult>(entity =>
		{
			entity.HasKey(e => e.ResultId).HasName("PK__UserSurv__9769020811404729");

			entity.Property(e => e.TakenAt).HasDefaultValueSql("(getdate())");

			entity.HasOne(d => d.Survey).WithMany(p => p.UserSurveyResults)
				.HasForeignKey(d => d.SurveyId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserSurve__Surve__4316F928");

			entity.HasOne(d => d.User).WithMany(p => p.UserSurveyResults)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("FK__UserSurve__UserI__4222D4EF");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
