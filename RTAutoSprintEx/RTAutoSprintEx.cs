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
//[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
//[BepInDependency(EnigmaticThunder.EnigmaticThunder.guid, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(GUID, NAME, VERSION)]

public class RTAutoSprintEx : BaseUnityPlugin {
	public const string
		NAME = "RTAutoSprintEx",
		GUID = "com.johnedwa." + NAME,
		VERSION = "2.0.0";

	private static bool RT_enabled;
	private static bool RT_isSprinting;

	internal HashSet<Type> statesWhichDisableSprint = new HashSet<Type>();

	public void RegisterSprintDisabler<T>() where T : BaseState {
		Debug.LogWarning("Sprint Disable registered for: " + typeof(T));
		statesWhichDisableSprint.Add(typeof(T));
	}

	public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
		var currentState = targetBody.GetComponent<EntityStateMachine>().state; 
			// EntityStates.GenericCharacterMain
			// EntityStates.Toolbot.ToolbotDualWield
			// EntityStates.Toolbot.ToolbotDash
		Debug.LogWarning("Character " + targetBody.name + " state is " + currentState); 
		if(currentState == null) return false;
		return statesWhichDisableSprint.Contains(currentState.GetType());
	}

	public void Awake() {

		RTAutoSprintEx.RT_enabled = true;
		RTAutoSprintEx.RT_isSprinting = false;

		R2API.Utils.CommandHelper.AddToConsoleWhenReady();

		RegisterSprintDisabler<EntityStates.Toolbot.ToolbotDualWield>();

		On.RoR2.CharacterBody.OnSkillActivated += (orig, self, GenericSkill) => { 
			orig(self, GenericSkill); 
			Debug.Log( GenericSkill.skillDef.skillName + " | Index: ");
		};

		On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
			orig.Invoke(self);
			RTAutoSprintEx.RT_isSprinting = false;
			RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
			if (instanceFieldBodyInputs) {
				RoR2.NetworkUser networkUser = self.networkUser;
				if (networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused) {
					RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
					if (instanceFieldBody && RTAutoSprintEx.RT_enabled) {
						Player inputPlayer = networkUser.localUser.inputPlayer;
						if (!instanceFieldBody.isSprinting) {
							if (!ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) instanceFieldBodyInputs.sprint.PushState(true);
						}
					}
				}
			}
}; // End of FixedUpdate
	} // End of Awake

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

internal static bool TryParseBool(string input, out bool result){ if(bool.TryParse(input,out result)){return true;} if(int.TryParse(input,out int val)) { result = val > 0 ? true : false; return true; } return false;}

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