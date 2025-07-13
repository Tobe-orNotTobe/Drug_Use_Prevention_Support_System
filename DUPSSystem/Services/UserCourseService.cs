using BusinessObjects;
using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class UserCourseService : IUserCourseService
	{
		private readonly IUserCourseRepository _userCourseRepository;
		private readonly ICourseRepository _courseRepository;
		private readonly IUserRepository _userRepository;

		public UserCourseService(	
			IUserCourseRepository userCourseRepository,
			ICourseRepository courseRepository,
			IUserRepository userRepository)
		{
			_userCourseRepository = userCourseRepository;
			_courseRepository = courseRepository;
			_userRepository = userRepository;
		}

		public List<UserCourse> GetAllUserCourses() => _userCourseRepository.GetAllUserCourses();

		public UserCourse? GetUserCourseById(int userCourseId) => _userCourseRepository.GetUserCourseById(userCourseId);

		public List<UserCourse> GetUserCoursesByUserId(int userId) => _userCourseRepository.GetUserCoursesByUserId(userId);

		public List<UserCourse> GetUserCoursesByCourseId(int courseId) => _userCourseRepository.GetUserCoursesByCourseId(courseId);

		public CourseRegistrationResponse RegisterUserForCourse(CourseRegistrationRequest request)
		{
			try
			{
				// Validate user exists
				var user = _userRepository.GetAccountById(request.UserId);
				if (user == null)
				{
					return new CourseRegistrationResponse
					{
						Success = false,
						Message = "Người dùng không tồn tại"
					};
				}

				// Validate course exists
				var course = _courseRepository.GetCourseById(request.CourseId);
				if (course == null)
				{
					return new CourseRegistrationResponse
					{
						Success = false,
						Message = "Khóa học không tồn tại"
					};
				}

				// Check if course is active
				if (!course.IsActive)
				{
					return new CourseRegistrationResponse
					{
						Success = false,
						Message = "Khóa học hiện không khả dụng"
					};
				}

				// Check if user already registered
				if (_userCourseRepository.IsUserRegisteredForCourse(request.UserId, request.CourseId))
				{
					return new CourseRegistrationResponse
					{
						Success = false,
						Message = "Bạn đã đăng ký khóa học này rồi"
					};
				}

				// Create new registration
				var userCourse = new UserCourse
				{
					UserId = request.UserId,
					CourseId = request.CourseId,
					RegisteredAt = DateTime.UtcNow
				};

				_userCourseRepository.SaveUserCourse(userCourse);

				return new CourseRegistrationResponse
				{
					Success = true,
					Message = "Đăng ký khóa học thành công",
					UserCourseId = userCourse.UserCourseId
				};
			}
			catch (Exception ex)
			{
				return new CourseRegistrationResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng ký"
				};
			}
		}

		public void UpdateUserCourse(UserCourse userCourse)
		{
			if (userCourse == null)
				throw new ArgumentNullException(nameof(userCourse));

			_userCourseRepository.UpdateUserCourse(userCourse);
		}

		public void DeleteUserCourse(UserCourse userCourse)
		{
			if (userCourse == null)
				throw new ArgumentNullException(nameof(userCourse));

			_userCourseRepository.DeleteUserCourse(userCourse);
		}

		public bool IsUserRegisteredForCourse(int userId, int courseId) =>
			_userCourseRepository.IsUserRegisteredForCourse(userId, courseId);

		public void MarkCourseAsCompleted(int userId, int courseId)
		{
			var userCourse = _userCourseRepository.GetUserCourseByUserAndCourse(userId, courseId);
			if (userCourse != null && userCourse.CompletedAt == null)
			{
				userCourse.CompletedAt = DateTime.UtcNow;
				_userCourseRepository.UpdateUserCourse(userCourse);
			}
		}
	}
}
