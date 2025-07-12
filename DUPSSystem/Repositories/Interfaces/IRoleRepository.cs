using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IRoleRepository
	{
		List<Role> GetAllRoles();
		Role? GetRoleById(int roleId);
		Role? GetRoleByName(string roleName);
		void SaveRole(Role role);
		void UpdateRole(Role role);
		void DeleteRole(Role role);
	}
}
