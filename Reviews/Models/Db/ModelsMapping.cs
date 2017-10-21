using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Reviews.Models.Db
{
    public class ModelsMapping : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Recipes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}