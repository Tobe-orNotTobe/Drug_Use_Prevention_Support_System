using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class ConsultantDAO
	{
		public static Consultant GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants
				.Include(c => c.User)
				.FirstOrDefault(c => c.ConsultantId.Equals(id));
		}

		public static List<Consultant> GetAll()
		{
			var list = new List<Consultant>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Consultants
					.Include(c => c.User)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in ConsultantDAO.GetAll: {e.Message}");
			}
			return list;
		}

		public static List<Consultant> GetAvailable()
		{
			var list = new List<Consultant>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Consultants
					.Include(c => c.User)
					.Where(c => c.User.IsActive)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in ConsultantDAO.GetAvailable: {e.Message}");
			}
			return list;
		}

		public static List<Consultant> Search(string searchTerm)
		{
			var list = new List<Consultant>();
			try
			{
				using var db = new DrugPreventionDbContext();
				if (string.IsNullOrWhiteSpace(searchTerm))
				{
					list = db.Consultants
						.Include(c => c.User)
						.ToList();
				}
				else
				{
					searchTerm = searchTerm.ToLower();
					list = db.Consultants
						.Include(c => c.User)
						.Where(c => c.User.FullName.ToLower().Contains(searchTerm) ||
								   (c.Expertise != null && c.Expertise.ToLower().Contains(searchTerm)) ||
								   (c.Qualification != null && c.Qualification.ToLower().Contains(searchTerm)))
						.ToList();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in ConsultantDAO.Search: {e.Message}");
			}
			return list;
		}

		public static void Save(Consultant s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Consultants.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Consultant s)
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

		public static void Delete(Consultant s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Consultants.SingleOrDefault(c => c.ConsultantId == s.ConsultantId);
				if (s1 != null)
				{
					context.Consultants.Remove(s1);
					context.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}
