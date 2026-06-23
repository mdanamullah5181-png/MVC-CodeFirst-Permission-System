using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_CodeFirst.Startup))]
namespace MVC_CodeFirst
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
