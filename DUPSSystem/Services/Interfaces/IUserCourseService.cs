using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IUserCourseService
	{
		List<UserCourse> GetAllUserCourses();
		UserCourse? GetUserCourseById(int userCourseId);
		List<UserCourse> GetUserCoursesByUserId(int userId);
		List<UserCourse> GetUserCoursesByCourseId(int courseId);
		CourseRegistrationResponse RegisterUserForCourse(CourseRegistrationRequest request);
		void UpdateUserCourse(UserCourse userCourse);
		void DeleteUserCourse(UserCourse userCourse);
		bool IsUserRegisteredForCourse(int userId, int courseId);
		void MarkCourseAsCompleted(int userId, int courseId);
	}
}
