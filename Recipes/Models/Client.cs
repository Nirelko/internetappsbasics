using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Recipes.Models
{
    public class Client : BaseModel
    {
        public Gender Gender { get; set; }

        [Required]
        [DisplayName("Username")]
        public string ClientName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }

        [DisplayName("Administrator")]
        public bool IsAdmin { get; set; }

        public virtual List<Comment> Comments { get; set; }

        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}