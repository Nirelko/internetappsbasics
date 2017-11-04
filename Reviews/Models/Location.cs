using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reviews.Models
{
    public class Location : BaseModel
    {
        [Required]
        public string Address { get; set; }
    }
}