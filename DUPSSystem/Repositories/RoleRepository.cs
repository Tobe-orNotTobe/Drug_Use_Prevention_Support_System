using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class RoleRepository : IRoleRepository
	{
		public List<Role> GetAllRoles() => RoleDAO.GetAll();

		public Role? GetRoleById(int roleId) => RoleDAO.GetById(roleId);

		public Role? GetRoleByName(string roleName) => RoleDAO.GetByName(roleName);

		public void SaveRole(Role role) => RoleDAO.Save(role);

		public void UpdateRole(Role role) => RoleDAO.Update(role);

		public void DeleteRole(Role role) => RoleDAO.Delete(role);
	}
}
