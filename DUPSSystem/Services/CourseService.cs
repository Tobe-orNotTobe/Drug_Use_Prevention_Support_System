using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class CourseService : ICourseService
	{
		private readonly ICourseRepository _courseRepository;

		public CourseService(ICourseRepository courseRepository)
		{
			_courseRepository = courseRepository;
		}

		public List<Course> GetAllCourses() => _courseRepository.GetAllCourses();

		public Course? GetCourseById(int courseId) => _courseRepository.GetCourseById(courseId);

		public void SaveCourse(Course course)
		{
			if (course == null)
				throw new ArgumentNullException(nameof(course));

			if (string.IsNullOrWhiteSpace(course.Title))
				throw new ArgumentException("Tiêu đề khóa học không được để trống");

			course.CreatedAt = DateTime.UtcNow;
			course.IsActive = true;

			_courseRepository.SaveCourse(course);
		}

		public void UpdateCourse(Course course)
		{
			if (course == null)
				throw new ArgumentNullException(nameof(course));

			if (string.IsNullOrWhiteSpace(course.Title))
				throw new ArgumentException("Tiêu đề khóa học không được để trống");

			course.UpdatedAt = DateTime.UtcNow;

			_courseRepository.UpdateCourse(course);
		}

		public void DeleteCourse(Course course)
		{
			if (course == null)
				throw new ArgumentNullException(nameof(course));

			_courseRepository.DeleteCourse(course);
		}

		public List<Course> GetActiveCourses() => _courseRepository.GetActiveCourses();

		public List<Course> SearchCourses(string searchTerm) => _courseRepository.SearchCourses(searchTerm);
	}
}