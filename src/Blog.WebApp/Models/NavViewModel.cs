namespace Blog.WebApp.Models
{
	public class NavViewModel
	{
		public string Slug { get; set; }
		public string Name { get; set; }
		public List<NavViewModel> Children { get; set; } = new List<NavViewModel>();
		public bool HasChildren
		{
			get
			{
				return Children.Count > 0;
			}
		}
	}
}
