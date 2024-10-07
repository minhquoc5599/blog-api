using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Components
{
	public class NavViewComponent: ViewComponent
	{
		public async Task<IViewComponentResult> InvokeAsync()
		{
			return View();
		}
	}
}
