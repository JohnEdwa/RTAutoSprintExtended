# Auto Sprint Extended
#### Fixed by JohnEdwa, Original by Relocity and Thrawnarch

A fork/reverse engineered/extended RT AutoSprint. The original hasn't been updated in 8 months and is very broken, and there is no source or contact info.   
This is a fixed version for the Hidden Realms update, build ID 4478858.


## Differences to the old version
* Works, for one thing
* Shows the correct crosshairs, instead of the useless sprinting chevron.
* Commando, Huntress and Engineer autosprint during more skills (still something a player could do by themselves).
* Added REX, Loader and Acrid with proper autosprinting.
* Artificer Flamethrower mode is configurable.
* Engineer can be allowed to sprint between deploying mines.

## Configuration
* Config file item: `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode. Has a console command so you can change it on the fly: `rt_artiflamemode: `. `toggle/true/1`, or `hold/false/0`.
* Config file item: `EngineerAllowM2Sprint: true/[false]`: When true, sprints between laying mines. Looks janky, but technically possible to do. As a positive, holding M2 doesn't disable sprinting anymore.

## Known Issues

* Acrid M1 animation cancellation is very pronounced with this mod. Waiting for the official fix before trying for a workaround.
* MUL-T Stun Grenade (M2) starts sprinting only after the put-away animation (~1s delay), although he could start right after firing.

## To-Do (hopefully)

* Change holding sprint to a walk button.
* Disable the generic sprinting crosshair.

## Changelog

`4478858.1.1`
 * Cleaned the code. 
 * Added REX, Acrid and Loader, fixed Commando, Huntress, Engi not being allowed to autosprint during some skills they are able to. 
 * MUL-T primary weapons autosprint when possible.
 * Added Engi M2 config option.
 * Removed the useless Spriting crosshair.

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