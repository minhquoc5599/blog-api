using Blog.Core.Configs;
using Blog.Core.Domain.Content;
using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Blog.WebApp.Helpers.Constants;
using Blog.WebApp.Helpers.Extensions;
using Blog.WebApp.Models.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Blog.WebApp.Controllers
{
	public class PostController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<AppUser> _userManager;
		private readonly AppSettings _appSettings;
		public PostController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
			IOptions<AppSettings> appSettings)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_appSettings = appSettings.Value;
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

		[HttpGet]
		[Route("post/create")]
		[Authorize]
		public async Task<IActionResult> Create()
		{
			return View(await InitCreatePostViewModel());
		}

		[HttpPost]
		[Route("post/create")]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([FromForm] CreatePostViewModel model, IFormFile thumbnail)
		{
			if (!ModelState.IsValid)
			{
				return View(await InitCreatePostViewModel());
			}

			var userId = User.GetUserId();
			var user = await _userManager.FindByIdAsync(userId.ToString());
			var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(model.CategoryId);
			var post = new Post
			{
				Name = model.Title,
				CategoryName = postCategory.Name,
				CategorySlug = postCategory.Slug,
				Slug = TextExtension.ToUnsignedString(model.Title),
				CategoryId = model.CategoryId,
				Content = model.Content,
				SeoDescription = model.SeoDescription,
				Status = PostStatus.Draft,
				AuthorUserId = userId,
				AuthorName = user.GetFullName(),
				AuthorUserName = user.UserName,
				Description = model.Description,
			};
			_unitOfWork.Posts.Add(post);

			if (thumbnail != null)
			{
				await UploadThumbnail(thumbnail, post);
			}
			var result = await _unitOfWork.CompleteAsync();
			if (result > 0)
			{
				TempData[AppConstant.SuccessFormMessage] = "Create post successfully.";
				return Redirect(AppUrl.Profile);
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Create post failed");
			}
			return View(model);
		}

		[HttpGet]
		[Route("posts/list")]
		public async Task<IActionResult> ListByUser(string keyword, int page = 1)
		{
			var posts = await _unitOfWork.Posts.GetPostsByUserId(keyword, User.GetUserId(), page, 2);
			return View(new PostListByUserViewModel()
			{
				Posts = posts
			});
		}

		private async Task<CreatePostViewModel> InitCreatePostViewModel()
		{
			var model = new CreatePostViewModel()
			{
				Title = "Untitled",
				CategoryList = new SelectList(await _unitOfWork.PostCategories.GetAllAsync(), "Id", "Name")
			};
			return model;
		}

		private async Task UploadThumbnail(IFormFile thumbnail, Post post)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(_appSettings.BackendApiUrl);
				byte[] data;
				using (var br = new BinaryReader(thumbnail.OpenReadStream()))
				{
					data = br.ReadBytes((int)thumbnail.OpenReadStream().Length);
				}
				var bytes = new ByteArrayContent(data);
				var multiContent = new MultipartFormDataContent
				{
					{ bytes, "file", thumbnail.FileName }
				};
				var uploadResult = await client.PostAsync("api/admin/media?type=posts", multiContent);
				if (uploadResult.StatusCode != HttpStatusCode.OK)
				{
					ModelState.AddModelError("", await uploadResult.Content.ReadAsStringAsync());
				}
				else
				{
					var path = await uploadResult.Content.ReadAsStringAsync();
					var pathObj = JsonSerializer.Deserialize<UploadResponse>(path);
					post.Thumbnail = pathObj?.Path;
				}
			}
		}
	}
}
