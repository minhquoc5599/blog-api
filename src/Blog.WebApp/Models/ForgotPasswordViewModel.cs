﻿using System.ComponentModel.DataAnnotations;

namespace Blog.WebApp.Models
{
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "Email required")]
		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Email format is not correct.")]
		public string Email { get; set; }
	}
}
