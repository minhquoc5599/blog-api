namespace Blog.Core.SeedWorks.Constants
{
	public static class AppUrl
	{
		// User
		public static string Login = "/login";
		public static string Register = "/register";
		public static string Profile = "/profile";
		public static string ChangePassword = "/change-password";

		// Page
		public static string Home = "/";
		public static string About = "/about";
		public static string Posts = "/posts";
		public static string Contact = "/contact";
		public static string PostByCategory = "/posts/{0}";
		public static string PostDetail = "/post/{0}";
		public static string PostByTag = "/tag/{0}";

		// Author
		public static string Author = "/author/{0}";
		
	}
}
