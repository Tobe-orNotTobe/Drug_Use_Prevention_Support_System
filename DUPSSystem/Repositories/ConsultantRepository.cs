using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class ConsultantRepository : IConsultantRepository
	{
		public List<Consultant> GetAllConsultants() => ConsultantDAO.GetAll();

		public Consultant? GetConsultantById(int consultantId) => ConsultantDAO.GetById(consultantId);

		public List<Consultant> GetAvailableConsultants() => ConsultantDAO.GetAvailable();

		public List<Consultant> SearchConsultants(string searchTerm) => ConsultantDAO.Search(searchTerm);

		public void SaveConsultant(Consultant consultant) => ConsultantDAO.Save(consultant);

		public void UpdateConsultant(Consultant consultant) => ConsultantDAO.Update(consultant);

		public void DeleteConsultant(Consultant consultant) => ConsultantDAO.Delete(consultant);
	}
}
