using BusinessObjects;

namespace DataAccessObjects
{
	public class CourseDAO
	{
		public static Course GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Courses.FirstOrDefault(c => c.CourseId.Equals(id));
		}

		public static List<Course> GetAll()
		{
			var list = new List<Course>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Courses.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static List<Course> GetActive()
		{
			var list = new List<Course>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Courses.Where(c => c.IsActive).ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static List<Course> Search(string searchTerm)
		{
			var list = new List<Course>();
			try
			{
				using var db = new DrugPreventionDbContext();
				if (string.IsNullOrWhiteSpace(searchTerm))
				{
					list = db.Courses.ToList();
				}
				else
				{
					searchTerm = searchTerm.ToLower();
					list = db.Courses
						.Where(c => c.Title.ToLower().Contains(searchTerm) ||
								   (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
								   (c.TargetAudience != null && c.TargetAudience.ToLower().Contains(searchTerm)))
						.ToList();
				}
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(Course s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Courses.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Course s)
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

		public static void Delete(Course s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Courses.SingleOrDefault(c => c.CourseId == s.CourseId);
				context.Courses.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}
