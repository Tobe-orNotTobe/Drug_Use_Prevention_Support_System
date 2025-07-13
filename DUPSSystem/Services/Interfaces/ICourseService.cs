using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface ICourseService
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
