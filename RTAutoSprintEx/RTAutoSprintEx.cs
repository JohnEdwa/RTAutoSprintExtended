using System;
using System.Reflection;
using System.Collections;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Rewired;
using UnityEngine;
using R2API.Utils;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;


namespace RT_AutoSprint_Ex {
//[BepInDependency("com.bepis.r2api")]
[BepInDependency(R2API.R2API.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(GUID, NAME, VERSION)]

public class RTAutoSprintEx : BaseUnityPlugin {
	public const string
		NAME = "RTAutoSprintEx",
		GUID = "com.johnedwa." + NAME,
		VERSION = "4478858.1.1";

	private static ConfigWrapper<bool> ArtificerFlamethrowerToggle;
	private static ConfigWrapper<bool> HoldSprintToWalk;
	private static ConfigWrapper<bool> DisableSprintingCrosshair;

	private static double RT_num;
	public static bool RT_enabled;
	public static bool RT_isSprinting;
	public static bool RT_artiFlaming;
	public static bool RT_tempDisable;

	public void Awake() {

		RTAutoSprintEx.RT_num = 0.0;
		RTAutoSprintEx.RT_enabled = true;
		RTAutoSprintEx.RT_isSprinting = false;
		RTAutoSprintEx.RT_artiFlaming = false;
		RTAutoSprintEx.RT_tempDisable = false;

	// Configuration
		On.RoR2.Console.Awake += (orig, self) => {
			CommandHelper.RegisterCommands(self);
			orig(self);
		};

		HoldSprintToWalk = Config.Wrap<bool>(
			"Config", "HoldSprintToWalk",
			"General: Holding Sprint key temporarily disables auto-sprinting, making you to walk.",
			true
		);

		DisableSprintingCrosshair = Config.Wrap<bool>(
			"Config", "DisableSprintingCrosshair",
			"General: Disables the (useless) sprinting crosshair. The most probable thing to break on game update.",
			true
		);

		ArtificerFlamethrowerToggle = Config.Wrap<bool>(
			"Config", "ArtificerFlamethrowerToggle",
			"Artificer: Sprinting cancels the flamethrower, therefore it either has to disable AutoSprint for a moment, or you need to keep the button held down\ntrue: Flamethrower is a toggle, cancellable by hitting Sprint or casting M2\nfalse: Flamethrower is cast when the button is held down (binding to side mouse button recommended).",
			true
		);

	// Artificer Flamethrower workaround logic
		On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) => {
			RTAutoSprintEx.RT_artiFlaming = true;
			RTAutoSprintEx.RT_tempDisable = ArtificerFlamethrowerToggle.Value;
			orig(self);
		};
		On.EntityStates.Mage.Weapon.Flamethrower.OnExit += (orig, self) => {
			RTAutoSprintEx.RT_artiFlaming = false;
			RTAutoSprintEx.RT_tempDisable = false;
			orig(self);
		};
	// Artificer bolt logic
		On.EntityStates.Mage.Weapon.FireFireBolt.OnEnter += (orig, self) => { RTAutoSprintEx.RT_num = -0.2; orig(self); };
		On.EntityStates.Mage.Weapon.FireLaserbolt.OnEnter += (orig, self) => { RTAutoSprintEx.RT_num = -0.2; orig(self); };

	// Engineer mine workaround
		On.EntityStates.Engi.EngiWeapon.FireMines.OnEnter += (orig, self) => { RTAutoSprintEx.RT_num = -0.4; orig(self); };
		On.EntityStates.Engi.EngiWeapon.FireSeekerGrenades.OnEnter += (orig, self) => { RTAutoSprintEx.RT_num = -0.4; orig(self); };

	// MUL-T workaround logic, disable sprinting while using the Nailgun, Scrap Launcher, or Stun Grenade.
		//Nailgun
		On.EntityStates.FireNailgun.OnEnter += (orig, self) => { RTAutoSprintEx.RT_tempDisable = true; orig(self); };
		On.EntityStates.FireNailgun.FixedUpdate += (orig, self) => {
			orig(self);
			if (self.GetFieldValue<bool>("beginToCooldown")) {RTAutoSprintEx.RT_tempDisable = false;};
		};
		// Stun Grenade (M2)
		On.EntityStates.Toolbot.AimGrenade.OnEnter += (orig, self) => { RTAutoSprintEx.RT_tempDisable = true; orig(self); };
		On.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += (orig, self) => {RTAutoSprintEx.RT_tempDisable = false; orig(self); };

	// REX workaround logic
		On.EntityStates.Treebot.Weapon.AimMortar.OnEnter += (orig, self) => { RTAutoSprintEx.RT_tempDisable = true; orig(self); };
		On.EntityStates.Treebot.Weapon.AimMortar.OnExit += (orig, self) => { RTAutoSprintEx.RT_tempDisable = false; orig(self); };
		On.EntityStates.Treebot.Weapon.AimMortar2.OnEnter += (orig, self) => { RTAutoSprintEx.RT_tempDisable = true; orig(self); };
		On.EntityStates.Treebot.Weapon.AimMortar2.OnProjectileFiredLocal += (orig, self) => { RTAutoSprintEx.RT_tempDisable = false; orig(self); };

	// Acrid M1 delay to help with the animation cancelling issue
		//On.EntityStates.Croco.Slash.OnExit += (orig, self) => { RTAutoSprintEx.RT_num = -0.2; orig(self); };

	// Disable sprinting crosshair
		if (DisableSprintingCrosshair.Value) {
			IL.RoR2.UI.CrosshairManager.UpdateCrosshair += (il) => {
				ILCursor c = new ILCursor(il);
					c.GotoNext(
						x => x.MatchLdarg(1),
						x => x.MatchCallvirt<CharacterBody>("get_isSprinting")
						);
					c.Index += 0;
					c.RemoveRange(2);
					c.Emit(OpCodes.Ldc_I4, 0);
			};
		}


	// Sprinting logic
		On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
			/*
			if (Input.GetKeyDown(KeyCode.F2)) {
				RTAutoSprintEx.RT_enabled = !RTAutoSprintEx.RT_enabled;
				RoR2.Chat.AddMessage("RTAutoSprintEx " + ((RTAutoSprintEx.RT_enabled) ? " enabled." : " disabled."));
			}
			*/

			RTAutoSprintEx.RT_isSprinting = false;
			bool skillsAllowAutoSprint = false;
			RoR2.NetworkUser networkUser = self.networkUser;
			RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
			if (instanceFieldBodyInputs) {
				if (networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused) {
					Player inputPlayer = networkUser.localUser.inputPlayer;
					RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
					if (instanceFieldBody && RTAutoSprintEx.RT_enabled) {
						RTAutoSprintEx.RT_isSprinting = instanceFieldBody.isSprinting;
						if (!RTAutoSprintEx.RT_isSprinting) {
							if (RTAutoSprintEx.RT_num > 0.1) {
								RTAutoSprintEx.RT_num = 0.0;
								switch(instanceFieldBody.baseNameToken){
									case "COMMANDO_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;
									case "HUNTRESS_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("SpecialSkill"));
										break;
									case "MAGE_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill") && !RTAutoSprintEx.RT_tempDisable);
										break;
									case "ENGI_BODY_NAME":
										skillsAllowAutoSprint = (true);
										break;
									case "MERC_BODY_NAME":
										skillsAllowAutoSprint = (true);
										break;
									case "TREEBOT_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill") && !RTAutoSprintEx.RT_tempDisable);
										break;
									case "LOADER_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;
									case "CROCO_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;
									case "TOOLBOT_BODY_NAME":
										skillsAllowAutoSprint = (!RTAutoSprintEx.RT_tempDisable);
										break;
									default:
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
										break;
								}
								RTAutoSprintEx.RT_isSprinting = skillsAllowAutoSprint;
							}
						}
						if (!RTAutoSprintEx.RT_isSprinting) RTAutoSprintEx.RT_num += (double)Time.deltaTime;

					// Disable sprinting if we movement angle is too large
						if (RTAutoSprintEx.RT_isSprinting) {
							Vector3 aimDirection = instanceFieldBodyInputs.aimDirection;
							aimDirection.y = 0f;
							aimDirection.Normalize();
							Vector3 moveVector = instanceFieldBodyInputs.moveVector;
							moveVector.y = 0f;
							moveVector.Normalize();
							if (Vector3.Dot(aimDirection, moveVector) < self.GetInstanceField<float>("sprintMinAimMoveDot")) {
								RTAutoSprintEx.RT_isSprinting = false;
							}
						}
					}
				// Walking logic.
					if (inputPlayer.GetButton("Sprint")) {
						if (HoldSprintToWalk.Value) RTAutoSprintEx.RT_isSprinting = false;
						if (RT_artiFlaming) RTAutoSprintEx.RT_isSprinting = true;
					}
				}

				orig.Invoke(self);
				if (instanceFieldBodyInputs && RTAutoSprintEx.RT_enabled) {
					instanceFieldBodyInputs.sprint.PushState(RTAutoSprintEx.RT_isSprinting);
				}
			}
		}; // End of FixedUpdate
		Debug.Log("Loaded RT AutoSprint Extended\nArtificer flamethrower mode is " + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
	} // End of Awake

	[RoR2.ConCommand(commandName = "rt_artiflamemode", flags = ConVarFlags.None, helpText = "Artificer Flamethrower Mode: Toggle or Hold")]
	private static void RTArtiFlameMode(RoR2.ConCommandArgs args) {
		args.CheckArgumentCount(1);
		switch (args[0].ToLower()) {
			case "toggle":
			case "true":
			case "1":
				ArtificerFlamethrowerToggle.Value = true;
				break;
			case "hold":
			case "false":
			case "0":
				ArtificerFlamethrowerToggle.Value = false;
				break;
			default:
				Debug.Log("Invalid argument. Valid argument: true/false, toggle/hold, 1/0");
				break;
		}
		Debug.Log($"Artificer flamethrower mode is " + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
	}
} // End of class RTAutoSprintEx

// Helper classes
public class CommandHelper {
	public static void RegisterCommands(RoR2.Console self) {
		var types = typeof(CommandHelper).Assembly.GetTypes();
		var catalog = self.GetFieldValue<IDictionary>("concommandCatalog");
		foreach (var methodInfo in types.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))) {
			var customAttributes = methodInfo.GetCustomAttributes(false);
			foreach (var attribute in customAttributes.OfType<RoR2.ConCommandAttribute>()) {
				var conCommand = Reflection.GetNestedType<RoR2.Console>("ConCommand").Instantiate();
				conCommand.SetFieldValue("flags", attribute.flags);
				conCommand.SetFieldValue("helpText", attribute.helpText);
				conCommand.SetFieldValue("action", (RoR2.Console.ConCommandDelegate)Delegate.CreateDelegate(typeof(RoR2.Console.ConCommandDelegate), methodInfo));
				catalog[attribute.commandName.ToLower()] = conCommand;
			}
		}
	}
}
public static class Utils
{
	public static T GetInstanceField<T>(this object instance, string fieldName) {
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
		return (T)((object)field.GetValue(instance));
	}
	public static void SetInstanceField<T>(this object instance, string fieldName, T value) {
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
		field.SetValue(instance, value);
	}
}
} // End of Namespace