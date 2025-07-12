using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class UserRepository : IUserRepository
	{
		public List<User> GetAccounts() => UserDAO.GetAll();

		public User GetAccountById(int userId) => UserDAO.GetById(userId);

		public void SaveAccount(User s) => UserDAO.Save(s);

		public void UpdateAccount(User n) => UserDAO.Update(n);

		public void DeleteAccount(User n) => UserDAO.Delete(n);
	}
}
