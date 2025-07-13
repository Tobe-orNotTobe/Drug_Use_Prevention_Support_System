using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IUserCourseRepository
	{
		List<UserCourse> GetAllUserCourses();
		UserCourse? GetUserCourseById(int userCourseId);
		List<UserCourse> GetUserCoursesByUserId(int userId);
		List<UserCourse> GetUserCoursesByCourseId(int courseId);
		void SaveUserCourse(UserCourse userCourse);
		void UpdateUserCourse(UserCourse userCourse);
		void DeleteUserCourse(UserCourse userCourse);
		bool IsUserRegisteredForCourse(int userId, int courseId);
		UserCourse? GetUserCourseByUserAndCourse(int userId, int courseId);
	}
}
