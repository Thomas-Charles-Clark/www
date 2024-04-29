using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(wwwDesignersGuild.Startup))]

namespace wwwDesignersGuild
{
    public partial class Startup
    {
        // This comment is the only change being committed to the BugFix_Testing branch.
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            ConfigureAuth(app);
        }
    }
}
