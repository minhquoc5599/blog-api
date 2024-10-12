using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Blog.WebApp.Models.Post
{
	public class CreatePostViewModel
	{
		[Required(ErrorMessage = "Title is required")]
		public required string Title { get; set; }
		public string? Description { get; set; }
		public string? Content { get; set; }
		public string? ThumbnailImage { get; set; }
		public Guid CategoryId { get; set; }
		public SelectList? CategoryList { get; set; }
		public string? SeoDescription { get; set; }
	}
}
