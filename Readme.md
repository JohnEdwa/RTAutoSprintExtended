# Auto Sprint Extended
#### Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch

The original hasn't been updated in 8 months and is very broken. This is a fixed version built for the Hidden Realms update, build ID 4478858.
The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended).

## The basics

This mod will automatically sprint whenever the base game would allow you to sprint, so you do not have to do it manually. As a bonus, it also sprints during some less obvious situations, such as when charging attacks or immediately after using utility skills.

## Features / Differences to the old version
* Works, for one thing
* Shows the correct crosshair while sprinting, instead of the useless chevron.
* MUL-T autosprints while using the Buzzsaw or charging the Rebar Launcher.
* Added REX, Loader and Acrid with proper autosprinting.
* Commando, Huntress and Engineer can autosprint during more skills.
* Artificer Flamethrower mode is configurable between the default Toggle and a "Hold to Cast".
* Engineer can be allowed to sprint between deploying mines.

## Configuration
* Config file item: `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode. Has a console command so you can change it on the fly: `rt_artiflamemode: `. `toggle/true/1`, or `hold/false/0`.
* Config file item: `EngineerAllowM2Sprint: true/[false]`: When true, sprints between laying mines. Looks janky, but technically possible to do. As a positive, holding M2 doesn't disable sprinting anymore.

## Known Issues

* Few skills block spinting simply based on the button input, and holding it down will make you walk even after the skill is on cooldown.
* Acrid: M1 animation cancelling is even more annoying, as this mod immediately sprints. #Wontfix, waiting for the official one first.
* MUL-T: Scrap Launcher sprints briefly between every shot which gets really annoying.

## To-Do (hopefully)

* Allow walking by holding Shift.

## Changelog

`4478858.1.1`
 * Cleaned the code. 
 * Added REX, Acrid and Loader, fixed Commando, Huntress, Engi not being allowed to autosprint during some skills they are able to. 
 * MUL-T primary weapons autosprint when possible.
 * Added Engi M2 config option.
 * Removed the useless sprinting crosshair.

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