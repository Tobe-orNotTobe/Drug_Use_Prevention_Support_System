using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
	public class UserProfileDto
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Address { get; set; }
	}

	public class CreateUserRequest
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string Phone { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Address { get; set; }
		public bool IsActive { get; set; }
		public string RoleName { get; set; }
	}

	public class UpdateProfileRequest
	{
		public int UserId { get; set; }
		public string FullName { get; set; }
		public string Phone { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Address { get; set; }
	}

	public class ChangePasswordRequest
	{
		public int UserId { get; set; }
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
	}

	public class BaseResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}
