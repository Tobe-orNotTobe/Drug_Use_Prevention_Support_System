using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class UserCourseRepository : IUserCourseRepository
	{
		public List<UserCourse> GetAllUserCourses() => UserCourseDAO.GetAll();

		public UserCourse? GetUserCourseById(int userCourseId) => UserCourseDAO.GetById(userCourseId);

		public List<UserCourse> GetUserCoursesByUserId(int userId) => UserCourseDAO.GetByUserId(userId);

		public List<UserCourse> GetUserCoursesByCourseId(int courseId) => UserCourseDAO.GetByCourseId(courseId);

		public void SaveUserCourse(UserCourse userCourse) => UserCourseDAO.Save(userCourse);

		public void UpdateUserCourse(UserCourse userCourse) => UserCourseDAO.Update(userCourse);

		public void DeleteUserCourse(UserCourse userCourse) => UserCourseDAO.Delete(userCourse);

		public bool IsUserRegisteredForCourse(int userId, int courseId) =>
			UserCourseDAO.IsUserRegisteredForCourse(userId, courseId);

		public UserCourse? GetUserCourseByUserAndCourse(int userId, int courseId) =>
			UserCourseDAO.GetByUserAndCourse(userId, courseId);

	}
}
