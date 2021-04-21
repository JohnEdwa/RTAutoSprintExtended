# RT AutoSprint Extended 2 | Game ver. 1.1.1.4

## Changelog

`2.0.0`  [2021-04-xx]

* Complete rewrite of the mod. Now allows me to add support for custom survivors, if you know of any skills that don't work, contact me (see below).
* [known issue] Artificer flamethrower "hold-to-cast" mode isn't implemented.
* [known issue] Disabling the mod only on certain survivors isn't possible.

Rest of the changelog can be found in [CHANGELOG.MD](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

# Description

Removes the need to manually sprint, instead always sprint when the base game would allow you to. Hold sprint to walk and cancel skills like flamethrower.

To supplement that, the sprinting crosshair and speedlines are removed, and the FOV and FOV sprint expanding can be edited.

Source code can be found [at the Github repo here,](https://github.com/JohnEdwa/RTAutoSprintExtended) if you find something that doesn't work, please open an issue on Github.
Kudos to Relocity and Thrawnarch for creating the original mod, and Rein, Harb and ThinkInvisible (among many others) from the modding Discord for help.

## Features, Changes and Options.

* Automatically sprints whenever the game would allow you to - including while charging Artificer or Engineer main attacks.
* Show the correct crosshair while sprinting instead of the useless chevron.
* Modify the the FOV, sprinting FOV change, and speedline effect in the config.
* Most of the configuration edits can be dones while the game is running, use the `rt_reload` console command to reload the file.

### Mod compatibility patches:

* Artificer Extended, Mando_Gaming, EggsSkills, The House, Playble Templar.    
If you find a skill that either sprints annoyingly between repeated uses, or out right cancels casting/channeling/charging, contact me (see below).

## Configuration

### Movement

    * `HoldSprintToWalk: [true]/false` : True: Holding sprint makes you walk | False: tapping sprint toggles autosprinting on and off.

### Visual

    * `FOVValue [60], 1-180`: Sets a custom (vertical) FOV. 60V is roughly 90H.
    * `SprintFOVMultiplier: [1.3] 0.1-2.0`: Sets the sprinting FOV multiplier. Set to 1 to disable.
    * `DisableSprintingCrosshair: [true]/false`: Disables the useless special sprinting chevron crosshair.
    * `DisableSpeedlines: true/[false]`: Disables the speedlines effect shown when sprinting.

### Misc

    * `DisabledAutoSprinting: true/[false]`: Disables the autosprinting part of the mod.
    * `DisableVisualChanges: true/[false]`: Disables the FOV/visual modification side of the mod.

### Console commands

    * `rt_help`: Prints all the possible commands.
    * `rt_reload`: Reload the RTAutoSprintEx2.cfg configuration file.
    * `rt_sprint_enable true/false`: Enables/Disables the sprinting part of the mod.

## Known Issues / ToDo

* No ability to disable for individual survivors or skills - yet.
* Artificer Flamethrower "Hold to Cast" is not implemented.

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).