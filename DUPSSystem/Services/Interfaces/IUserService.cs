using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IUserService
	{
		List<User> GetAccounts();
		User GetAccountById(int userId);
		void SaveAccount(CreateUserRequest request);
		void UpdateAccount(User s);
		void DeleteAccount(User s);
	}
}
