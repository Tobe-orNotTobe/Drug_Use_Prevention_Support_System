using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IConsultantService
	{
		ConsultantListResponse GetAllConsultants();
		ConsultantResponse GetConsultantById(int consultantId);
		ConsultantListResponse GetAvailableConsultants();
		ConsultantListResponse SearchConsultants(string searchTerm);
		ConsultantResponse CreateConsultant(ConsultantCreateRequest request);
		ConsultantResponse UpdateConsultant(ConsultantUpdateRequest request);
		ConsultantResponse DeleteConsultant(int consultantId);

		// Direct entity methods for OData controllers
		List<Consultant> GetConsultants();
		Consultant? GetConsultant(int consultantId);
		void SaveConsultant(Consultant consultant);
		void UpdateConsultant(Consultant consultant);
		void DeleteConsultant(Consultant consultant);
	}
}
