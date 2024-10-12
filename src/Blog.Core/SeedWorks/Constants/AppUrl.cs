namespace Blog.Core.SeedWorks.Constants
{
	public static class AppUrl
	{
		// User
		public static string Login = "/login";
		public static string Register = "/register";
		public static string Profile = "/profile";
		public static string EditProfile = "/profile/edit";
		public static string ChangePassword = "/profile/change-password";
		public static string ForgotPassword = "/forgot-password";
		public static string ResetPassword = "/reset-password";

		// Page
		public static string Home = "/";
		public static string About = "/about";
		public static string Posts = "/posts";
		public static string Contact = "/contact";
		public static string PostByCategory = "/posts/{0}";
		public static string PostDetail = "/post/{0}";
		public static string PostByTag = "/tag/{0}";
		public static string Series = "/series";
		public static string SeriesDetail = "/series/{0}";
		public static string CreatePost = "/post/create";
		public static string PostByUser = "/posts/list";

		// Author
		public static string Author = "/author/{0}";
		
	}
}
