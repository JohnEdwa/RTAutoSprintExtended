# RT AutoSprint Extended 2 | Game ver. 1.1.1.4

[**For custom survivor support, see the RTAutoSprintEx Addon.**](https://thunderstore.io/package/JohnEdwa/RTAutoSprintExAddon/)

---

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).

## Changelog

`2.0.0`  [2021-04-xx]

* Complete rewrite of the mod. Now with possible support for custom survivors, skills and a kind-of-an-API.
* Custom survivor patch released as its own addon.
* *Configuration file reset and renamed to `RTAutoSprintEx2.cfg`.* I tried to make it migrate, I failed. Sorry.
* [known issue] Artificer flamethrower "hold-to-cast" mode isn't implemented.
* [known issue] Disabling the mod only on certain survivors isn't possible.

Rest of the changelog can be found in [CHANGELOG.MD](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

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

You can use SendMessage to register an EntityState to the list of Sprint Disablers and Animation Delayers. 
Add a soft dependency to ensure RTAutoSprintEx is loaded before your mod.

```
[BepInDependency("com.johnedwa.RTAutoSprintEx", BepInDependency.DependencyFlags.SoftDependency)]

if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.johnedwa.RTAutoSprintEx")) {
    SendMessage("RT_SprintDisableMessage", "EntityStates.Mage.Weapon.Flamethrower"); 
    SendMessage("RT_AnimationDelayMessage", "EntityStates.Mage.Weapon.FireFireBolt"); 
}
```

`RT_SprintDisableMessage`  blocks AutoSprinting from activating when the player is in that EntityState.
`RT_AnimationDelayMessage` looks for a field called `duration` to use as a delay - useful for keeping wind-down animations from being immediately cancelled. 

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