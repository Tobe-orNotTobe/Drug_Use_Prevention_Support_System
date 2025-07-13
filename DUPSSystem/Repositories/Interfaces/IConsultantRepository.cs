using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IConsultantRepository
	{
		List<Consultant> GetAllConsultants();
		Consultant? GetConsultantById(int consultantId);
		List<Consultant> GetAvailableConsultants();
		List<Consultant> SearchConsultants(string searchTerm);
		void SaveConsultant(Consultant consultant);
		void UpdateConsultant(Consultant consultant);
		void DeleteConsultant(Consultant consultant);
	}
}
