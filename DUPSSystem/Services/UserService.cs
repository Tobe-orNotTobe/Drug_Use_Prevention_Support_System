using Azure.Core;
using BusinessObjects;
using BusinessObjects.DTOs;
using DocumentFormat.OpenXml.Spreadsheet;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _repoUser;
		private readonly IRoleRepository _repoRole;

		public UserService()
		{
			_repoUser = new UserRepository();
			_repoRole = new RoleRepository();
		}

		public void DeleteAccount(User n) => _repoUser.DeleteAccount(n);

		public User GetAccountById(int userId) => _repoUser.GetAccountById(userId);

		public List<User> GetAccounts() => _repoUser.GetAccounts();

		public void SaveAccount(CreateUserRequest request)
		{
			var user = new User
			{
				FullName = request.FullName,
				Email = request.Email,
				PasswordHash = request.PasswordHash,
				Phone = request.Phone,
				DateOfBirth = request.DateOfBirth,
				Gender = request.Gender,
				Address = request.Address,
				IsActive = request.IsActive,
				CreatedAt = DateTime.UtcNow
			};

			_repoUser.SaveAccount(user);

			var newUser = _repoUser.GetUserByEmail(user.Email);

			var role = _repoRole.GetRoleByName(request.RoleName);

			_repoUser.AssignUserRole(newUser.UserId, role.RoleId);
		}

		public void UpdateAccount(User n) => _repoUser.UpdateAccount(n);
	}
}
