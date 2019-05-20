using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Alocacao.Startup))]
namespace Alocacao
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
