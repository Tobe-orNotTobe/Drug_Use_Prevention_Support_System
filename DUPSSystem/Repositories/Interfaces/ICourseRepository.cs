using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ICourseRepository
	{
		List<Course> GetAllCourses();
		Course? GetCourseById(int courseId);
		void SaveCourse(Course course);
		void UpdateCourse(Course course);
		void DeleteCourse(Course course);
		List<Course> GetActiveCourses();
		List<Course> SearchCourses(string searchTerm);
	}
}
