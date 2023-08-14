using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Doorprize.Startup))]
namespace Doorprize
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
