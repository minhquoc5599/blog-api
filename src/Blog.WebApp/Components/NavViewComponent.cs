using Blog.Core.SeedWorks;
using Blog.WebApp.Models.Nav;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Components
{
	public class NavViewComponent : ViewComponent
	{
		private readonly IUnitOfWork _unitOfWork;
		public NavViewComponent(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = await _unitOfWork.PostCategories.GetAllAsync();
			var navItems = model.Select(x => new NavViewModel()
			{
				Slug = x.Slug,
				Name = x.Name,
				Children = model.Where(x => x.ParentId == x.Id).Select(i => new NavViewModel()
				{
					Slug = x.Slug,
					Name = x.Name,
				}).ToList(),
			}).ToList();
			return View(navItems);
		}
	}
}
