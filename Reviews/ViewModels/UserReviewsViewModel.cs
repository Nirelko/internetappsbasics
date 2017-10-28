using System.ComponentModel;

namespace Reviews.ViewModels
{
    public class UserReviewsViewModel : BaseViewModel
    {
        [DisplayName("User Name")]
        public string UserName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Review")]
        public string Title { get; set; }
    }
}