using Blog.Core.SeedWorks;
using Blog.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Controllers
{
	public class PostController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public PostController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[Route("posts")]
		public IActionResult Index()
		{
			return View();
		}

		[Route("posts/{categorySlug}")]
		public async Task<IActionResult> ListByCategory([FromRoute] string categorySlug, [FromQuery] int page = 1)
		{
			var posts = await _unitOfWork.Posts.GetPostsByCategory(categorySlug, page, 1);
			var category = await _unitOfWork.PostCategories.GetBySlug(categorySlug);
			return View(new PostListByCategoryViewModel
			{
				Category = category,
				Posts = posts
			});
		}

		[Route("tag/{tag}")]
		public IActionResult ListByTag([FromRoute] string tag, [FromQuery] int? page = 1)
		{
			return View();
		}

		[Route("post/{slug}")]
		public IActionResult Detail(string slug)
		{
			return View();
		}
	}
}
