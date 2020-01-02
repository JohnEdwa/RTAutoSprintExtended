# Auto Sprint Extended
#### Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch

The original hasn't been updated in 8 months and is very broken. This is a fixed version built for the Hidden Realms update, build ID 4478858.
The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended).
If you find something that doesn't work, please open an issue on Github.

## The basics

Your character will automatically sprint whenever the base game would allow you to sprint, so you do not have to do it manually. This includes sprinting during some less obvious situations, such as when charging certain attacks or immediately after using utility skills.   
The special (useless) sprinting crosshair is disabled, and holding sprint now makes you walk instead, if for some reason you wish to do so.

## Features / Differences to the old version
* Works, for one thing
* Shows the correct crosshair while sprinting, instead of the useless chevron.
* MUL-T autosprints while using the Buzzsaw or charging the Rebar Launcher.
* Added REX, Loader and Acrid with proper autosprinting.
* Commando, Huntress and Engineer can autosprint during more skills.
* Artificer Flamethrower mode is configurable between the default Toggle and a "Hold to Cast".

## Configuration
* `HoldSprintToWalk: [true]/false`: Holding down sprint disable auto-sprinting, forcing the character to walk.
* `DisableSprintingCrosshair: [true]/false`: Disables the sprinting chevron crosshair.
* `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode. Has a console command so you can change it on the fly: `rt_artiflamemode: `. `toggle/true/1`, or `hold/false/0`.

## Known Issues

* Few skills block spinting simply based on the button input, and holding it down will force you to walk even when the skill is on cooldown. Notable ones are:
 * Artificer: Special, Utility.
 * Huntress: Special.
 * Custom/New Survivors: Primary, Secondary, Special.
* MUL-T: Scrap Launcher sprints briefly between every shot which gets really annoying/nausiating.
* Acrid: M1 animation cancelling is even more annoying, as this mod immediately sprints. #Wontfix, waiting for the official one first.

## To-Do (hopefully)

* Fix the FOV/Sprint blinking with cooldown-between-casts skills such as MUL-T Scrap Launcher.

## Changelog

`4478858.1.1`
 * Cleaned/rewrote the code. 
 * Added and fixed support for all survivors.
 * Removed the useless sprinting crosshair.
 * Added special workarounds to allow REX, MUL-T and Artificer to sprint even more.
 * Added Config items for disabling crosshair and enabling walking.

`4478858.1.0` - Initial Version (Based on 1.0.5), fixed, and added Artificer flamethrower config.

---

[//]: # (Thanks to FunkFrog and Sipondo for letting us use their README as a basis for this one. You're doing god's work.)

# Auto Sprint
#### By Relocity and Thrawnarch

This mod has been developed to allow easy sprinting, without affecting game balance.

## Features

Whenever you could be sprinting in the base game, this mod makes you sprint. Includes special cases:

- You stop sprinting briefly when attacking.

- Huntress and Merc sprint while using basic (LMB) skills.

- You sprint while charging Artificer M2 or Engineer M1, after a very brief pause.

- Does not cancel channeled skills like the Artificer ult.

## Installation Guide

- Copy the 'RTAutoSprintEx.dll' file to your BepInEx plugins folder.

- Never press sprint again.

- (Optional) unbind your sprint button.

## Known Issues

- At the moment, any custom classes will not allow sprinting during skill use, except for the Utility skill (ie. the commando is the default sprinting behavior).

### Changelog

`1.0.5` - Hopefully fixed an issue that was throwing a ton of NullReferenceException reports, creating a ton of lag in multiplayer games.

`1.0.4` - No longer cancels Artificer ult skill!

`1.0.3` - Added a callout for a known issue with the Artificer's ult skill.

`1.0.2` - Clarified readme, fixed formatting in changelog, we hope.

`1.0.1` - Cleaned up code slightly.

`1.0.0` - Initial release.