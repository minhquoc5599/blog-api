using System.ComponentModel;

namespace Blog.Core.SeedWorks.Constants
{
    public static class Permissions
    {
        public static class Dashboard
        {
            [Description("View dashboard")]
            public const string View = "Permissions.Dashboard.View";
        }

        public static class Roles
        {
            [Description("View roles")]
            public const string View = "Permissions.Roles.View";

            [Description("Create role")]
            public const string Create = "Permissions.Roles.Create";

            [Description("Edit role")]
            public const string Edit = "Permissions.Roles.Edit";

            [Description("Delete role")]
            public const string Delete = "Permissions.Roles.Delete";
        }

        public static class Users
        {
            [Description("View users")]
            public const string View = "Permissions.Users.View";

            [Description("Create user")]
            public const string Create = "Permissions.Users.Create";

            [Description("Edit user")]
            public const string Edit = "Permissions.Users.Edit";

            [Description("Delete user")]
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class PostCategories
        {
            [Description("View post categories")]
            public const string View = "Permissions.PostCategories.View";

            [Description("Create post categories")]
            public const string Create = "Permissions.PostCategories.Create";

            [Description("Edit post categories")]
            public const string Edit = "Permissions.PostCategories.Edit";

            [Description("Delete post categories")]
            public const string Delete = "Permissions.PostCategories.Delete";
        }

        public static class Posts
        {
            [Description("View posts")]
            public const string View = "Permissions.Posts.View";

            [Description("Create post")]
            public const string Create = "Permissions.Posts.Create";

            [Description("Edit post")]
            public const string Edit = "Permissions.Posts.Edit";

            [Description("Delete post")]
            public const string Delete = "Permissions.Posts.Delete";

            [Description("Approve post")]
            public const string Approve = "Permissions.Posts.Approve";
        }

        public static class Series
        {
            [Description("View series")]
            public const string View = "Permissions.Series.View";

            [Description("Create series")]
            public const string Create = "Permissions.Series.Create";

            [Description("Edit series")]
            public const string Edit = "Permissions.Series.Edit";

            [Description("Delete series")]
            public const string Delete = "Permissions.Series.Delete";
        }

        public static class Royalty
        {
            [Description("View royalty")]
            public const string View = "Permissions.Royalty.View";
            [Description("Pay royalty")]
            public const string Pay = "Permissions.Royalty.Pay";
        }
    }
}
