using MediatR;

namespace Blog.WebApp.Helpers.Events.Register
{
	public class RegisterEvent: INotification
	{
		public string UsernName { get; set; }
		public RegisterEvent(string UserName)
		{
			UsernName = UserName;
		}
	}
}
