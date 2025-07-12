using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class UserDAO
	{
		public static User GetById(int userId)
		{
			using var db = new DrugPreventionDbContext();
			return db.Users.FirstOrDefault(c => c.UserId.Equals(userId));
		}

		public static User? GetByEmail(string email)
		{
			using var db = new DrugPreventionDbContext();
			return db.Users.FirstOrDefault(u => u.Email == email);
		}

		public static List<string> GetUserRoles(int userId)
		{
			using var db = new DrugPreventionDbContext();
			return db.Users
				.Where(u => u.UserId == userId)
				.SelectMany(u => u.Roles)
				.Select(r => r.RoleName)
				.ToList();
		}

		public static void AssignUserRole(int userId, int roleId)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var user = context.Users.Include(u => u.Roles).FirstOrDefault(u => u.UserId == userId);
				var role = context.Roles.FirstOrDefault(r => r.RoleId == roleId);

				if (user != null && role != null)
				{
					if (!user.Roles.Any(r => r.RoleId == roleId))
					{
						user.Roles.Add(role);
						context.SaveChanges();
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
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
