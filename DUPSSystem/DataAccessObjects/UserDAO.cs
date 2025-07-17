using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
   public class UserDAO
    {
        public static User GetById(int userId)
        {
            using var db = new DrugPreventionDbContext();
            return db.Users
                .Include(u => u.Roles)
                .FirstOrDefault(c => c.UserId.Equals(userId));
        }

        public static User? GetByEmail(string email)
        {
            using var db = new DrugPreventionDbContext();
            return db.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Email == email);
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

        public static void UpdateProfile(int userId, string fullName, string phone, string address, DateTime? dateOfBirth, string gender)
        {
            using var context = new DrugPreventionDbContext();
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.FullName = fullName;
                user.Phone = phone;
                user.Address = address;
                user.DateOfBirth = dateOfBirth;
                user.Gender = gender;
                user.UpdatedAt = DateTime.Now;
                user.IsActive = true;
                context.SaveChanges();
            }
        }

        public static void UpdatePassword(int userId, string newPassword)
        {
            using var context = new DrugPreventionDbContext();
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.PasswordHash = newPassword;
                user.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }
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
                listAccounts = db.Users
                    .Include(u => u.Roles)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToList();
            }
            catch (Exception e) 
            {
                throw new Exception(e.Message);
            }
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
                var user = context.Users
                   .Include(u => u.Roles)
                   .FirstOrDefault(u => u.UserId == s.UserId);

				if (user != null)
				{
					user.Roles.Clear();
					context.Users.Remove(user);
					context.SaveChanges();
				}
			}
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }}
