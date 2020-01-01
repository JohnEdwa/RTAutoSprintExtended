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




namespace RT_AutoSprint
{
	[BepInDependency("com.bepis.r2api")]
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]

	public class RTAutoSprintEx : BaseUnityPlugin
	{
		public const string pluginGuid = "com.RelocityThrawnarchJohnEdwa" + pluginName;
        private const string pluginName = "RTAutoSprintEx";
		private const string pluginVersion = "4478858.1.1";

		private static ConfigWrapper<bool> ArtificerFlamethrowerToggle;

		private static double RT_num;
		public static bool RT_isSprinting;
		public static bool RT_flameOn;

		public void Awake()
		{
			RTAutoSprintEx.RT_num = 0.0;

		// Configuration
			On.RoR2.Console.Awake += (orig, self) => {
				CommandHelper.RegisterCommands(self);
				orig(self);
			};

			ArtificerFlamethrowerToggle = Config.Wrap<bool>(
				"Artificer",
				"ArtificerFlamethrowerToggle",
				"Sprinting cancels the flamethrower, therefore it either has to disable AutoSprint for a moment, or you need to keep the button held down\ntrue: Flamethrower is a toggle, cancellable by hitting Sprint or casting M2\nfalse: Flamethrower is cast when the button is held down (binding to side mouse button recommended).",
				true
			);

			EngineerAllowM2Sprint = Config.Wrap<bool>(
				"Engineer",
				"EngineerM2Sprint",
				"Allows Engineer to auto-sprint between throwing mines. Looks really janky but technically possible.",
				false
			);

		// Artificer Flamethrower workaround logic
			On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) => {
				if (ArtificerFlamethrowerToggle.Value) {
					RTAutoSprintEx.RT_flameOn = true;
				} else RTAutoSprintEx.RT_flameOn = false;
				orig(self);
			};
			On.EntityStates.Mage.Weapon.Flamethrower.FixedUpdate += (orig, self) => {
				// This is for cancelling mid-cast by hitting Sprint
				if (Input.GetKeyDown(KeyCode.LeftShift)) {
					RTAutoSprintEx.RT_flameOn = false;
				}
				orig(self);
			};

			On.EntityStates.Mage.Weapon.Flamethrower.OnExit += (orig, self) => {
				RTAutoSprintEx.RT_flameOn = false;
				orig(self);
			};

		// Sprinting logic
			On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
				RTAutoSprintEx.RT_isSprinting = false;
				bool skillsAllowAutoSprint = false;
				RoR2.NetworkUser networkUser = self.networkUser;
				RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
				if (instanceFieldBodyInputs) {
					if (networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused) {
						Player inputPlayer = networkUser.localUser.inputPlayer;
						RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
						if (instanceFieldBody) {
							RTAutoSprintEx.RT_isSprinting = instanceFieldBody.isSprinting;
							if (!RTAutoSprintEx.RT_isSprinting) {
								if (RTAutoSprintEx.RT_num > 0.1) {
									RTAutoSprintEx.RT_isSprinting = !RTAutoSprintEx.RT_isSprinting;
									RTAutoSprintEx.RT_num = 0.0;
								}
								switch(instanceFieldBody.baseNameToken){
									case "COMMANDO_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;
									case "MAGE_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill") && !RTAutoSprintEx.RT_flameOn);
										break;
									case "ENGI_BODY_NAME":
										if (EngineerAllowM2Sprint.Value) skillsAllowAutoSprint = (!inputPlayer.GetButton("UtilitySkill"));
										else skillsAllowAutoSprint = (!inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("UtilitySkill"));
										break;									
									case "HUNTRESS_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("SpecialSkill"));
										break;
									case "MERC_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
										break;
									case "LOADER_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;											
									case "ACRID_BODY_NAME":
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill"));
										break;
									default:
										skillsAllowAutoSprint = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
										break;
								}
							}
							if (skillsAllowAutoSprint) {
								RTAutoSprintEx.RT_num += (double)Time.deltaTime;
							} else {
								RTAutoSprintEx.RT_num = 0.0;
							}
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
					}
				}
				orig.Invoke(self);
				if (instanceFieldBodyInputs) {
					instanceFieldBodyInputs.sprint.PushState(RTAutoSprintEx.RT_isSprinting);
				}
			};
			Debug.Log("Loaded RT AutoSprint Extended\nArtificer flamethrower mode is " + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
		}

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
	}

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
}