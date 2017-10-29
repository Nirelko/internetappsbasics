using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reviews.Models
{
    public class Review : BaseModel
    {
        [MaxLength(20)]
        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [MaxLength(8000)]
        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created at")]
        public DateTime CreationDate 
        {
            get
            {
                return this.dateCreated.HasValue
                   ? this.dateCreated.Value
                   : DateTime.Now;
            }

            set { this.dateCreated = value; }
        }

        private DateTime? dateCreated = null;

        [Required]
        [DisplayName("Category")]
        public virtual Category Category { get; set; }

        [Required]
        [DisplayName("Posting User")]
        public virtual User User { get; set; }

        public virtual List<Comment> Comments { get; set; }
    }
}