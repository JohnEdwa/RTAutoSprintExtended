# Changelog

`1.3.3` [2021-04-20]

* Fixed captains alt airstrike.

`1.3.2` [2021-04-06]

* Forgot to add command helper back.

`1.3.0` [2021-04-06]

* Back to R2API. Otherwise identical to 1.2.1

`1.2.1` [2021-04-03]

* MUL-T dualwield should now work properly.

`1.2.0` [2021-03-30]

* Updated for Anniversary, switched from R2API to EnigmaticThunder.
* [KNOWN ISSUE] Bandits revolver has a winddown animation which blocks autosprinting. Shooting or tapping sprint will cancel this.

`1.1.3` [2020-10-08]

* Added `ToggleAutoSprint` option, with it enabled pressing Sprint will toggle between walking and auto-sprinting.

`1.1.2` [2020-09-03]

* Fixed another MUL-T weapon swap bug.
* Updated the config handling a little.
* Updated the readme and moved old changelog to [CHANGELOG.md](https://github.com/JohnEdwa/RTAutoSprintExtended/blob/master/CHANGELOG.md).

`1.1.1` [2020-08-23]

* Fixed an issue causing MUL-T sprinting to break if you swapped from the nailgun to another weapon while firing.

`1.1.0` [2020-08-22]

* Slightly better custom survivor support, the config file can be used to disable AutoSprinting for survivors that break.

`1.0.8` [2020-08-18]

* Fixed some 1.0 issues.
* Added Captain.
* Removed the very poor partial support for unspecified/custom survivors as it just breaks things.

`1.0.7` [unreleased]

* Following skills should now correctly cancel or cast when you tap sprint: Engineer Harpoon Launcher, Artificer Ice Wall, MUL-T Blast Canister, REX Drill/Seed Barrage
* Added a config for a 360 sprinting cheat on request. Enable from the config or using the console command `rt_sprintcheat`.

`1.0.6` [2020-04-04]

* Hopefully fixed sprinting sometimes not being activated again correctly.

`1.0.5` [2020-04-03]

* Artifacts update release - Added Engineers Harpoon skill.
* Fixed Console Commands.

`1.0.4` [2020-01-26]

* Tiny tweak to the IL codes to remove an incompatibility with Rein's Rogue Wisp survivor.

`1.0.3` [2020-01-09]

* Removed a debug F2 disable, whoops.
* Edited readme to (maybe) work with the Thunderstore Markdown parser.

`1.0.2` [2020-01-09]

* Added FOV configuration and disabling speedlines.
* Arti, Engi, Commando and Loader use the animation duration instead of a static delay between shots.
* Automatic animation cancellation for the above.
* Added MUL-T Scrap Launcher logic.
* Fixed the readme and manifest version.
* Other small tweaks.

`1.0.1` [2020-01-02]

* Initial release.

`0.2.0`

* Cleaned/rewrote the code. 
* Added and fixed support for all survivors.
* Removed the useless sprinting crosshair.
* Added special workarounds to allow most survivors to sprint during more skills.
* Added config items for disabling crosshair and enabling walking.

`0.1.0` 

* Initial Version (Based on RT AutoSprint 1.0.5), fixed, and added Artificer flamethrower config.