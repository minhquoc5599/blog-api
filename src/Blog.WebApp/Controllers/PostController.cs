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

			var viewModel = new PostListByCategoryViewModel
			{
				Category = category,
				Posts = posts
			};
			return View(viewModel);
		}

		[Route("tag/{tagSlug}")]
		public async Task<IActionResult> ListByTag([FromRoute] string tagSlug, [FromQuery] int page = 1)
		{
			var posts = await _unitOfWork.Posts.GetPostsByTag(tagSlug, page, 1);
			var tag = await _unitOfWork.Tags.GetTagBySlug(tagSlug);

			var viewModel = new PostListByTagViewModel
			{
				Tag = tag,
				Posts = posts
			};
			return View(viewModel);
		}

		[Route("post/{slug}")]
		public async Task<IActionResult> Detail(string slug)
		{
			var post = await _unitOfWork.Posts.GetPostBySlug(slug);
			var category = await _unitOfWork.PostCategories.GetBySlug(post.CategorySlug);
			var tags = await _unitOfWork.Posts.GetDetailTagsByPostId(post.Id);

			var viewModel = new PostDetailViewModel
			{
				Post = post,
				Category = category,
				Tags = tags
			};
			return View(viewModel);
		}
	}
}
