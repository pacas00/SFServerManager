# SFServerManager
Repo SFServerManager, SFSM_Watchdog and binary builds of the SFServerManager uplugin for Satisfactory.

## Note
This is a work in progress.

Currently, it does not support the following:
* (Watchdog) Official Satisfactory Dedicated Server - Not Released, Support is planned.
* (Watchdog) Multiple Instances on the same host pc- Not Planned, Will accept PRs for this if its wanted.
* (Panel) Multiple Users - Planned
* (Panel) Proper Database - Currently uses LiteDB for testing, undecided on what this will switch to.


## So what can this all do? (Currently)

### SFSM Game Plugin
* Automatically load a save game on startup (based on a file name)
* Automatically save a game every X (default:10) minutes (to that file name)
* (WebCommand) Load a savegame by filename provided
* (WebCommand) Save a savegame by filename provided
* (WebCommand) Quit to Main Menu
* (WebCommand) Quit to Desktop
* (WebCommand) Kick Players
* (WebCommand) List all Players
* (WebCommand) Stop / Restart (Watchdog command, but watchdog will attempt to save and close via the plugin if enabled)
* (PLANNED) - Will use a supplied (APIKey/Access Secret) to validate origin of the command requests. (Mostly compplete, will be enabled once web panel is "done")


### Watchdog
* Start a retail copy of Satisfactory (Steam / Epic)
* Make sure SF stays running
* Handle Start/Stop/Restart/Force Quit commands from Panel
* Forward SFSM Game Plugin commands to the plugin (if enabled)


### Web Panel
* Manage instances of Watchdog enabled servers.
* Show Status from Game Plugin enabled servers (Directly or via Watchdog)
** Show Session name, visiblity and show Session ID (Non-Dedicated Only)
** Show Game Version and SML version (If installed)
** Show Player count
** Show list of Players
** Show list of Plugins
** (Coming Soon) Kick Players via player list.
** (Coming Soon) Save and Load via manage tab
