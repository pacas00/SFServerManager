using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentScheduler;
using IniParser;
using IniParser.Model;

namespace SFSM_Watchdog
{
    public static class Configuration
    {
        //Helper

        public static void WriteLine(string s, bool status = false)
        {
            WriteConsole(s, status);
        }
        public static void WriteLine(Exception ex)
        {
            WriteConsole(ex.Message);
            WriteConsole(ex.Source);
            WriteConsole(ex.StackTrace);
            if (ex.InnerException != null)
            {
                WriteConsole("INNER");
                WriteConsole(ex.InnerException.Message);
                WriteConsole(ex.InnerException.Source);
                WriteConsole(ex.InnerException.StackTrace);
            }
        }
        
        /// <summary>
        /// Writes the console.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="Status">if set to <c>true</c> then is written to [status].</param>
        public static void WriteConsole(string s, bool Status = false)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + ": " + s);
            if (Status)
            {
                StatusLine = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + ": " + s;
            }
        }

        //Settings

        public static void CreateSettings()
        {
            //Ok, Make new
            var parser = new FileIniDataParser();
            IniData data = new IniData();

            data["Satisfactory"]["DriveLetter"] = DriveLetter;
            data["Satisfactory"]["FolderPath"] = FolderPath;
            data["Satisfactory"]["ProgramName"] = ProgramName;
            data["Satisfactory"]["SFSM_Installed"] = SFSM_Installed.ToString();
            data["Satisfactory"]["SFSM_Port"] = SFSM_Port.ToString();
            data["Satisfactory"]["GameType"] = GameType.ToString();

            data["Session"]["StartLoc"] = StartLoc.ToString();
            data["Session"]["WorldName"] = WorldName;
            data["Session"]["SaveName"] = SaveName;
            data["Session"]["AdminPassword"] = AdminPassword;

            data["Network"]["MultiHome"] = MultiHome.ToString();
            data["Network"]["LanOnly"] = LanOnly.ToString();

            data["Timers"]["EnableAutoRestart"] = EnableAutoRestart.ToString();
            data["Timers"]["AutoRestartMinutesInterval"] = AutoRestartMinutesInterval.ToString();

            
            data["Web"]["WebListenIP"] = WebListenIP.ToString();
            data["Web"]["WebPort"] = WebPort.ToString();
            data["Web"]["AccessSecret"] = AccessSecret.ToString();

            parser.WriteFile("Configuration.ini", data);
		}

        public static bool LoadSettings()
        {
            if (!File.Exists("Configuration.ini"))
            {
                CreateSettings();
                return false;
            }
            try
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");

                DriveLetter = data["Satisfactory"]["DriveLetter"];
                FolderPath = data["Satisfactory"]["FolderPath"];
                ProgramName = data["Satisfactory"]["ProgramName"];
                SFSM_Installed = bool.Parse(data["Satisfactory"]["SFSM_Installed"]);
                SFSM_Port = int.Parse(data["Satisfactory"]["SFSM_Port"]);
                try
                {
                    if (!Enum.TryParse<GameTypes>(data["Satisfactory"]["GameType"], true, out GameType))
                    {
                        GameType = GameTypes.DedicatedServer;
                    }
                } catch (Exception ex)
                {
                    GameType = GameTypes.DedicatedServer;
                }

                try
                {
                    if (!Enum.TryParse<StartLocations>(data["Session"]["StartLoc"], true, out StartLoc))
                    {
                        StartLoc = StartLocations.Grass_Fields;
                    }
                } catch (Exception ex)
                {
                    StartLoc = StartLocations.Grass_Fields;
                }
                WorldName = data["Session"]["WorldName"];
                SaveName = data["Session"]["SaveName"];
                AdminPassword = data["Session"]["AdminPassword"];

                MultiHome = bool.Parse(data["Network"]["MultiHome"]);
                LanOnly = bool.Parse(data["Network"]["LanOnly"]);

                EnableAutoRestart = bool.Parse(data["Timers"]["EnableAutoRestart"]);
                AutoRestartMinutesInterval = int.Parse(data["Timers"]["AutoRestartMinutesInterval"]);
                
                WebListenIP = (data["Web"]["WebListenIP"]);
                WebPort = int.Parse(data["Web"]["WebPort"]);
                AccessSecret = data["Web"]["AccessSecret"];

                parser.WriteFile("Configuration.ini", data);
			}
            catch (Exception ex)
            {
                //Crap, barf to console and stop.
                Console.WriteLine(ex);
                Program.Restart();
                CreateSettings();
				return false;
            }

            return true;
        }

        internal static bool IsGameRunning()
        {
            if (gameProcess == null)
            {
                return false;
            }

            if (gameProcess.HasExited)
            {
                return false;
            }

            return true;
        }
        internal static void EnsureStarted()
        {
            if (!IsGameRunning())
            {
                JobManager.GetSchedule("WatchDog").Execute();
            }
        }

        #region JobSystem

        public static Process gameProcess = null;


        public static bool FindGame()
        {
            var candidates = Process.GetProcesses().Where(x => ((x.ProcessName.Contains("UE4-Win64-Shipping") ||x.ProcessName.Contains("FactoryGame-Win64-Shipping")) || x.MainWindowTitle.Contains("Satisfactory")) &&
                                                                !x.ProcessName.Contains("Satisfactory Mod Manager") &&
                                                                !x.ProcessName.Contains("Explorer") &&
                                                                !x.ProcessName.Contains("SFSM") &&
                                                                !x.MainWindowTitle.Contains("SFSM") &&
                                                                !x.ProcessName.Contains("SatisfactoryServerManager") &&
                                                                !x.MainWindowTitle.Contains("SatisfactoryServerManager") &&
                                                                !x.MainWindowTitle.Contains("Satisfactory Mod") &&
                                                                !x.MainWindowTitle.Equals("") &&
                                                                !x.HasExited).ToList();
            if (candidates.Count() > 1)
            {
                Configuration.WriteConsole("Something went wrong, we got " + candidates.Count() + " copies of Satisfactory running?");
                return false;
            }
            else if (candidates.Count == 0)
            {
                Configuration.WriteConsole("Satisfactory not running");
                Configuration.gameProcess = null;

                //CrashReportClient.exe
                var candidatesCrashReporter = Process.GetProcesses().Where(x => x.ProcessName.Contains("CrashReportClient") && (x.MainWindowTitle.Contains("Unreal Engine 4 Crash Reporter") || x.MainWindowTitle.Contains("CrashReportClient")));
                if (candidatesCrashReporter.Count() > 0)
                {
                    Configuration.WriteConsole("Killing Crash Reporters");
                    foreach (var CrashReporter in candidatesCrashReporter)
                    {
                        CrashReporter.Kill();
                    }

                }

                return false;
            }
            else
            {
                //We now have a reference to retail.
                if (gameProcess == null)
                {
                    //It was running and we didn't know.
                    //Assume it was manual start.


                    Configuration.gameProcess = candidates[0];
                    Configuration.WriteConsole("Got Instance of " + Configuration.gameProcess.ProcessName + " " + Configuration.gameProcess.MainWindowTitle, true);

                    return true;
                }
                else
                {
                    //Do this both times, but needs to be after a null check
                    Configuration.gameProcess = candidates[0];
                    Configuration.WriteConsole("Got Instance of " + Configuration.gameProcess.ProcessName + " " + Configuration.gameProcess.MainWindowTitle);
                    return true;
                }


            }
        }


        #endregion


        //Game

        public static string DriveLetter = "S";
        public static string FolderPath = "\\SatisfactoryExperimental\\FactoryGame\\Binaries\\Win64";
        public static string ProgramName = "Engine\\Binaries\\Win64\\UE4-Win64-Shipping.exe";
        public static GameTypes GameType = GameTypes.DedicatedServer;
        public static bool SFSM_Installed = false;
        public static int SFSM_Port = 8033;

        public static StartLocations StartLoc = StartLocations.DuneDesert;
        public static string WorldName = "AWholeNewWorld";
        public static string SaveName = "DedicatedServer";
        public static string AdminPassword = "ChangeMeOrElse";

        public static bool MultiHome = false;
        public static bool LanOnly = false;

        public static int AutoRestartMinutesInterval = 4 * 60;
        public static bool EnableAutoRestart = true;

        
        public static string WebListenIP = "0.0.0.0";
        public static int WebPort = 8034;

        public static string AccessSecret = "C366020887DF753F10B335D9E0A031D8";
        public static string StatusLine = "No Status";

        /// <summary>
        /// Gets the map parameters.
        /// </summary>
        /// <returns></returns>
        ///Game/FactoryGame/Map/GameLevel01/Persistent_Level??startloc=DuneDesert?sessionName=Update 3?Visibility=SV_FriendsOnly?loadgame=After Edit?listen
        public static string GetMapParameters()
        {
            if (LanOnly)
            {
                return $"/Game/FactoryGame/Map/GameLevel01/Persistent_Level??startloc={ToString(StartLoc)}?sessionName={WorldName}?Visibility=SV_FriendsOnly?EnableCheats?server?loadgame={SaveName}?listen?bIsLanMatch={LanOnly.ToInt()}?port=7777{(MultiHome ? "?MULTIHOME" : "")}?NOSPLASH{(GameType == GameTypes.DedicatedServer ? "" : "?WinX=0?WinY=0?ResX=640?ResY=480?ConsoleX=0?ConsoleY=481")}";
            }
            return $"/Game/FactoryGame/Map/GameLevel01/Persistent_Level??startloc={ToString(StartLoc)}?sessionName={WorldName}?Visibility=SV_FriendsOnly?loadgame={SaveName}?listen?NOSPLASH{(GameType == GameTypes.DedicatedServer ? "" : "?WinX=0?WinY=0?ResX=640?ResY=480?ConsoleX=0?ConsoleY=481")}";
        }

		// Epic Launcher breaks all params, need to pass 'WinX=0?WinY=0?ResX=640?ResY=480?ConsoleX=0?ConsoleY=481 -log' via its settings.

		public static string GetLaunchParameters()
        {
            return GetMapParameters() + " -server -log"
//#if !RETAIL
//                + " -EpicPortal -epicusername=\"DedicatedServerTest\" -epicuserid=00000000000000000000000000000001 -name=DedicatedServerTest"
//#endif
			;
		}

        

        public static int ToInt(this bool b)
        {
            if (b) return 1;
            return 0;
        }


		//StartLocations
		//Grass Fields
		//Rocky Desert ?
		//Northern Forest ?
        //Northern Desert


        public static void TerminateGame()
        {
            if (gameProcess == null)
            {
                return;
            }
            gameProcess.Kill();
        }

        public static string SFSMHostname()
        {
            return (WebListenIP == "0.0.0.0" ? "localhost" : WebListenIP);
        }

        public static bool SFSMDetected()
        {
            return SFSM_Installed;
        }

        public static string ToString(StartLocations value)
        {
            return value.ToString().Replace("_"," ");
        }
    }

}
