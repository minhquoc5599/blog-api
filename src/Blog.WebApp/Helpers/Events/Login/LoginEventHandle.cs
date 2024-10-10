using MediatR;

namespace Blog.WebApp.Helpers.Events.Login
{
	public class LoginEventHandle : INotificationHandler<LoginEvent>
	{
		public Task Handle(LoginEvent notification, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
