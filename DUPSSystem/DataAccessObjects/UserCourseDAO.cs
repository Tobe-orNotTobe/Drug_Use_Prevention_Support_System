using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class UserCourseDAO
	{
		public static UserCourse GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.UserCourses
				.Include(uc => uc.User)
				.Include(uc => uc.Course)
				.FirstOrDefault(c => c.UserCourseId.Equals(id));
		}

		public static List<UserCourse> GetAll()
		{
			var list = new List<UserCourse>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserCourses
					.Include(uc => uc.User)
					.Include(uc => uc.Course)
					.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static List<UserCourse> GetByUserId(int userId)
		{
			var list = new List<UserCourse>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserCourses
					.Include(uc => uc.User)
					.Include(uc => uc.Course)
					.Where(uc => uc.UserId == userId)
					.OrderByDescending(uc => uc.RegisteredAt)
					.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static List<UserCourse> GetByCourseId(int courseId)
		{
			var list = new List<UserCourse>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserCourses
					.Include(uc => uc.User)
					.Include(uc => uc.Course)
					.Where(uc => uc.CourseId == courseId)
					.OrderByDescending(uc => uc.RegisteredAt)
					.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static bool IsUserRegisteredForCourse(int userId, int courseId)
		{
			try
			{
				using var db = new DrugPreventionDbContext();
				return db.UserCourses.Any(uc => uc.UserId == userId && uc.CourseId == courseId);
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public static UserCourse? GetByUserAndCourse(int userId, int courseId)
		{
			try
			{
				using var db = new DrugPreventionDbContext();
				return db.UserCourses
					.Include(uc => uc.User)
					.Include(uc => uc.Course)
					.FirstOrDefault(uc => uc.UserId == userId && uc.CourseId == courseId);
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public static void Save(UserCourse s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.UserCourses.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(UserCourse s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Entry(s).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Delete(UserCourse s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.UserCourses.SingleOrDefault(c => c.UserCourseId == s.UserCourseId);
				context.UserCourses.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}