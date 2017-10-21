using System.ComponentModel;

namespace Reviews.ViewModels
{
    public class RecipeCommentViewModel
    {
        public int Id { get; set; }

        [DisplayName("Review")]
        public string Title { get; set; }

        [DisplayName("Number Of Comments")]
        public int NumberOfComment { get; set; }

        [DisplayName("Author")]
        public string AuthorFullName { get; set; }
    }
}