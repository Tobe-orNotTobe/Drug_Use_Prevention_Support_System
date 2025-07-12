using BusinessObjects;

namespace DataAccessObjects
{
	public class UserDAO
	{
		public static User GetById(int userId)
		{
			using var db = new DrugPreventionDbContext();
			return db.Users.FirstOrDefault(c => c.UserId.Equals(userId));
		}

		public static List<User> GetAll()
		{
			var listAccounts = new List<User>();
			try
			{
				using var db = new DrugPreventionDbContext();
				listAccounts = db.Users.ToList();
			}
			catch (Exception e) { }
			return listAccounts;
		}

		public static void Save(User s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Users.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(User s)
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

		public static void Delete(User s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Users.SingleOrDefault(c => c.UserId == s.UserId);
				context.Users.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}
