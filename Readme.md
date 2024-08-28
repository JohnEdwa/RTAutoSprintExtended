# RT AutoSprint Extended 2 | Game ver. 1.3.1

## [**For custom survivor/skill support, see RTAutoSprintAddon.**](https://thunderstore.io/package/JohnEdwa/RTAutoSprintAddon/)


### Latest changes

'3.0.0-BETA'

* Updated for Seekers of the Storm, doesn't have the new characters yet.

# Description

Removes the need to manually sprint, instead always sprint when the base game would allow you to. Hold sprint to walk and cancel skills like flamethrower. To supplement that, the useless sprinting crosshair is also disabled.

Kudos to Relocity and Thrawnarch for creating the original mod, and Rein, Harb, ThinkInvisible, Twiner son of Twine, and Aaron (among others) from the modding Discord for help.

## Features, Changes and Options.

* Automatically sprints whenever the game would allow you to - including while charging Artificer or Engineer main attacks.
* Show the correct crosshair while sprinting instead of the useless chevron.

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

### Debug

* `PrintEntityStates: true/[false]` : Prints all the know EntityStates to the BepInEx console for use with custom survivors and AutoSprintAddon.

### Movement

* `HoldSprintToWalk: [true]/false` : True: Holding sprint makes you walk | False: tapping sprint toggles autosprinting on and off.

### Visual

* `DisableSprintingCrosshair: [true]/false`: Disables the useless special sprinting chevron crosshair.

## Changelog

Full changelog can be found in [CHANGELOG.MD](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa#7903).