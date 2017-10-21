using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Recipes.Models.Db
{
    public class ModelsMapping : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}