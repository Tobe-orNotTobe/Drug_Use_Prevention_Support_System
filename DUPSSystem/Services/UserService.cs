using BusinessObjects;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _repo;

		public UserService() => _repo = new UserRepository();

		public void DeleteAccount(User n) => _repo.DeleteAccount(n);

		public User GetAccountById(int userId) => _repo.GetAccountById(userId);

		public List<User> GetAccounts() => _repo.GetAccounts();

		public void SaveAccount(User s) => _repo.SaveAccount(s);

		public void UpdateAccount(User n) => _repo.UpdateAccount(n);
	}
}
