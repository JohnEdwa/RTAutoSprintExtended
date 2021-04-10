
/*

THINGS TO DO:
 Custom survivor disable/config
 Console Commands
 Rewrite Configuration
 Artificer Hold To Cast
*/

/*

internal HashSet<Type> statesWhichDisableSprint = new HashSet<Type>();
public void RegisterSprintDisabler<T>() where T : BaseState {
    statesWhichDisableSprint.Add(typeof(T));
}

public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
    var currentState = targetBody.GetComponent<EntityStateMachine>()?.state;
    if(currentState == null) return false;
    return statesWhichDisableSprint.Contains(currentState.GetType());
}

*/


/*

Skills that cancel on sprint:
	EntityStates.Mage.Weapon.Flamethrower.OnEnter
	EntityStates.Mage.Weapon.PrepWall.OnEnter

	EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.OnEnter

	EntityStates.Engi.EngiMissilePainter.OnEnte
	
	EntityStates.Toolbot.FireNailgun.OnEnter
	EntityStates.Toolbot.AimGrenade.OnEnter
	*EntityStates.Toolbot.ToolbotDualWield.OnEnter
	*EntityStates.Toolbot.ToolbotDualWieldBase.OnEnter
	*EntityStates.Toolbot.ToolbotDualWieldStart.OnEnter

	EntityStates.Treebot.Weapon.AimMortar.OnEnter
	EntityStates.Treebot.Weapon.AimMortar2.OnEnter

	EntityStates.Captain.Weapon.SetupAirstrike.OnEnter
	EntityStates.Captain.Weapon.SetupSupplyDrop.OnEnter

Skills that need animation delay:
	EntityStates.Mage.Weapon.FireFireBolt.OnEnter
	EntityStates.Mage.Weapon.FireLaserbolt.OnEnter

	EntityStates.Bandit2.Weapon.Bandit2FirePrimaryBase.OnEnter

	EntityStates.Engi.EngiWeapon.FireMines.OnEnter
	EntityStates.Engi.EngiWeapon.FireSeekerGrenades.OnEnter

	EntityStates.Toolbot.FireGrenadeLauncher.PlayAnimation
	EntityStates.Treebot.Weapon.FireSyringe.OnEnter

	EntityStates.Croco.Slash.PlayAnimation

	EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter

	EntityStates.Loader.SwingComboFist.PlayAnimation

On.RoR2.PlayerCharacterMasterController.FixedUpdate -> self.GetInstanceField<RoR2.CharacterBody>("body").GetComponent<EntityStateMachine>().state.ToString()
	// EntityStates.Toolbot.ToolbotDualWield, EntityStates.Toolbot.ToolbotDualStart


	// EntityStates.Toolbot.ToolbotDash
	// EntityStates.Toolbot.ToolbotDashImpact
	// EntityStates.SpawnTeleporterState
	// EntityStates.GenericCharacterPod
	// EntityStates.GenericCharacterVehicleSeated
	// EntityStates.GenericCharacterMain
	// EntityStates.Mage.MageCharacterMain


EntityState chains:

	ToolbotDualWield		: ToolbotDualWieldBase : GenericCharacterMain : BaseCharacterMain : BaseState : EntityState
	ToolbotDualWieldStart	: ToolbotDualWieldBase

	ToolbotDash : 															BaseCharacterMain
	ToolbotDashImpact : 																		BaseState

	FireNailgun : BaseNailgunState : BaseToolbotPrimarySkillState : BaseSkillState : 			BaseState
	AimGrenade : AimThrowableBase : 								BaseSkillState

*/

#define DEBUG

using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Rewired;
using UnityEngine;
using R2API.Utils;
using RoR2;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates;

namespace RTAutoSprintEx {
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
[R2APISubmoduleDependency(nameof(CommandHelper))]
[BepInDependency(R2API.R2API.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(GUID, NAME, VERSION)]

public class RTAutoSprintEx : BaseUnityPlugin {
	public const string
		NAME = "RTAutoSprintEx",
		GUID = "com.johnedwa." + NAME,
		VERSION = "2.0.0";

	private static bool RT_enabled;

	internal HashSet<string> knownEntityStates = new HashSet<string>();
	internal HashSet<Type> statesWhichDisableSprint = new HashSet<Type>();
	internal HashSet<Type> statesWhichDelaySprint = new HashSet<Type>();

	public static ConfigEntry<bool> HoldSprintToWalk { get; set; }
	public static ConfigEntry<bool> SprintInAnyDirection { get; set; }
	public static ConfigEntry<bool> ToggleAutoSprint { get; set; }
	public static ConfigEntry<bool> ArtificerFlamethrowerToggle { get; set; }	
	public static ConfigEntry<bool> DisableSprintingCrosshair { get; set; }
	public static ConfigEntry<double> AnimationCancelDelay { get; set; }
	public static ConfigEntry<bool> DisableFOVChange { get; set; }
	public static ConfigEntry<bool> DisableSpeedlines { get; set; }
	public static ConfigEntry<int> CustomFOV { get; set; }
	public static ConfigEntry<double> SprintFOVMultiplier { get; set; }


	public void Awake() {
		
		RTAutoSprintEx.RT_enabled = true;
		double RT_num = 0.0;
		bool RT_isSprinting = false;
		bool RT_animationCancel = false;

		R2API.Utils.CommandHelper.AddToConsoleWhenReady();
		//CustomSurvivors = Config.Bind<string>("", "CustomSurvivorDisable", "", new ConfigDescription("List of custom survivors names that are disabled. The name is printed to the chat and log at spawn. Example: 'CustomSurvivorDisable: = SNIPER_NAME AKALI'"));
		//ArtificerFlamethrowerToggle = Config.Bind<bool>("", "ArtificerFlamethrowerToggle", true, new ConfigDescription("Artificer: Sprinting cancels the flamethrower, therefore it either has to disable AutoSprint for a moment, or you need to keep the button held down\ntrue: Flamethrower is a toggle, cancellable by hitting Sprint or casting M2\nfalse: Flamethrower is cast when the button is held down (binding to side mouse button recommended).", new AcceptableValueList<bool>(true, false)));
		HoldSprintToWalk = Config.Bind<bool>("", "HoldSprintToWalk", true, new ConfigDescription("Holding the Sprint key temporarily disables auto-sprinting, making you walk. Overrided by ToggleAutoSprint.", new AcceptableValueList<bool>(true, false)));
		ToggleAutoSprint = Config.Bind<bool>("", "ToggleAutoSprint", false, new ConfigDescription("Pressing the Sprint key toggles between walking and auto-sprinting. Overrides HoldSprintToWalk", new AcceptableValueList<bool>(true, false)));
		DisableSprintingCrosshair = Config.Bind<bool>("", "DisableSprintingCrosshair", true, new ConfigDescription("Disables the (useless) sprinting crosshair.", new AcceptableValueList<bool>(true, false)));
		CustomFOV = Config.Bind<int>("", "FOVValue", 70, new ConfigDescription("Change FOV. Game default is 60, set to -1 to disable change.", new AcceptableValueRange<int>(-1, 359)));
        DisableFOVChange = Config.Bind<bool>("", "DisableFOVChange", false, new ConfigDescription("Disables FOV change when sprinting", new AcceptableValueList<bool>(true, false)));
		SprintFOVMultiplier = Config.Bind<double>("", "SprintFOVMultiplier", 1.1, new ConfigDescription("Sets a custom sprinting FOV multiplier. Game default is 1.3, set to -1 to disable change.", new AcceptableValueRange<double>(-1, 3)));
		DisableSpeedlines = Config.Bind<bool>("", "DisableSpeedlines", false, new ConfigDescription("Disables speedlines while sprinting", new AcceptableValueList<bool>(true, false)));
		SprintInAnyDirection = Config.Bind<bool>("", "SprintInAnyDirection", false, new ConfigDescription("Cheat, Allows you to sprint in any direction.", new AcceptableValueList<bool>(true, false)));
		AnimationCancelDelay = Config.Bind<double>("", "AnimationCancelDelay", 0.2, new ConfigDescription("Some skills can be animation cancelled by starting to sprint. This value sets how long to wait.", new AcceptableValueRange<double>(0.0, 1.0)));


	// MUL-T
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWield>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWieldBase>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWieldStart>();
		RegisterSprintDisabler<EntityStates.Toolbot.FireNailgun>();
		RegisterSprintDisabler<EntityStates.Toolbot.AimStunDrone>();
		RegisterDelayer<EntityStates.Toolbot.FireGrenadeLauncher>();

	// Artificer
		RegisterSprintDisabler<EntityStates.Mage.Weapon.Flamethrower>();
		RegisterSprintDisabler<EntityStates.Mage.Weapon.PrepWall>();
		RegisterDelayer<EntityStates.Mage.Weapon.FireFireBolt>();
		RegisterDelayer<EntityStates.Mage.Weapon.FireLaserbolt>();

	// Bandit
		//RegisterSprintDisabler<EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState>();
		RegisterSprintDisabler<EntityStates.Bandit2.Weapon.PrepSidearmResetRevolver>();
		RegisterSprintDisabler<EntityStates.Bandit2.Weapon.PrepSidearmSkullRevolver>();
		//RegisterDelayer<EntityStates.Bandit2.Weapon.Bandit2FirePrimaryBase>();
		RegisterDelayer<EntityStates.Bandit2.Weapon.FireShotgun2>();
		RegisterDelayer<EntityStates.Bandit2.Weapon.Bandit2FireRifle>();

	// Engineer
		RegisterSprintDisabler<EntityStates.Engi.EngiMissilePainter.Paint>();
		RegisterDelayer<EntityStates.Engi.EngiWeapon.FireMines>();
		RegisterDelayer<EntityStates.Engi.EngiWeapon.FireSeekerGrenades>();

	// Rex
		RegisterSprintDisabler<EntityStates.Treebot.Weapon.AimMortar>();
		RegisterSprintDisabler<EntityStates.Treebot.Weapon.AimMortar2>();
		RegisterSprintDisabler<EntityStates.Treebot.Weapon.AimMortarRain>();
		RegisterDelayer<EntityStates.Treebot.Weapon.FireSyringe>();

	// Captain
		RegisterSprintDisabler<EntityStates.Captain.Weapon.SetupAirstrike>();
		RegisterSprintDisabler<EntityStates.Captain.Weapon.SetupSupplyDrop>();

	// Acrid
		RegisterDelayer<EntityStates.Croco.Slash>();

	// Commando
		RegisterDelayer<EntityStates.Commando.CommandoWeapon.FirePistol2>();
	
	// Loader
		RegisterDelayer<EntityStates.Loader.SwingComboFist>();

/*
		On.RoR2.CharacterBody.OnSkillActivated += (orig, self, GenericSkill) => { 
			orig(self, GenericSkill); 
			ShouldSprintBeDisabledOnThisBody(GenericSkill.characterBody);
			Debug.Log(
				GenericSkill.skillDef.skillName  + " | " + 			// FireFirebolt
				GenericSkill + " | " + 								// MageBody(Clone) (RoR2.GenericSkill)
				GenericSkill.characterBody + " | " + 				// MageBody(Clone) (RoR2.CharacterBody)
				GenericSkill.characterBody.name + " | " + 			// MageBody(Clone)
				GenericSkill.characterBody.master + " | " + 		// CommandoMaster(Clone) (RoR2.CharacterMaster)
				GenericSkill.characterBody.masterObject + " | " 	// CommandoMaster(Clone) (UnityEngine.GameObject)			
				);
		};
*/

		On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
			orig.Invoke(self);
			if (RT_enabled) {
				RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
				if (instanceFieldBodyInputs) {
					if (self.networkUser && self.networkUser.localUser != null && !self.networkUser.localUser.isUIFocused) {
						RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
						if (instanceFieldBody) {
							Player inputPlayer = self.networkUser.localUser.inputPlayer;
							RT_isSprinting = instanceFieldBody.isSprinting;
							
						// Limit the rate, fixes MUL-T power mode reset. 
							RT_num += (double)Time.deltaTime;		
							if (RT_num >= 0.15 ) {
								if (!RT_isSprinting) {
									if (!RT_animationCancel) { RT_num = 0 - SprintDelayTime(instanceFieldBody); }
									if (RT_num >= 0) {
										RT_isSprinting = !ShouldSprintBeDisabledOnThisBody(instanceFieldBody);
										RT_num = 0;
										RT_animationCancel = false;
									}
								}
								
							// Walking logic
								if (inputPlayer.GetButton("Sprint")) {
									RT_num = 0;
									if (RT_isSprinting && HoldSprintToWalk.Value && !ToggleAutoSprint.Value) RT_isSprinting = false;
									if (!RT_isSprinting && ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) RT_isSprinting = true;
								}
							}

						// Animation cancelling logic.
							if (!RT_animationCancel && RT_num < -(AnimationCancelDelay.Value)
								&& !inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill") 
								&& !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill")) {
									RT_num = -(AnimationCancelDelay.Value);
									RT_animationCancel = true;
							}

						// Angle check disables sprinting if the movement angle is too large
							if (RT_isSprinting && !SprintInAnyDirection.Value) {
								Vector3 aimDirection = instanceFieldBodyInputs.aimDirection;
								aimDirection.y = 0f;
								aimDirection.Normalize();
								Vector3 moveVector = instanceFieldBodyInputs.moveVector;
								moveVector.y = 0f;
								moveVector.Normalize();					
								if ((Vector3.Dot(aimDirection, moveVector) <= self.GetFieldValue<float>("sprintMinAimMoveDot"))) {
									RT_isSprinting = false;
								}
							}

							instanceFieldBodyInputs.sprint.PushState(RT_isSprinting);
						} // End of if (instanceFieldBody)
					} // End of if (networkUser) check
				} // End of if (instanceFieldBodyInputs)
			} // End of "RT_enabled"
		}; // End of FixedUpdate


	// Custom FOV
        On.RoR2.CameraRigController.Update += (orig, self) => {
            orig(self);
			if (CustomFOV.Value > 0 && CustomFOV.Value != self.baseFov && CustomFOV.Value < 360) self.baseFov = CustomFOV.Value;
        };

	// Sprinting Crosshair
		IL.RoR2.UI.CrosshairManager.UpdateCrosshair += (il) => {
			ILCursor c = new ILCursor(il);
			if (DisableSprintingCrosshair.Value) {
				Debug.Log("RtAutoSprintEx: Disabling sprinting crosshair:");
				try {
					c.Index = 0;
					c.GotoNext ( 
						MoveType.After, 
						x => x.MatchCallvirt<CharacterBody>("get_isSprinting")
					);
					c.Emit(OpCodes.Ldc_I4, 0);
					c.Emit(OpCodes.And);
				} catch (Exception ex) { Debug.LogError(ex); }
			}
		};		

	//Sprinting FOV change
		IL.RoR2.CameraRigController.Update += (il) => {
            ILCursor c = new ILCursor(il);
            if (DisableFOVChange.Value) {
				Debug.Log("RtAutoSprintEx: Disabling Sprint FOV Change:");
				try {
					c.Index = 0;
					c.GotoNext(
						x => x.MatchLdloc(0),
						x => x.MatchLdarg(0),
						x => x.MatchLdfld <RoR2.CameraRigController>("targetBody"),
						x => x.MatchCallvirt<RoR2.CharacterBody>("get_isSprinting")
					);
					c.RemoveRange(10);
				} catch (Exception ex) { Debug.LogError(ex); }
            } else if (!DisableFOVChange.Value && (SprintFOVMultiplier.Value != -1)) {
				Debug.Log("RtAutoSprintEx: Modifying Sprint FOV Multiplier:");
				try {
					c.Index = 0;
					c.GotoNext(
						x => x.MatchLdloc(0),
						x => x.MatchLdarg(0),
						x => x.MatchLdfld <RoR2.CameraRigController>("targetBody"),
						x => x.MatchCallvirt<RoR2.CharacterBody>("get_isSprinting")
					);
					c.Index += 7;
					c.Next.Operand = (float)SprintFOVMultiplier.Value;
				} catch (Exception ex) { Debug.LogError(ex); }
        	}
		// Disable Speedlines
            if (DisableSpeedlines.Value) {
				Debug.Log("RtAutoSprintEx: Disabling Speedlines:");
				try {
					c.Index = 0;
					c.GotoNext(
						x => x.MatchLdarg(0),
						x => x.MatchLdfld<RoR2.CameraRigController>("sprintingParticleSystem"),
						x => x.MatchCallvirt<UnityEngine.ParticleSystem>("get_isPlaying") 
					);
					c.RemoveRange(3);
					c.Emit(OpCodes.Ldc_I4, 1);
				} catch (Exception ex) { Debug.LogError(ex); }
            }
			Debug.Log("RtAutoSprintEx: CameraRigController.Update IL edits done.");
		};
	} // End of Awake

// Registers EntityStates as sprint disablers
	public void RegisterSprintDisabler<T>() where T : BaseState {
		Debug.LogWarning("Sprint disabled for : " + typeof(T).ToString());
		statesWhichDisableSprint.Add(typeof(T));
	}

// Registers EntityStates as sprint delayers	
	public void RegisterDelayer<T>() where T : BaseState {
		Debug.LogWarning("Sprint delay added for : " + typeof(T).ToString());
		statesWhichDelaySprint.Add(typeof(T));
	}

// Checks if an EntityState blocks sprinting
	public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
		EntityStateMachine[] stateMachines;
		stateMachines = targetBody.GetComponents<EntityStateMachine>();
		bool isSprintBlocked = false;
		foreach (EntityStateMachine machine in stateMachines) {
			var currentState = machine.state;	
			if(currentState == null) { return false; }
#if DEBUG
			if(!knownEntityStates.Contains(currentState.ToString())) {
				knownEntityStates.Add(currentState.ToString());
				Debug.LogError("List of Known EntityStates;");
				foreach (var item in knownEntityStates) {
					Debug.Log(item);
				}
			}
#endif
			if (statesWhichDisableSprint.Contains(currentState.GetType())) { isSprintBlocked = true; }
		}
		return isSprintBlocked;
	}

// Calculates the animation delay
	public double SprintDelayTime(CharacterBody targetBody) {
		float duration = 0.0f;
		EntityStateMachine[] stateMachines;
		stateMachines = targetBody.GetComponents<EntityStateMachine>();
		foreach (EntityStateMachine machine in stateMachines) {
			var currentState = machine.state;	
			if(currentState == null) { return duration; }
			if (statesWhichDelaySprint.Contains(currentState.GetType())) { 
				//try { duration = (double) currentState.GetFieldValue<float>("duration");} catch {}
				var stateField = currentState.GetType().GetFieldCached("duration");
				if (stateField != null) duration = (float)stateField.GetValue(currentState);
				return duration;
			}
		}
		return duration;
	}


// Console Commands
	[RoR2.ConCommand(commandName = "rt_help", flags = ConVarFlags.ExecuteOnServer, helpText = "List all RTAutoSprintEx console commands.")]
	private static void cc_rt_help(ConCommandArgs args) {
		Debug.Log("'rt_enabled <bool>'. Default: true. Enables/Disables the sprinting part of the mod.");
		//Debug.Log("'rt_sprintcheat <bool>'. Default: false. Allows you to sprint in any direction.");
		Debug.Log("'rt_fov <int>'. Default: 60. Valid Range: 1-359. Sets the base FOV");
		//Debug.Log("'rt_disable_fov_change <bool>'\t Default false.");
		//Debug.Log("'rt_fov_multiplier <float>'\t Default: 1,3. Valid Range: 0.5-2.0. How much the camera FOV changes when sprinting.");
		//Debug.Log("'rt_disable_speedlines <bool>'\t Default: false.");
		//Debug.Log("'rt_disable_sprinting_crosshair <bool>'\t Default: true.");
		//Debug.Log("'rt_artificer_flamethrower_toggle <bool>'.  Default: true.");
		//Debug.Log("Rest of the options aren't currently run-time editable, you have to change them in the config:");
		//Debug.Log("AnimationCancelDelay, HoldSprintToWalk, DisableSprintingCrosshair, DisableSpeedlines, DisableFOVChange, SprintFOVMultiplier");
	}

	[RoR2.ConCommand(commandName = "rt_enabled", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(bool)enabled")]
	private static void cc_rt_enabled(ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			if (Utils.TryParseBool(args[0], out bool result)) {
				RTAutoSprintEx.RT_enabled = (bool)result;
				Debug.Log($"{nameof(RTAutoSprintEx.RT_enabled)}={RTAutoSprintEx.RT_enabled}");
			}	
		} catch (Exception ex) { Debug.LogError(ex); }
	}

	[RoR2.ConCommand(commandName = "rt_fov", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(int)fov")]
	private static void cc_rt_fov (ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			int? value = args.TryGetArgInt(0);
			if (value.HasValue && value >= 1 && value <= 359) {		
				CustomFOV.Value = (int)value;
				Debug.Log($"{nameof(CustomFOV)}={value}");	
			}
		} catch (Exception ex) { Debug.LogError(ex); }
	}

} // End of class RTAutoSprintEx
} // End of Namespace