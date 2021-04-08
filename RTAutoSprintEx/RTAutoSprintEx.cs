
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
	private static bool RT_isSprinting;
	private static double RT_num;
	private List<string> RT_KnownEntities;

	public void Awake() {

		RT_enabled = true;
		RT_isSprinting = false;
		RT_num = 0.0;

#if DEBUG
		RT_KnownEntities = new List<string>();
#endif

		R2API.Utils.CommandHelper.AddToConsoleWhenReady();

		// Toolbot
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWield>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWieldBase>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWieldStart>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDash>();
		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDashImpact>();
			RegisterSprintDisabler<EntityStates.Toolbot.FireNailgun>();
			RegisterSprintDisabler<EntityStates.Toolbot.AimGrenade>();


		On.RoR2.CharacterBody.OnSkillActivated += (orig, self, GenericSkill) => { 
			orig(self, GenericSkill); 
			ShouldSprintBeDisabledOnThisBody(GenericSkill.characterBody);
			Debug.Log(
				GenericSkill.skillDef.skillName  + " | " +
				GenericSkill + " | " + 
				GenericSkill.characterBody + " | " + 
				GenericSkill.characterBody.name + " | " + 
				GenericSkill.characterBody.master + " | " + 
				GenericSkill.characterBody.masterObject + " | " 				
				);
		};


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
							if (!RT_isSprinting) {
								RT_num += (double)Time.deltaTime;
								if (RT_num >= 0.1) {
									RT_isSprinting = !ShouldSprintBeDisabledOnThisBody(instanceFieldBody);
									RT_num = 0.0;
								}
							}

						// Disable sprinting if movement angle is too large
							if (RT_isSprinting) {
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
	} // End of Awake

	internal HashSet<Type> statesWhichDisableSprint = new HashSet<Type>();
	public void RegisterSprintDisabler<T>() where T : BaseState {
		Debug.LogWarning("Sprint disabled for : " + typeof(T).ToString());
		statesWhichDisableSprint.Add(typeof(T));
	}

	public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
		var currentState = targetBody.GetComponent<EntityStateMachine>().state;

#if DEBUG
		if(!RT_KnownEntities.Contains(currentState.ToString())) {
			RT_KnownEntities.Add(currentState.ToString());
			Debug.LogError("List of Known EntityStates;");
			foreach (string item in RT_KnownEntities) {
				Debug.Log(item);
			}
		}
#endif

		if(currentState == null) {
#if DEBUG
			Debug.LogWarning("currentState returned null"); 
#endif
			return false;
		}
		return statesWhichDisableSprint.Contains(currentState.GetType());
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

	internal static bool TryParseBool(string input, out bool result){
		if(bool.TryParse(input,out result)) { return true; }
		if(int.TryParse(input,out int val)) { result = val > 0 ? true : false; return true; }
		return false;
	}

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