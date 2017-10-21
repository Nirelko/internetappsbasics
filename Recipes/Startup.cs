using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Recipes.Startup))]
namespace Recipes
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
