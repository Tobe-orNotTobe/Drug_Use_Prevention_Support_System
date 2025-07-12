using BusinessObjects;

namespace DataAccessObjects
{
	public class RoleDAO
	{
		public static Role? GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Roles.FirstOrDefault(c => c.RoleId.Equals(id));
		}

		public static Role? GetByName(string roleName)
		{
			using var db = new DrugPreventionDbContext();
			return db.Roles.FirstOrDefault(r => r.RoleName == roleName);
		}

		public static List<Role> GetAll()
		{
			var list = new List<Role>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Roles.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(Role s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Roles.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Role s)
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

		public static void Delete(Role s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Roles.SingleOrDefault(c => c.RoleId == s.RoleId);
				context.Roles.Remove(s1);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}