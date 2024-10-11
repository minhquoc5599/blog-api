using Blog.Core.SeedWorks;
using Blog.WebApp.Models.Series;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Controllers
{
	public class SeriesController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public SeriesController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[Route("series")]
		public async Task<IActionResult> Index([FromQuery] int page = 1)
		{
			var series = await _unitOfWork.Series.GetSeriesAsync(string.Empty, page, 1);
			var viewModel = new SeriesListViewModel
			{
				Series = series
			};
			return View(viewModel);
		}

		[Route("series/{slug}")]
		public async Task<IActionResult> Detail([FromRoute] string slug, [FromQuery] int page = 1)
		{
			var posts = await _unitOfWork.Series.GetPostsInSeries(slug, page);
			var series = await _unitOfWork.Series.GetSeriesBySlug(slug);
			var viewModel = new SeriesDetailViewModel
			{
				Posts = posts,
				Series = series
			};
			return View(viewModel);
		}
	}
}
