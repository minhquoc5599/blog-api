using MediatR;

namespace Blog.WebApp.Helpers.Events.Login
{
	public class LoginEvent : INotification
	{
		public string UsernName { get; set; }
		public LoginEvent(string UserName)
		{
			UsernName = UserName;
		}
	}
}
