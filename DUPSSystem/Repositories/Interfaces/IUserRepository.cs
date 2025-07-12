using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IUserRepository
	{
		List<User> GetAccounts();
		User GetAccountById(int userId);
		void SaveAccount(User s);
		void UpdateAccount(User s);
		void DeleteAccount(User s);
	}
}
