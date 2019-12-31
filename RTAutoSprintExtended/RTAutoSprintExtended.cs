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
	[BepInPlugin(PluginGuid, pluginName, pluginVersion)]

	public class RTAutoSprintEXTENDED : BaseUnityPlugin
	{
		public const string PluginGuid = "com.RelocityThrawnarchJohnEdwa.RTAutoSprintEXTENDED" + pluginName;
        private const string pluginName = "RTAutoSprintEXTENDED";
		private const string pluginVersion = "4478858.1.0";

		private static ConfigWrapper<bool> ArtificerFlamethrowerToggle;

		private static double RT_num;
		public static bool RT_autoSprint;
		public static bool RT_flameOn;

		public void Awake()
		{
			RTAutoSprintEXTENDED.RT_num = 0.0;

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

		// Artificer Flamethrower logic
			On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) => {
				if (ArtificerFlamethrowerToggle.Value) {
					RTAutoSprintEXTENDED.RT_flameOn = true;
				} else RTAutoSprintEXTENDED.RT_flameOn = false;
				orig(self);
			};
			On.EntityStates.Mage.Weapon.Flamethrower.FixedUpdate += (orig, self) => {
				if (Input.GetKeyDown(KeyCode.LeftShift)) {
					RTAutoSprintEXTENDED.RT_flameOn = false;
				}
				orig(self);
			};

			On.EntityStates.Mage.Weapon.Flamethrower.OnExit += (orig, self) => {
				RTAutoSprintEXTENDED.RT_flameOn = false;
				orig(self);
			};

		// Sprinting logic
			On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate(On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
				RTAutoSprintEXTENDED.RT_autoSprint = false;
				bool flag = false;
				RoR2.NetworkUser networkUser = self.networkUser;
				RoR2.InputBankTest instanceField = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
				bool flag2 = instanceField;
				if (flag2) {
					bool flag3 = networkUser && networkUser.localUser != null && !networkUser.localUser.isUIFocused;
					if (flag3) {
						Player inputPlayer = networkUser.localUser.inputPlayer;
						RoR2.CharacterBody instanceField2 = self.GetInstanceField<RoR2.CharacterBody>("body");
						bool flag4 = instanceField2;
						if (flag4) {
							RTAutoSprintEXTENDED.RT_autoSprint = instanceField2.isSprinting;
							bool flag5 = !RTAutoSprintEXTENDED.RT_autoSprint;
							if (flag5) {
								bool flag6 = RTAutoSprintEXTENDED.RT_num > 0.1;
								bool flag7 = flag6;
								if (flag7) {
									RTAutoSprintEXTENDED.RT_autoSprint = !RTAutoSprintEXTENDED.RT_autoSprint;
									RTAutoSprintEXTENDED.RT_num = 0.0;
								}
								bool flag8 = instanceField2.baseNameToken == "MAGE_BODY_NAME";
								if (flag8) {
									flag = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill") && !RTAutoSprintEXTENDED.RT_flameOn);
								} else {
									bool flag9 = instanceField2.baseNameToken == "ENGI_BODY_NAME";
									if (flag9) {
										flag = (!inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill"));
									} else {
										bool flag10 = instanceField2.baseNameToken == "HUNTRESS_BODY_NAME";
										if (flag10) {
											flag = (!inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
										} else {
											bool flag11 = instanceField2.baseNameToken == "MERC_BODY_NAME";
											if (flag11) {
												flag = (!inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
											} else {
												flag = (!inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill") && !inputPlayer.GetButton("SpecialSkill"));
											}
										}
									}
								}
							}
							bool flag12 = flag;
							if (flag12) {
								RTAutoSprintEXTENDED.RT_num += (double)Time.deltaTime;
							} else {
								RTAutoSprintEXTENDED.RT_num = 0.0;
							}
							bool rt_autoSprint = RTAutoSprintEXTENDED.RT_autoSprint;
							if (rt_autoSprint) {
								Vector3 aimDirection = instanceField.aimDirection;
								aimDirection.y = 0f;
								aimDirection.Normalize();
								Vector3 moveVector = instanceField.moveVector;
								moveVector.y = 0f;
								moveVector.Normalize();
								bool flag13 = Vector3.Dot(aimDirection, moveVector) < self.GetInstanceField<float>("sprintMinAimMoveDot");
								if (flag13) {
									RTAutoSprintEXTENDED.RT_autoSprint = false;
								}
							}
						}
					}
				}
				orig.Invoke(self);
				bool flag14 = instanceField;
				if (flag14) {
					instanceField.sprint.PushState(RTAutoSprintEXTENDED.RT_autoSprint);
				}
			};
			Debug.Log("Loaded RT AutoSprint Extended\nArtificer flamethrower mode is " + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
			//RoR2.Chat.AddMessage("Loaded RT AutoSprint Extended\nArtificer flamethrower mode is " + ((ArtificerFlamethrowerToggle.Value) ? " [toggle]." : " [hold]."));
		}

		/*
		public void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
				ArtificerFlamethrowerToggle.Value = !ArtificerFlamethrowerToggle.Value;
				RoR2.Chat.AddMessage("Flamethrower Toggle: " + ArtificerFlamethrowerToggle.Value);
			}
        }
		*/


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