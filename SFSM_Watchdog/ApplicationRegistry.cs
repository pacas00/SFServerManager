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
            Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<Jobs.ProcessWatcher>()).WithName("WatchDog").NonReentrant().ToRunEvery(1).Minutes();
            
            if (!Configuration.SFSMDetected())
            {
                // Cheating at math using datetime

                DateTime today    = DateTime.Now;
                DateTime tomorrow = today.AddDays(1);

                //Restart Watchdog in 24 hours
                Program.NextAutoRestart = tomorrow;

                DateTime now  = DateTime.Now;
                DateTime four = DateTime.Now.AddHours(4);


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
