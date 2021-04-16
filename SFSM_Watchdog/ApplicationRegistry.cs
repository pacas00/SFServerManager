using System;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace SFSM_Watchdog
{
    public class ApplicationRegistry : Registry
    {
        // Inject the application service provider using pure DI.
        // (see Startup.cs code example)
        public ApplicationRegistry(IServiceProvider sp)
        {
            Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<Jobs.ProcessWatcher>()).NonReentrant().ToRunEvery(1).Minutes();
            
            if (!Configuration.SFSMDetected())
            {
                Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<Jobs.AutoRestart>()).WithName("AutoSave").ToRunEvery(Configuration.AutoRestartMinutesInterval).Minutes();

                // Cheating at math using datetime

                DateTime today    = DateTime.Today.AddHours(7).AddMinutes(30);
                DateTime tomorrow = today.AddDays(1);

                //Primary Schedule for restarts
                Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<Jobs.AutoRestart>()).WithName("AutoRestart").ToRunOnceAt(today.Hour, today.Minute).AndEvery(24).Hours();
                today = today.AddMinutes(Configuration.AutoRestartMinutesInterval);

                DateTime now  = DateTime.Now;
                DateTime four = DateTime.Now.AddHours(4);

                if (now < today && today < four)
                {
                    Program.NextAutoRestart = today;
                }

                while (today < tomorrow && today != tomorrow)
                {
                    Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<Jobs.AutoRestart>()).WithName($"AutoRestart_{today.Hour}_{today.Minute}").ToRunOnceAt(today.Hour, today.Minute).AndEvery(24).Hours();

                    if (now < today && today < four)
                    {
                        Program.NextAutoRestart = today;
                    }

                    today = today.AddMinutes(Configuration.AutoRestartMinutesInterval);
                }

            }
        }
    }

}
