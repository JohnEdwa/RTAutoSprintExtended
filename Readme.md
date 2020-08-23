# RT AutoSprint Extended | BuildID: 5381045
### Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch

The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended) If you find something that doesn't work, please open an issue on Github.   
Kudos to Relocity and Thrawnarch for creating the original mod, and Rein and Harb (among many others) from the modding Discord for help.

## The basics

Your character will automatically sprint whenever the base game would allow you to sprint. This includes sprinting during some less obvious situations, such as when charging certain attacks or immediately after using utility skills.   
The special (useless) sprinting crosshair is disabled, and holding sprint now makes you walk instead, if for some reason you wish to do so.

## Features and Changes

* Works, for one thing.
* Shows the correct crosshair while sprinting instead of the useless chevron.
* You can modify the the FOV, sprinting FOV change, and speedline effect in the config. 
* Should work for all default survivors, many of which can auto-sprint while charging skills even though starting the cast normally makes you walk.
* Automatic animation cancelling when you stop attacking - no need to manually press "Sprint".
* Artificer Flamethrower mode is configurable between the default Toggle and a "Hold to Cast".

## Configuration

* `CustomSurvivorDisable: []` : List of custom survivors names that are disabled. The name is printed to the chat and log at spawn. Example: "CustomSurvivorDisable: = SNIPER_NAME AKALI GOKU". 
* `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode.
* `HoldSprintToWalk: [true]/false`: Holding down sprint disable auto-sprinting, forcing the character to walk.
* `SprintInAnyDirection: true/[false]`: Allows you to sprint in any direction.
* `DisableSprintingCrosshair: [true]/false`: Disables the useless special sprinting chevron crosshair.
* `DisableSpeedlines: true/[false]`: Disables the speedlines effect shown when sprinting. 
* `DisableFOVChange: true/[false]`: Disables the FOV change when you sprint.  
* `SprintFOVMultiplier: [1.3] 0.5-2.0`: Sets the Sprinting FOV multiplier, if not disable with the above setting.
* `CustomFOV: [60], 1-359`: Sets a custom (vertical) FOV. 60V is roughly 90H.
* `AnimationCancelDelay: [0.2], 0.0-1.0 `: How long to wait after attack button is released to animation cancel and sprint anyway.
* Console Command: `rt_help` will list all possible console commands.
* Console Command: `rt_enabled` can be used to disable/enable most of the sprinting functionality.
* Console Command: `rt_fov` changes CustomFOV.
* Console Command: `rt_artificer_flamethrower_toggle` sets the flamethrower mode.
* Console Command: `rt_sprintcheat` disables angle checking and allows you to sprint in any direction.

## Known Issues

* Visions of Heresy will animation cancel as you sprint between every shot.
* Acrid M1 animation cancelling is even more broken than vanilla.
* Arti flamethrower has a small animation glitch at the start.
* Custom survivors aren't supported, but the mod now shouldn't break them either.

## To-Do

* Rewrite the mod to work from detecting what skill is being used, instead of editing each skill.
* Make all config console commands update at runtime.
* Figure a ~~better~~ way to handle custom survivors.
* Add an option to disable REX burrowing under the ground on sprint.

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).

## Changelog

`1.1.1`

* Fixed an issue causing MUL-T sprinting to break if you swapped from the nailgun to another weapon while firing.

`1.1.0`

* Slightly better custom survivor support, the config file can be used to disable AutoSprinting for survivors that break.

`1.0.8`

* Fixed some 1.0 issues.
* Added Captain.
* Removed the very poor partial support for unspecified/custom survivors as it just breaks things.

`1.0.7`

* Following skills should now correctly cancel or cast when you tap sprint: Engineer Harpoon Launcher, Artificer Ice Wall, MUL-T Blast Canister, REX Drill/Seed Barrage
* Added a config for a 360 sprinting cheat on request. Enable from the config or using the console command `rt_sprintcheat`.

`1.0.6`

* Hopefully fixed sprinting sometimes not being activated again correctly.

`1.0.5`

* Artifacts update release - Added Engineers Harpoon skill.
* Fixed Console Commands.

`1.0.4`

* Tiny tweak to the IL codes to remove an incompatibility with Rein's Rogue Wisp survivor.

`1.0.3`

* Removed a debug F2 disable, whoops.
* Edited readme to (maybe) work with the Thunderstore Markdown parser.

`1.0.2`

* Added FOV configuration and disabling speedlines.
* Arti, Engi, Commando and Loader use the animation duration instead of a static delay between shots.
* Automatic animation cancellation for the above.
* Added MUL-T Scrap Launcher logic.
* Fixed the readme and manifest version.
* Other small tweaks.

`1.0.1`

* Initial release.

`0.2.0`

* Cleaned/rewrote the code. 
* Added and fixed support for all survivors.
* Removed the useless sprinting crosshair.
* Added special workarounds to allow most survivors to sprint during more skills.
* Added config items for disabling crosshair and enabling walking.

`0.1.0` 

* Initial Version (Based on RT AutoSprint 1.0.5), fixed, and added Artificer flamethrower config.