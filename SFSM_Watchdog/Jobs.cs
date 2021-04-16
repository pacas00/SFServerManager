using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentScheduler;
using SFServerManager.Code;

namespace SFSM_Watchdog
{
    public class Jobs
    {
        static bool Warned = false;
        public static void RestartViaSFSM()
		{
            CommandRequest commandRequest = HttpHelper.CreateCommandRequest(Configuration.AccessSecret, "Restart");
            CommandResponse commandResponse = HttpHelper.PostCommandRequest(commandRequest, $"http://{Configuration.SFSMHostname()}:{Configuration.SFSM_Port}/command");
        }

        public class AutoRestart : IJob
        {
            public void Execute()
            {
                if (Configuration.gameProcess == null || (Configuration.gameProcess != null && Configuration.gameProcess.HasExited))
                {
                    //Game Dead, Do nothing
                }
                else
                {
                    RestartViaSFSM();

                    Configuration.WriteConsole("Saving World", true);
					//Ok, We are running.
                    Thread.Sleep(5000);

                    DateTime now = DateTime.Now;
                    Program.NextAutoRestart = now.AddMinutes(Configuration.AutoRestartMinutesInterval);

                    
					Configuration.WriteConsole("Restarting", true);
                    Thread.Sleep(15);
					Configuration.TerminateGame();
                    Thread.Sleep(15);

				}
            }
        }

		public class ProcessWatcher : IJob
        {
            public static bool DontRestart { get; set; } = false;

            public void Execute()
            {
                Configuration.FindGame();
                if (Configuration.gameProcess == null || (Configuration.gameProcess != null && Configuration.gameProcess.HasExited))
                {
                    //BEFORE WE RESTART

                    if (Program.StartTime.AddHours(24) < DateTime.Now)
                    {
                        Program.Restart();
                        return;
                    }

                    //THIS IS HERE SO THE WATCHDOG CAN STILL SELF-RESTART
                    if (DontRestart)
                    {
                        return;
                    }

                    if (Configuration.GameType == GameTypes.DedicatedServer)
                    {
                        //Command Line based startup
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.Arguments        = Configuration.GetLaunchParameters();
                        startInfo.FileName         = Configuration.ProgramName;
                        startInfo.WorkingDirectory = Configuration.DriveLetter + ":" + Configuration.FolderPath;

                        Configuration.WriteConsole("Starting");
                        Configuration.gameProcess = Process.Start(startInfo);
                    }
                    else
                    {
                        //Launch via launcher with arguments
                        if (Configuration.GameType == GameTypes.Steam || Configuration.GameType == GameTypes.SteamEXP)
                        {
                            //steam://run/<id>//<args>/
                            //The //<args> is optional, args are passed to the application as launch parameters.
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            string           Args      = "/C \"start steam://run/526870/\"";
                            startInfo.Arguments        = Args;
                            startInfo.FileName         = "cmd.exe";
                            startInfo.WorkingDirectory = Environment.SystemDirectory;
                            startInfo.UseShellExecute  = false;

                            Configuration.WriteConsole("Starting", true);
                            if (!Warned)
                            {
                                Configuration.WriteConsole("Non-Dedicated Server builds don't support launch arguments. Make sure the SFSM plugin is installed and configured to autostart");
                                Warned = true;
                            }
                            Process.Start(startInfo);

                        }
                        if (Configuration.GameType == GameTypes.EpicEA)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            string           Args      = "/C \"start com.epicgames.launcher://apps/CrabEA?action=launch^&silent=true\"";
                            startInfo.Arguments        = Args;
                            startInfo.FileName         = "cmd.exe";
                            startInfo.WorkingDirectory = Environment.SystemDirectory;
                            startInfo.UseShellExecute  = false;

                            Configuration.WriteConsole("Starting", true);
                            if (!Warned)
                            {
                                Configuration.WriteConsole("Non-Dedicated Server builds don't support launch arguments. Make sure the SFSM plugin is installed and configured to autostart");
                                Warned = true;
                            }
                            Process.Start(startInfo);
                        }
                        if (Configuration.GameType == GameTypes.EpicEXP)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            string           Args      = "/C \"start com.epicgames.launcher://apps/CrabTest?action=launch^&silent=true\"";
                            startInfo.Arguments        = Args;
                            startInfo.FileName         = "cmd.exe";
                            startInfo.WorkingDirectory = Environment.SystemDirectory;
                            startInfo.UseShellExecute  = false;

                            Configuration.WriteConsole("Starting", true);
                            if (!Warned)
                            {
                                Configuration.WriteConsole("Non-Dedicated Server builds don't support launch arguments. Make sure the SFSM plugin is installed and configured to autostart");
                                Warned = true;
                            }
                            Process.Start(startInfo);
                        }

                    }
				}
				else
                {
                    //Ok, We are running.
                }
            }

        }





        //Old Perf commands
        //KeyboardOperations.EnterCommand("r.fog 0");
        //KeyboardOperations.EnterCommand("r.screenpercentage 25");

    }
}
