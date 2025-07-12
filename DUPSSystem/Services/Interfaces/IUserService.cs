using BusinessObjects;

namespace Services.Interfaces
{
	public interface IUserService
	{
		List<User> GetAccounts();
		User GetAccountById(int userId);
		void SaveAccount(User s);
		void UpdateAccount(User s);
		void DeleteAccount(User s);
	}
}
