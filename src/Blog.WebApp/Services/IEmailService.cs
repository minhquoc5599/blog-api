using Blog.WebApp.Models.Auth;

namespace Blog.WebApp.Services
{
    public interface IEmailService
	{
		Task SendEmail(EmailModel emailData);
	}
}
