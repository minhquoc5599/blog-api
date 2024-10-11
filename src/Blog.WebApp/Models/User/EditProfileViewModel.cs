using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Blog.WebApp.Models.User
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }
    }
}
