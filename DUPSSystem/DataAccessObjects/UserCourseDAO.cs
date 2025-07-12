using BusinessObjects;

namespace DataAccessObjects
{
	public class UserCourseDAO
	{
		public static UserCourse GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.UserCourses.FirstOrDefault(c => c.UserCourseId.Equals(id));
		}

		public static List<UserCourse> GetAll()
		{
			var list = new List<UserCourse>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserCourses.ToList();
			}
			catch (Exception e) { }
			return list;
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
