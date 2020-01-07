# Auto Sprint Extended
#### Fixed & Extended by JohnEdwa, Original by Relocity and Thrawnarch
#### Made and tested for RoR2 build-ID 4478858 - Hidden Realms Update

The code can be found [at the Github repo here.](https://github.com/JohnEdwa/RTAutoSprintExtended).
If you find something that doesn't work, please open an issue on Github.   
Kudos to Relocity and Thrawnarch for creating the original mod.

## The basics

Your character will automatically sprint whenever the base game would allow you to sprint. This includes sprinting during some less obvious situations, such as when charging certain attacks or immediately after using utility skills.   
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
* `ArtificerFlamethrowerToggle: [true]/false`: Sets the flamethrower mode. Default is toggle cancellable by pressing the sprint key, alternative is a "Hold to cast" mode. Has a console command so you can change it on the fly: `rt_artiflamemode`: `toggle/true/1` or `hold/false/0`.
* `AnimationCancelDelay: 0.0 [0.2] 1.0 `: How long to wait after attack button is released to animation cancel and sprint anyway.

## Known Issues

* Acrid M1 animation cancelling is even more broken.
* Custom survivors block sprinting based on button input, as long as Primary, Secondary or Special is held down you won't sprint.

## To-Do

* Figure a better way to handle custom survivors.
* Add an option to disable REX burrowing under the ground on sprint.


## Contact

Open an issue [at the Github repo](https://github.com/JohnEdwa/RTAutoSprintExtended) or find me on the RoR2 modding discord (JohnEdwa).

## Changelog

`1.0.2`
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

`0.1.0` - Initial Version (Based on 1.0.5), fixed, and added Artificer flamethrower config.