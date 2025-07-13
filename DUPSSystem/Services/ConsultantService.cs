using BusinessObjects;
using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class ConsultantService : IConsultantService
	{
		private readonly IConsultantRepository _consultantRepository;
		private readonly IUserRepository _userRepository;

		public ConsultantService(IConsultantRepository consultantRepository, IUserRepository userRepository)
		{
			_consultantRepository = consultantRepository;
			_userRepository = userRepository;
		}

		public ConsultantListResponse GetAllConsultants()
		{
			try
			{
				var consultants = _consultantRepository.GetAllConsultants();
				var consultantDetails = consultants.Select(MapToConsultantDetail).ToList();

				return new ConsultantListResponse
				{
					Success = true,
					Message = "Lấy danh sách tư vấn viên thành công",
					Data = consultantDetails
				};
			}
			catch (Exception ex)
			{
				return new ConsultantListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy danh sách tư vấn viên: " + ex.Message,
					Data = new List<ConsultantDetail>()
				};
			}
		}

		public ConsultantResponse GetConsultantById(int consultantId)
		{
			try
			{
				var consultant = _consultantRepository.GetConsultantById(consultantId);
				if (consultant == null)
				{
					return new ConsultantResponse
					{
						Success = false,
						Message = "Không tìm thấy tư vấn viên"
					};
				}

				return new ConsultantResponse
				{
					Success = true,
					Message = "Lấy thông tin tư vấn viên thành công",
					Data = MapToConsultantDetail(consultant)
				};
			}
			catch (Exception ex)
			{
				return new ConsultantResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy thông tin tư vấn viên: " + ex.Message
				};
			}
		}

		public ConsultantListResponse GetAvailableConsultants()
		{
			try
			{
				var consultants = _consultantRepository.GetAvailableConsultants();
				var consultantDetails = consultants.Select(MapToConsultantDetail).ToList();

				return new ConsultantListResponse
				{
					Success = true,
					Message = "Lấy danh sách tư vấn viên khả dụng thành công",
					Data = consultantDetails
				};
			}
			catch (Exception ex)
			{
				return new ConsultantListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy danh sách tư vấn viên: " + ex.Message,
					Data = new List<ConsultantDetail>()
				};
			}
		}

		public ConsultantListResponse SearchConsultants(string searchTerm)
		{
			try
			{
				var consultants = _consultantRepository.SearchConsultants(searchTerm);
				var consultantDetails = consultants.Select(MapToConsultantDetail).ToList();

				return new ConsultantListResponse
				{
					Success = true,
					Message = "Tìm kiếm tư vấn viên thành công",
					Data = consultantDetails
				};
			}
			catch (Exception ex)
			{
				return new ConsultantListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi tìm kiếm tư vấn viên: " + ex.Message,
					Data = new List<ConsultantDetail>()
				};
			}
		}

		public ConsultantResponse CreateConsultant(ConsultantCreateRequest request)
		{
			try
			{
				// Validate user exists
				var user = _userRepository.GetAccountById(request.UserId);
				if (user == null)
				{
					return new ConsultantResponse
					{
						Success = false,
						Message = "Người dùng không tồn tại"
					};
				}

				// Check if user is already a consultant
				var existingConsultants = _consultantRepository.GetAllConsultants();
				if (existingConsultants.Any(c => c.UserId == request.UserId))
				{
					return new ConsultantResponse
					{
						Success = false,
						Message = "Người dùng này đã là tư vấn viên"
					};
				}

				var consultant = new Consultant
				{
					UserId = request.UserId,
					Qualification = request.Qualification,
					Expertise = request.Expertise,
					WorkSchedule = request.WorkSchedule,
					Bio = request.Bio
				};

				_consultantRepository.SaveConsultant(consultant);

				// Reload with user data
				var savedConsultant = _consultantRepository.GetConsultantById(consultant.ConsultantId);

				return new ConsultantResponse
				{
					Success = true,
					Message = "Tạo tư vấn viên thành công",
					Data = MapToConsultantDetail(savedConsultant!)
				};
			}
			catch (Exception ex)
			{
				return new ConsultantResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi tạo tư vấn viên: " + ex.Message
				};
			}
		}

		public ConsultantResponse UpdateConsultant(ConsultantUpdateRequest request)
		{
			try
			{
				var consultant = _consultantRepository.GetConsultantById(request.ConsultantId);
				if (consultant == null)
				{
					return new ConsultantResponse
					{
						Success = false,
						Message = "Không tìm thấy tư vấn viên"
					};
				}

				consultant.Qualification = request.Qualification;
				consultant.Expertise = request.Expertise;
				consultant.WorkSchedule = request.WorkSchedule;
				consultant.Bio = request.Bio;

				_consultantRepository.UpdateConsultant(consultant);

				return new ConsultantResponse
				{
					Success = true,
					Message = "Cập nhật tư vấn viên thành công",
					Data = MapToConsultantDetail(consultant)
				};
			}
			catch (Exception ex)
			{
				return new ConsultantResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi cập nhật tư vấn viên: " + ex.Message
				};
			}
		}

		public ConsultantResponse DeleteConsultant(int consultantId)
		{
			try
			{
				var consultant = _consultantRepository.GetConsultantById(consultantId);
				if (consultant == null)
				{
					return new ConsultantResponse
					{
						Success = false,
						Message = "Không tìm thấy tư vấn viên"
					};
				}

				_consultantRepository.DeleteConsultant(consultant);

				return new ConsultantResponse
				{
					Success = true,
					Message = "Xóa tư vấn viên thành công"
				};
			}
			catch (Exception ex)
			{
				return new ConsultantResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi xóa tư vấn viên: " + ex.Message
				};
			}
		}

		// Direct entity methods for OData controllers
		public List<Consultant> GetConsultants() => _consultantRepository.GetAllConsultants();

		public Consultant? GetConsultant(int consultantId) => _consultantRepository.GetConsultantById(consultantId);

		public void SaveConsultant(Consultant consultant)
		{
			if (consultant == null)
				throw new ArgumentNullException(nameof(consultant));

			_consultantRepository.SaveConsultant(consultant);
		}

		public void UpdateConsultant(Consultant consultant)
		{
			if (consultant == null)
				throw new ArgumentNullException(nameof(consultant));

			_consultantRepository.UpdateConsultant(consultant);
		}

		public void DeleteConsultant(Consultant consultant)
		{
			if (consultant == null)
				throw new ArgumentNullException(nameof(consultant));

			_consultantRepository.DeleteConsultant(consultant);
		}

		private ConsultantDetail MapToConsultantDetail(Consultant consultant)
		{
			return new ConsultantDetail
			{
				ConsultantId = consultant.ConsultantId,
				UserId = consultant.UserId,
				FullName = consultant.User?.FullName ?? "Không xác định",
				Email = consultant.User?.Email ?? "Không xác định",
				Phone = consultant.User?.Phone,
				Qualification = consultant.Qualification,
				Expertise = consultant.Expertise,
				WorkSchedule = consultant.WorkSchedule,
				Bio = consultant.Bio,
				IsActive = consultant.User?.IsActive ?? false,
				TotalAppointments = consultant.Appointments?.Count ?? 0
			};
		}
	}
}