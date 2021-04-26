# RT AutoSprint Extended 2 | Game ver. 1.1.1.4

## [**For custom survivor/skill support, see RTAutoSprintAddon.**](https://thunderstore.io/package/JohnEdwa/RTAutoSprintAddon/)

---

### Latest changes

`2.0.0`  [2021-04-26]

* Complete rewrite of the mod from scratch. Now with possible support for custom survivors, skills and a kind-of-an-API.
* Custom survivor/skill patch [released as its own addon for compatibility reasons.](https://thunderstore.io/package/JohnEdwa/RTAutoSprintAddon/)
* [known issue] Artificer flamethrower "hold-to-cast" mode isn't implemented.
* [known issue] Disabling the mod only on certain survivors isn't possible.
`2.0.1` Added console logs to the SendMessage receivers.
`2.0.2` Fixed Toggle Sprint mode skill cancelling.

# Description

Removes the need to manually sprint, instead always sprint when the base game would allow you to. Hold sprint to walk and cancel skills like flamethrower.
To supplement that, the sprinting crosshair and speedlines are removed, and the FOV and FOV sprint expanding can be edited.

Kudos to Relocity and Thrawnarch for creating the original mod, and Rein, Harb, ThinkInvisible, Twiner son of Twine, and Aaron (among others) from the modding Discord for help.

## Features, Changes and Options.

* Automatically sprints whenever the game would allow you to - including while charging Artificer or Engineer main attacks.
* Show the correct crosshair while sprinting instead of the useless chevron.
* Modify the the FOV, sprinting FOV change, and speedline effect in the config.
* Most of the configuration edits can be dones while the game is running, use the `rt_reload` console command to reload the file.

### Mod compatibility and "API":

[**For custom survivor/skill support, see RTAutoSprintAddon.**](https://thunderstore.io/package/JohnEdwa/RTAutoSprintAddon/)

How to implement RTAutoSprintEx support for your mod:

Add a soft dependency for ``com.johnedwa.RTAutoSprintEx``, then use SendMessage to send the EntityStates you want RTAutoSprintEx to look out for.
``RT_SprintDisableMessage`` blocks AutoSprinting from activating when the player is in that EntityState.
``RT_AnimationDelayMessage`` looks for a field called ``duration`` to use as a delay - useful for keeping wind-down animations from being immediately cancelled. As an example, here it is in [EntityStates.Mage.Weapon.FireFireBolt](https://user-images.githubusercontent.com/5417183/116014709-4c688200-a63f-11eb-8b25-4b030fe18a17.JPG)

```
[BepInDependency("com.johnedwa.RTAutoSprintEx", BepInDependency.DependencyFlags.SoftDependency)]

...

if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.johnedwa.RTAutoSprintEx")) {
    SendMessage("RT_SprintDisableMessage", "EntityStates.Mage.Weapon.Flamethrower"); 
    SendMessage("RT_AnimationDelayMessage", "EntityStates.Mage.Weapon.FireFireBolt"); 
}
```

## Configuration

### Movement

* `HoldSprintToWalk: [true]/false` : True: Holding sprint makes you walk | False: tapping sprint toggles autosprinting on and off.

### Visual

* `FOVValue [60], 1-180`: Sets a custom (vertical) FOV. 60V is roughly 90H.
* `SprintFOVMultiplier: [1.3], 0.1-2.0`: Sets the sprinting FOV multiplier. Set to 1 to disable.
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

## Changelog

Full changelog can be found in [CHANGELOG.MD](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).