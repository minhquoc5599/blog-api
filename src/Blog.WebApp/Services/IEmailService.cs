using Blog.WebApp.Models;

namespace Blog.WebApp.Services
{
	public interface IEmailService
	{
		Task SendEmail(EmailModel emailData);
	}
}
