using System.Text.Json.Serialization;

namespace Blog.WebApp.Models.Post
{
	public class UploadResponse
	{
		[JsonPropertyName("path")]
		public string Path { get; set; }
	}
}
