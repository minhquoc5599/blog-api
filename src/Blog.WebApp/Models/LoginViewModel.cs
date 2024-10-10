using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Blog.WebApp.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Email is required")]
		[DisplayName("UserName")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[DisplayName("Password")]
		public string Password { get; set; }

		public bool RememberMe { get; set; }
	}
}
