using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Rewired;
using UnityEngine;
using EnigmaticThunder.Util;
using RoR2;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates;

namespace RTAutoSprintEx {
//[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
[BepInDependency(EnigmaticThunder.EnigmaticThunder.guid, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(GUID, NAME, VERSION)]

public class RTAutoSprintEx : BaseUnityPlugin {
	public const string
		NAME = "RTAutoSprintEx",
		GUID = "com.johnedwa." + NAME,
		VERSION = "2.0.0";

	private static double RT_num;
	private static bool RT_enabled;
	private static bool RT_isSprinting;
	private static bool RT_cancelWithSprint;
	private static bool RT_tempDisable;
	private string[] RT_CustomSurvivors;
	private bool RT_CustomSurvivorDisable;

	internal HashSet<Type> statesWhichDisableSprint = new HashSet<Type>();

	public void RegisterSprintDisabler<T>() where T : BaseState {
		Debug.LogWarning("Sprint Disable registered for: " + typeof(T));
		statesWhichDisableSprint.Add(typeof(T));
	}

	public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
		//var currentState = targetBody.GetComponent<EntityStateMachine>()?.state;
		var currentState = targetBody.GetComponent<EntityStateMachine>().state; // this is always EntityStates.GenericCharacterMain
		Debug.LogWarning(currentState); 
		if(currentState == null) return false;
		return statesWhichDisableSprint.Contains(currentState.GetType());
	}


	public void Awake() {

		RTAutoSprintEx.RT_num = 0.0;
		RTAutoSprintEx.RT_enabled = true;
		RTAutoSprintEx.RT_isSprinting = false;
		RTAutoSprintEx.RT_cancelWithSprint = false;
		RTAutoSprintEx.RT_tempDisable = false;
		RT_CustomSurvivorDisable = false;

		RegisterSprintDisabler<EntityStates.Toolbot.FireNailgun>();
		RegisterSprintDisabler<EntityStates.Toolbot.FireGrenadeLauncher>();
		RegisterSprintDisabler<EntityStates.Toolbot.AimGrenade>();
		RegisterSprintDisabler<EntityStates.Captain.Weapon.SetupAirstrike>();
		RegisterSprintDisabler<EntityStates.Captain.Weapon.SetupSupplyDrop>();

		On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
			orig.Invoke(self);

			//RTAutoSprintEx.RT_isSprinting = false;
			bool skillsAllowAutoSprint = false;
			bool knownSurvivor = true;
			
			RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
			if (instanceFieldBodyInputs) {
				RoR2.NetworkUser networkUser = self.networkUser;
				if (networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused) {
					RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
					if (instanceFieldBody && RTAutoSprintEx.RT_enabled) {
						Player inputPlayer = networkUser.localUser.inputPlayer;
						RTAutoSprintEx.RT_isSprinting = instanceFieldBody.isSprinting;
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
									skillsAllowAutoSprint = (!RTAutoSprintEx.RT_tempDisable);
									break;
								default:									
									break;
							}

						
						Debug.LogWarning(self.GetInstanceField<RoR2.CharacterBody>("body").GetComponent<EntityStateMachine>().state);

						//ShouldSprintBeDisabledOnThisBody(instanceFieldBody);
					}
				}

				if (instanceFieldBodyInputs && RTAutoSprintEx.RT_enabled && knownSurvivor) {
					instanceFieldBodyInputs.sprint.PushState(RTAutoSprintEx.RT_isSprinting);
				}
			}
}; // End of FixedUpdate
	} // End of Awake
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