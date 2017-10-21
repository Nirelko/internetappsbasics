using Microsoft.Owin;
using Owin;
using Reviews;

[assembly: OwinStartup(typeof(Startup))]
namespace Reviews
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
