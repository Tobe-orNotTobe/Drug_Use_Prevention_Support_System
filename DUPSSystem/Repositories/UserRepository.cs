using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class UserRepository : IUserRepository
	{

		public List<User> GetAccounts() => UserDAO.GetAll();

		public User GetAccountById(int userId) => UserDAO.GetById(userId);

		public User? GetUserByEmail(string email) => UserDAO.GetByEmail(email);

		public List<string> GetUserRoles(int userId) => UserDAO.GetUserRoles(userId);

		public void SaveAccount(User s) => UserDAO.Save(s);

		public void UpdateAccount(User n) => UserDAO.Update(n);

		public void DeleteAccount(User n) => UserDAO.Delete(n);

		public void AssignUserRole(int userId, int roleId) => UserDAO.AssignUserRole(userId, roleId);

		public void UpdateProfile(int userId, string fullName, string phone, string address, DateTime? dateOfBirth, string gender)
		=> UserDAO.UpdateProfile(userId, fullName, phone, address, dateOfBirth, gender);

		public void UpdatePassword(int userId, string newPassword)
		=> UserDAO.UpdatePassword(userId, newPassword);
	}
}
