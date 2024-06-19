﻿using System.ComponentModel.DataAnnotations;

namespace Test.WSAM.Models
{
	public class RegisterModel
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
		[Required , Compare(nameof(Password))]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
