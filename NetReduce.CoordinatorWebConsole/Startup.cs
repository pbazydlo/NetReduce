using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NetReduce.CoordinatorWebConsole.Startup))]
namespace NetReduce.CoordinatorWebConsole
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
