using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReportingWeb.Startup))]
namespace ReportingWeb
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
