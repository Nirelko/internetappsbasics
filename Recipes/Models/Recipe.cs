using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recipes.Models
{
    public class Recipe : BaseModel
    {
        [Required]
        [ForeignKey("Client")]
        [DisplayName("Posting Client")]
        public int ClientId { get; set; }

        [Required]
        [ForeignKey("Category")]
        [DisplayName("Food Category")]
        public int CategoryId { get; set; }

        [MaxLength(20)]
        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [MaxLength(5000)]
        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created at")]
        public DateTime CreationDate { get; set; }

        public virtual Client Client { get; set; }

        public virtual Category Category { get; set; }

        public virtual List<Comment> Comments { get; set; }
    }
}