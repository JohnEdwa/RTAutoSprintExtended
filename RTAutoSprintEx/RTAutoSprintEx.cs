
using System;
using System.Reflection;
//using System.Collections;
//using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Rewired;
using UnityEngine;
using R2API.Utils;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RT_AutoSprint_Ex {
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
[R2APISubmoduleDependency(nameof(CommandHelper))]
[BepInDependency(R2API.R2API.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(GUID, NAME, VERSION)]


public class RTAutoSprintEx : BaseUnityPlugin {
	public const string
		NAME = "RTAutoSprintEx",
		GUID = "com.johnedwa." + NAME,
		VERSION = "1.0.8";

	private static ConfigEntry<bool> HoldSprintToWalk;
	private static ConfigEntry<bool> SprintInAnyDirection;
	private static ConfigEntry<bool> ArtificerFlamethrowerToggle;	
	private static ConfigEntry<bool> DisableSprintingCrosshair;
	private static ConfigEntry<double> AnimationCancelDelay;
	private static ConfigEntry<bool> DisableFOVChange;
	private static ConfigEntry<bool> DisableSpeedlines;
	private static ConfigEntry<int> CustomFOV;
	private static ConfigEntry<double> SprintFOVMultiplier;

	private static double RT_num;
	private static bool RT_enabled;
	private static bool RT_isSprinting;
	private static bool RT_cancelWithSprint;
	private static bool RT_tempDisable;

	public void Awake() {

		RTAutoSprintEx.RT_num = 0.0;
		RTAutoSprintEx.RT_enabled = true;
		RTAutoSprintEx.RT_isSprinting = false;
		RTAutoSprintEx.RT_cancelWithSprint = false;
		RTAutoSprintEx.RT_tempDisable = false;

	// Configuration
		R2API.Utils.CommandHelper.AddToConsoleWhenReady();
		//On.RoR2.Console.Awake += (orig, self) => { CommandHelper.RegisterCommands(self); orig(self); };
		
		ArtificerFlamethrowerToggle = Config.Bind("", "ArtificerFlamethrowerToggle", true, new ConfigDescription("Artificer: Sprinting cancels the flamethrower, therefore it either has to disable AutoSprint for a moment, or you need to keep the button held down\ntrue: Flamethrower is a toggle, cancellable by hitting Sprint or casting M2\nfalse: Flamethrower is cast when the button is held down (binding to side mouse button recommended).", new AcceptableValueList<bool>(true, false)));
		HoldSprintToWalk = Config.Bind("", "HoldSprintToWalk", true, new ConfigDescription("General: Holding Sprint key temporarily disables auto-sprinting, making you walk.", new AcceptableValueList<bool>(true, false)));
		SprintInAnyDirection = Config.Bind("", "SprintInAnyDirection", false, new ConfigDescription("Cheat: Allows you to sprint in any direction.", new AcceptableValueList<bool>(true, false)));
		AnimationCancelDelay = Config.Bind("", "AnimationCancelDelay", 0.2, new ConfigDescription("General: Some skills can be animation cancelled by starting to sprint. This value sets how long to wait.", new AcceptableValueRange<double>(0.0, 1.0)));
		DisableSprintingCrosshair = Config.Bind("", "DisableSprintingCrosshair", true, new ConfigDescription("General: Disables the (useless) sprinting crosshair. The most probable thing to break on game update.", new AcceptableValueList<bool>(true, false)));
		DisableSpeedlines = Config.Bind("", "DisableSpeedlines", false, new ConfigDescription("General: Disables speedlines while sprinting", new AcceptableValueList<bool>(true, false)));
        CustomFOV = Config.Bind("", "FOVValue", 60, new ConfigDescription("FOV : Change FOV. Set to -1 to disable and use default.", new AcceptableValueRange<int>(1, 359)));
        DisableFOVChange = Config.Bind("", "DisableFOVChange", false, new ConfigDescription("FOV: Disables FOV change when sprinting", new AcceptableValueList<bool>(true, false)));
		SprintFOVMultiplier = Config.Bind("", "SprintFOVMultiplier", 1.3, new ConfigDescription("FOV: Sets a custom sprinting FOV multiplier.", new AcceptableValueRange<double>(0.1, 3)));

	// Artificer
		//Flamethrower
		On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Mage.Weapon.Flamethrower.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };
		// Ice Wall
		On.EntityStates.Mage.Weapon.PrepWall.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Mage.Weapon.PrepWall.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };
		// Bolt logic
		On.EntityStates.Mage.Weapon.FireFireBolt.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };
		On.EntityStates.Mage.Weapon.FireLaserbolt.OnEnter += (orig, self) => {  orig(self);  RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };

	// Engineer
		// Mines
		On.EntityStates.Engi.EngiWeapon.FireMines.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };
		On.EntityStates.Engi.EngiWeapon.FireSeekerGrenades.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };
		// Harpoon
		On.EntityStates.Engi.EngiMissilePainter.Paint.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true;};
		On.EntityStates.Engi.EngiMissilePainter.Paint.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };

	// MUL-T
		//Nailgun
		On.EntityStates.FireNailgun.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.NailgunFinalBurst.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_tempDisable = false; };
		// Scrap Launcher
		On.EntityStates.Toolbot.FireGrenadeLauncher.PlayAnimation += (orig, self, duration) => { orig(self, duration); RTAutoSprintEx.RT_num = -duration; };
		// Stun Grenade (M2)
		On.EntityStates.Toolbot.AimGrenade.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };

	// REX workaround logic
		On.EntityStates.Treebot.Weapon.FireSyringe.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };

		On.EntityStates.Treebot.Weapon.AimMortar.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Treebot.Weapon.AimMortar.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false;};
		On.EntityStates.Treebot.Weapon.AimMortar2.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true;};
		On.EntityStates.Treebot.Weapon.AimMortar2.OnProjectileFiredLocal += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false;};

	// Acrid M1 delay to help with the animation cancelling issue
		//On.EntityStates.Croco.Slash.OnEnter += (orig, self) => { orig(self);  RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("durationBeforeInterruptable"); };
		On.EntityStates.Croco.Slash.PlayAnimation += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };

	// Commando M1 delay
		On.EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter += (orig, self) => {  orig(self);  RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };

	// Loader M1 Delay
		On.EntityStates.Loader.SwingComboFist.PlayAnimation += (orig, self) => { orig(self); RTAutoSprintEx.RT_num = -self.GetFieldValue<float>("duration"); };


	// Captain
		On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };
		On.EntityStates.Captain.Weapon.SetupSupplyDrop.OnEnter += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = true; RTAutoSprintEx.RT_tempDisable = true; };
		On.EntityStates.Captain.Weapon.SetupSupplyDrop.OnExit += (orig, self) => { orig(self); RTAutoSprintEx.RT_cancelWithSprint = false; RTAutoSprintEx.RT_tempDisable = false; };


	// This could be eventually used to do all the disabling stuff without touching the skills themselves, I think.
	/*
 		On.RoR2.CharacterBody.OnSkillActivated += (orig, self, GenericSkill) => { 
			orig(self, GenericSkill); 
				RoR2.Chat.AddMessage(
					GenericSkill.skillDef.skillName + " | Index: "
				);
		};
	*/


	// Sprinting logic
		On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
			RTAutoSprintEx.RT_isSprinting = false;
			bool skillsAllowAutoSprint = false;
			bool knownSurvivor = true;
			RoR2.NetworkUser networkUser = self.networkUser;
			RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
			if (instanceFieldBodyInputs) {
				if (networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused) {
					Player inputPlayer = networkUser.localUser.inputPlayer;
					RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
					if (instanceFieldBody && RTAutoSprintEx.RT_enabled) {
						RTAutoSprintEx.RT_isSprinting = instanceFieldBody.isSprinting;
						knownSurvivor = true;
						switch(instanceFieldBody.baseNameToken){
								case "COMMANDO_BODY_NAME":
								case "HUNTRESS_BODY_NAME":
								case "MERC_BODY_NAME":
								case "LOADER_BODY_NAME":
								case "CROCO_BODY_NAME":
									skillsAllowAutoSprint = true;
									break;
								case "ENGI_BODY_NAME":	
								case "TREEBOT_BODY_NAME":
								case "TOOLBOT_BODY_NAME":
								case "CAPTAIN_BODY_NAME":
									skillsAllowAutoSprint = (!RTAutoSprintEx.RT_tempDisable);
									break;								
								case "MAGE_BODY_NAME":
									// If TOGGLE, just follow tempDisable, if HOLD disable when button released
									if (RT_cancelWithSprint && !ArtificerFlamethrowerToggle.Value && !inputPlayer.GetButton("SpecialSkill")) RTAutoSprintEx.RT_tempDisable = false;
									skillsAllowAutoSprint = (!RTAutoSprintEx.RT_tempDisable);
									break;
								default:
									knownSurvivor = false;
									//skillsAllowAutoSprint = false;
									//skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
									break;
							}
						if (knownSurvivor) {
							if (!RTAutoSprintEx.RT_isSprinting && RTAutoSprintEx.RT_num >= 0.1) {
								RTAutoSprintEx.RT_num = 0.0;
								RTAutoSprintEx.RT_isSprinting = skillsAllowAutoSprint;
							}
							// Animation cancelling after stopping attack
							if (RTAutoSprintEx.RT_num < -(AnimationCancelDelay.Value)
								&& !inputPlayer.GetButton("PrimarySkill") 
								&& !inputPlayer.GetButton("SecondarySkill") 
								&& !inputPlayer.GetButton("SpecialSkill")
								&& !inputPlayer.GetButton("UtilitySkill")) 
								{ RTAutoSprintEx.RT_num = -(AnimationCancelDelay.Value); }
							if (!RTAutoSprintEx.RT_isSprinting) RTAutoSprintEx.RT_num += (double)Time.deltaTime;

						// Disable sprinting if the movement angle is too large
							if (RTAutoSprintEx.RT_isSprinting) {
								Vector3 aimDirection = instanceFieldBodyInputs.aimDirection;
								aimDirection.y = 0f;
								aimDirection.Normalize();
								Vector3 moveVector = instanceFieldBodyInputs.moveVector;
								moveVector.y = 0f;
								moveVector.Normalize();
								if ((Vector3.Dot(aimDirection, moveVector) < self.GetFieldValue<float>("sprintMinAimMoveDot")) && !RTAutoSprintEx.SprintInAnyDirection.Value) {
									RTAutoSprintEx.RT_isSprinting = false;
								}
							} 
							// Walking logic.
							if (inputPlayer.GetButton("Sprint")) {
								RTAutoSprintEx.RT_num = 1.0; 
								if (HoldSprintToWalk.Value) RTAutoSprintEx.RT_isSprinting = false;
								if (RT_cancelWithSprint) RTAutoSprintEx.RT_isSprinting = true;
							}
						}
					}
				}

				orig.Invoke(self);

				if (instanceFieldBodyInputs && RTAutoSprintEx.RT_enabled && knownSurvivor) {
					instanceFieldBodyInputs.sprint.PushState(RTAutoSprintEx.RT_isSprinting);
				}
			}
		}; // End of FixedUpdate

	// Custom FOV
        On.RoR2.CameraRigController.Update += (orig, self) => {
			if (CustomFOV.Value != self.baseFov) self.baseFov = CustomFOV.Value;
            orig(self);
        };

	// Sprinting Crosshair
		IL.RoR2.UI.CrosshairManager.UpdateCrosshair += (il) => {
			ILCursor c = new ILCursor(il);
			if (DisableSprintingCrosshair.Value) {
				Debug.Log("AutoSprint: Disabling sprinting crosshair:");
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

		//Sprinting FOV change and Speedlines.
		IL.RoR2.CameraRigController.Update += (il) => {
            ILCursor c = new ILCursor(il);
            if (DisableFOVChange.Value) {
				Debug.Log("AutoSprint: Disabling Sprint FOV Change:");
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
            } else if (!DisableFOVChange.Value && SprintFOVMultiplier.Value != 1.3) {
				Debug.Log("AutoSprint: Modifying Sprint FOV Multiplier:");
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
            if (DisableSpeedlines.Value) {
				Debug.Log("AutoSprint: Disabling Speedlines:");
				try {
					c.Index = 0;
					c.GotoNext(
						x => x.MatchLdarg(0), // 748	08E9	ldarg.0
						x => x.MatchLdfld<RoR2.CameraRigController>("sprintingParticleSystem"), // 749	08EA	ldfld	class [UnityEngine.ParticleSystemModule]UnityEngine.ParticleSystem RoR2.CameraRigController::sprintingParticleSystem
						x => x.MatchCallvirt<UnityEngine.ParticleSystem>("get_isPlaying") // 750	08EF	callvirt	instance bool [UnityEngine.ParticleSystemModule]UnityEngine.ParticleSystem::get_isPlaying()
					);
					c.RemoveRange(3);
					c.Emit(OpCodes.Ldc_I4, 1);
				} catch (Exception ex) { Debug.LogError(ex); }
            }
			Debug.Log("AutoSprint: CameraRigController.Update IL edits done.");
        };
		//Debug.Log("Loaded RT AutoSprint Extended: Artificer flamethrower mode is" + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
	} // End of Awake


	// Console Commands

	[RoR2.ConCommand(commandName = "rt_help", flags = ConVarFlags.ExecuteOnServer, helpText = "")]
	private static void cc_rt_help(ConCommandArgs args) {
		Debug.Log("'rt_enabled <bool>'. Default: true. Enables/Disables the sprinting part of the mod.");
		Debug.Log("'rt_sprintcheat <bool>'. Default: false. Allows you to sprint in any direction.");
		Debug.Log("'rt_fov <int>'. Default: 60. Valid Range: 1-359. Sets the base FOV");
		//Debug.Log("'rt_disable_fov_change <bool>'\t Default false.");
		//Debug.Log("'rt_fov_multiplier <float>'\t Default: 1,3. Valid Range: 0.5-2.0. How much the camera FOV changes when sprinting.");
		//Debug.Log("'rt_disable_speedlines <bool>'\t Default: false.");
		//Debug.Log("'rt_disable_sprinting_crosshair <bool>'\t Default: true.");
		Debug.Log("'rt_artificer_flamethrower_toggle <bool>'.  Default: true.");
		Debug.Log("Rest of the options aren't currently run-time editable, you have to change them in the config:");
		Debug.Log("AnimationCancelDelay, HoldSprintToWalk, DisableSprintingCrosshair, DisableSpeedlines, DisableFOVChange, SprintFOVMultiplier");
	}

	[RoR2.ConCommand(commandName = "rt_enabled", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(bool)enabled")]
	private static void cc_rt_enabled(ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			if (TryParseBool(args[0], out bool result)) {
				RTAutoSprintEx.RT_enabled = (bool)result;
				Debug.Log($"{nameof(RTAutoSprintEx.RT_enabled)}={RTAutoSprintEx.RT_enabled}");
			}	
		} catch (Exception ex) { Debug.LogError(ex); }
	}

	[RoR2.ConCommand(commandName = "rt_sprintcheat", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(bool)enabled")]
	private static void cc_rt_sprintcheat(ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			if (TryParseBool(args[0], out bool result)) {
				RTAutoSprintEx.SprintInAnyDirection.Value = (bool)result;
				Debug.Log($"{nameof(RTAutoSprintEx.SprintInAnyDirection.Value)}={RTAutoSprintEx.SprintInAnyDirection.Value}");
			}	
		} catch (Exception ex) { Debug.LogError(ex); }
	}

	[RoR2.ConCommand(commandName = "rt_fov", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(int)fov")]
	private static void cc_rt_fov (ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			int? value = args.TryGetArgInt(0);
			if (value.HasValue) {		
				CustomFOV.Value = (int)value;
				Debug.Log($"{nameof(CustomFOV)}={value}");	
			}
		} catch (Exception ex) { Debug.LogError(ex); }
	}

	[RoR2.ConCommand(commandName = "rt_artificer_flamethrower_toggle", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0]=(bool)enabled")]
	private static void cc_rt_artificer_flamethrower_toggle(ConCommandArgs args) {
		try {
			args.CheckArgumentCount(1);
			if (TryParseBool(args[0], out bool result)) {	
				ArtificerFlamethrowerToggle.Value = result;
				Debug.Log($"{nameof(ArtificerFlamethrowerToggle)}={result}");
			}	
		} catch (Exception ex) { Debug.LogError(ex); }
	}	


internal static bool TryParseBool(string input, out bool result){
    if(bool.TryParse(input,out result)){return true;}
	if(int.TryParse(input,out int val)) { result = val > 0 ? true : false; return true; } return false;}

} // End of class RTAutoSprintEx

// Utilities
	public static class Utils
	{
		public static T GetInstanceField<T>(this object instance, string fieldName)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
			return (T)((object)field.GetValue(instance));
		}

		public static void SetInstanceField<T>(this object instance, string fieldName, T value)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
			field.SetValue(instance, value);
		}
	}

} // End of Namespace