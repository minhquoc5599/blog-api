using MediatR;

namespace Blog.WebApp.Helpers.Events.Register
{
	public class RegisterEventHandle : INotificationHandler<RegisterEvent>
	{
		public Task Handle(RegisterEvent notification, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
