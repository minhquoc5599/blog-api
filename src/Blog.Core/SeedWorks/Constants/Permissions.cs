using System.ComponentModel;

namespace Blog.Core.SeedWorks.Constants
{
    public static class Permissions
    {
        // Display category
        public static class Dashboard
        {
            [Description("View dashboard")]
            public const string View = "Permissions.Dashboard.View";
        }

        public static class System
        {
            [Description("View system")]
            public const string View = "Permissions.System.View";
        }

        public static class Content
        {
            [Description("View content")]
            public const string View = "Permissions.Content.View";
        }

        public static class Report
        {
            [Description("View report")]
            public const string View = "Permissions.Report.View";
        }


        // Api
        public static class Roles
        {
            [Description("View roles")]
            public const string View = "Permissions.Roles.View";

            [Description("View role permissions")]
            public const string ViewRolePermissions = "Permissions.Roles.ViewRolePermissions";

            [Description("Create role")]
            public const string Create = "Permissions.Roles.Create";

            [Description("Edit role")]
            public const string Edit = "Permissions.Roles.Edit";

            [Description("Edit role permissions")]
            public const string EditRolePermissions = "Permissions.Roles.EditRolePermissions";

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

            [Description("Change password current user")]
            public const string ChangePasswordCurrentUser = "Permissions.Users.ChangePasswordCurrentUser";

            [Description("Set password user")]
            public const string SetPassword = "Permissions.Users.SetPassword";

            [Description("Change email")]
            public const string ChangeEmail = "Permissions.Users.ChangeEmail";

            [Description("Assign roles to user")]
            public const string AssignRolesToUser = "Permissions.Users.AssignRolesToUser";
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

            [Description("Submit for approval")]
            public const string SubmitForApproval = "Permissions.Posts.SubmitForApproval";

            [Description("Reject post")]
            public const string Reject = "Permissions.Posts.Reject";

            [Description("Reject Reason")]
            public const string RejectReason = "Permissions.Posts.RejectReason";

            [Description("Get post activity logs")]
            public const string GetPostActivityLogs = "Permissions.Posts.GetPostActivityLogs";

            [Description("Get series")]
            public const string GetSeries = "Permissions.Posts.GetSeries";
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

            [Description("Add post in series")]
            public const string AddPostInSeries = "Permissions.Series.AddPostInSeries";

            [Description("Delete post in series")]
            public const string DeletePostInSeries = "Permissions.Series.DeletePostInSeries";

            [Description("get posts in series")]
            public const string GetPostsInSeries = "Permissions.Series.GetPostsInSeries";
        }

        public static class Royalty
        {
            [Description("Get transactions")]
            public const string GetTransactions = "Permissions.Royalty.GetTransactions";

            [Description("Get royalty report")]
            public const string GetRoyaltyReport = "Permissions.Royalty.GetRoyaltyReport";

            [Description("Pay royalty")]
            public const string Pay = "Permissions.Royalty.Pay";
        }
    }
}
