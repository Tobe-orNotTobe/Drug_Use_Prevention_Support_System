using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IUserRepository
	{
		List<User> GetAccounts();
		User GetAccountById(int userId);
		User? GetUserByEmail(string email);
		List<string> GetUserRoles(int userId);
		void SaveAccount(User s);
		void UpdateAccount(User s);
		void DeleteAccount(User s);
		void AssignUserRole(int userId, int roleId);

		void UpdateProfile(int userId, string fullName, string phone, string address, DateTime? dateOfBirth, string gender);
		void UpdatePassword(int userId, string newPasswordHash);
	}
}
