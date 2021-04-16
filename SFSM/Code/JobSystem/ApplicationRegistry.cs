using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace SFServerManager.Code.JobSystem
{
    public class ApplicationRegistry : Registry
    {
        // Inject the application service provider using pure DI.
        // (see Startup.cs code example)
        public ApplicationRegistry(IServiceProvider sp)
        {
            Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<ServerStatusChecker>()).NonReentrant().ToRunEvery(1).Minutes();
        }
    }

}
