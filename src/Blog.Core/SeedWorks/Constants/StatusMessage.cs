namespace Blog.Core.SeedWorks.Constants
{
    public static class StatusMessage
    {
        public static class BadRequest
        {
            public const string InvalidRequest = "Invalid request";
            public const string InvalidToken = "Invalid token";
        }

        public static class Unauthorized
        {
            public const string LockedUser = "User has been locked out";
            public const string LoginFailed = "Login failed";
        }

        public static class NotFound
        {
            public const string User = "User does not exist";
            public const string Role = "Role does not exist";
            public const string Post = "Post does not exist";
            public const string PostCategory = "Post category does not exist";
            public const string Series = "Series does not exist";
            public const string PostInSeries = "Post does not exist in series";
        }

        public static class Conflict
        {
            public const string User = "User already exists";
            public const string Post = "Slug exists";
            public const string PostInSeries = "Post already exists in series";
        }

        public const string InternalServerError = "Internal server error";
    }
}
