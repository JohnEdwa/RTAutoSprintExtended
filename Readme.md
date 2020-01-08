# Auto Sprint Extended
#### Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch
#### Made and tested for RoR2 build-ID 4478858 - Hidden Realms Update

The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended) If you find something that doesn't work, please open an issue on Github.   
Kudos to Relocity and Thrawnarch for creating the original mod, and Rein and Harb (among many others) from the modding discord for help.

## The basics

Your character will automatically sprint whenever the base game would allow you to sprint. This includes sprinting during some less obvious situations, such as when charging certain attacks or immediately after using utility skills.   
The special (useless) sprinting crosshair is disabled, and holding sprint now makes you walk instead, if for some reason you wish to do so.

## Features / Differences to the old version
    * Works, for one thing
    * Shows the correct crosshair while sprinting, instead of the useless chevron.
    * FOV, Sprinting FOV and Speedlines can be configured.
    * MUL-T autosprints while using the Buzzsaw or charging the Rebar Launcher.
    * Added REX, Loader and Acrid with proper autosprinting.
    * Commando, Huntress and Engineer can autosprint during more skills.
    * Artificer Flamethrower mode is configurable between the default Toggle and a "Hold to Cast".

## Configuration
    * `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode.
      * Console Command: `rt_artificer_flamethrower_toggle`.
    * `HoldSprintToWalk: [true]/false`: Holding down sprint disable auto-sprinting, forcing the character to walk.
    * `DisableSprintingCrosshair: [true]/false`: Disables the sprinting chevron crosshair.
    * `DisableSpeedlines: [true]/false`: Disables the speedlines effect shown when sprinting. 
    * `DisableFOVChange: [true]/false`: Disables the FOV change when you start to sprint.  
    * `SprintFOVMultiplier: [1.3] 0.5-2.0`: Sets the Sprinting FOV multiplier.
    * `CustomFOV: [60], 1-359`: Sets a custom (vertical) FOV. 60V is roughly 90H.
      * Console Command: `rt_fov`. 
    * `AnimationCancelDelay: [0.2], 0.0-1.0 `: How long to wait after attack button is released to animation cancel and sprint anyway. 


## Known Issues

    * Acrid M1 animation cancelling is even more broken.
    * Arti flamethrower has a small animation glitch.
    * Backup Magazines might cause a small sprint glitch/FOV change between casts.
    * Custom survivors block sprinting based on button input, as long as Primary, Secondary or Special is held down you won't sprint.

## To-Do

    * Make all config console commands update at runtime.
    * Figure a better way to handle custom survivors.
    * Add an option to disable REX burrowing under the ground on sprint.

## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa).

## Changelog

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