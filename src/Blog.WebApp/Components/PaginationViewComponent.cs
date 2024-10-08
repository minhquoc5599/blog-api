using Blog.Core.Models.Base;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Components
{
	public class PaginationViewComponent : ViewComponent
	{
		public Task<IViewComponentResult> InvokeAsync(PagingBaseResponse response)
		{
			return Task.FromResult((IViewComponentResult)View("Default", response));
		}
	}
}
