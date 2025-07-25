﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string Password { get; set; } = null!;
	}
}
