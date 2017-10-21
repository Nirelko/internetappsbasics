using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reviews.Models
{
    public class Category : BaseModel
    {
        [Required]
        [DisplayName("Category")]
        public string Name { get; set; }

        public virtual List<Review> Reviews { get; set; }
    }
}