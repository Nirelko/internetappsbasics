using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reviews.Models
{
    public class Comment : BaseModel
    {
        [MaxLength(8000)]
        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created at")]
        public DateTime CreationDate { get; set; }

        [Required]
        public virtual Review Review { get; set; }

        [Required]
        public virtual User User { get; set; }
    }
}