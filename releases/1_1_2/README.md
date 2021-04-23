# RT AutoSprint Extended | Game ver. 1.0.1.1

*Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch.*

The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended) If you find something that doesn't work, please open an issue on Github.
Kudos to Relocity and Thrawnarch for creating the original mod, and Rein and Harb (among many others) from the modding Discord for help.

## The Basics

Your character will automatically sprint whenever the base game would allow you to sprint. This includes sprinting during some less obvious situations, such as when charging certain attacks or immediately after using utility skills.
The special (useless) sprinting crosshair is disabled, holding sprint now makes you walk instead, and you can configure FOV and related settings.

## Features and Changes

* Shows the correct crosshair while sprinting instead of the useless chevron.
* You can modify the the FOV, sprinting FOV change, and speedline effect in the config.
* Works for all default survivors, many of which can auto-sprint while charging skills even though starting the cast normally makes you walk.
* Custom survivors have basic support, but issues with some skills are inevitable.
* Automatic animation cancelling when you stop attacking - no need to manually press "Sprint".
* Artificer Flamethrower mode is configurable between the default Toggle and a "Hold to Cast".

## Configuration

### Survivors

* `CustomSurvivorDisable: []` : List of custom survivors names that won't auto-sprint. The name is printed to the chat at spawn. Example: "CustomSurvivorDisable: = SNIPER_NAME AKALI".
* `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode.
* `AnimationCancelDelay: [0.2], 0.0-1.0`: How long to wait after attack button is released to animation cancel and sprint anyway.

### Movement

* `HoldSprintToWalk: [true]/false`: Holding down sprint disable auto-sprinting, forcing the character to walk.
* `SprintInAnyDirection: true/[false]`: (Cheat) Allows you to sprint in any direction.

### Visual

* `CustomFOV: [60], 1-359`: Sets a custom (vertical) FOV. 60V is roughly 90H.
* `DisableFOVChange: true/[false]`: Disables the FOV change when you sprint.  
* `SprintFOVMultiplier: [1.3] 0.1-2.0`: Sets the Sprinting FOV multiplier, if not disable with the above setting.
* `DisableSprintingCrosshair: [true]/false`: Disables the useless special sprinting chevron crosshair.
* `DisableSpeedlines: true/[false]`: Disables the speedlines effect shown when sprinting.

### Console commands

* Console Command: `rt_help` will list all possible console commands.
* Console Command: `rt_enabled` can be used to disable/enable most of the sprinting functionality.
* Console Command: `rt_fov` changes CustomFOV.
* Console Command: `rt_artificer_flamethrower_toggle` sets the flamethrower mode.
* Console Command: `rt_sprintcheat` disables angle checking and allows you to sprint in any direction.

## Known Issues

* Visions of Heresy will animation cancel as you sprint between every shot.
* Acrid M1 animation cancelling is even more broken than vanilla.
* Arti flamethrower has a small animation glitch at the start.
* Custom survivors aren't properly supported, but the mod now shouldn't break them either - if it does, they can be disabled in the config.

## To-Do

* Rewrite the mod to work from detecting what skill is being used, instead of editing each skill.
* Make all config console commands update at runtime.
* Figure a ~~better~~ way to handle custom survivors, specifically a skill-by-skill disable workaround.
* Add an option to disable REX burrowing under the ground on sprint.

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).

## Changelog

`1.2.0` [2020-09-03]

* Fixed another MUL-T weapon swap bug.
* Tweaked the Configs a little.
* Updated the readme and moved old changelog to [CHANGELOG.md](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

`1.1.1` [2020-08-23]

* Fixed an issue causing MUL-T sprinting to break if you swapped from the nailgun to another weapon while firing.

`1.1.0` [2020-08-22]

* Slightly better custom survivor support, the config file can be used to disable AutoSprinting for survivors that break.
