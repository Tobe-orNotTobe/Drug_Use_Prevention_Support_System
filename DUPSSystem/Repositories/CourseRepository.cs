using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class CourseRepository : ICourseRepository
	{
		public List<Course> GetAllCourses() => CourseDAO.GetAll();

		public Course? GetCourseById(int courseId) => CourseDAO.GetById(courseId);

		public void SaveCourse(Course course) => CourseDAO.Save(course);

		public void UpdateCourse(Course course) => CourseDAO.Update(course);

		public void DeleteCourse(Course course) => CourseDAO.Delete(course);

		public List<Course> GetActiveCourses() => CourseDAO.GetActive();

		public List<Course> SearchCourses(string searchTerm) => CourseDAO.Search(searchTerm);
	}
}
