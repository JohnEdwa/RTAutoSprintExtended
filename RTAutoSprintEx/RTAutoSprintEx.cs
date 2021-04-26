/*
THINGS TO DO:
    Risk of Options 2 config
    Artificer Hold To Cast
    HoldSprintToWalk and walking auto-cancelling casts.
*/

//#define DEBUGGY

using System;
using System.IO;
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
    [BepInPlugin("com.johnedwa.RTAutoSprintEx", "RTAutoSprintEx", "2.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]

    public class RTAutoSprintEx : BaseUnityPlugin {
        private static bool RT_enabled = true;
        private static bool RT_visuals = true;

#if DEBUGGY
        internal HashSet<string> knownEntityStates = new HashSet<string>();
#endif
        internal HashSet<string> stateSprintDisableList = new HashSet<string>();
        internal HashSet<string> stateAnimationDelayList = new HashSet<string>();

        // Receive SendMessages.
        public void RT_SprintDisableMessage(string state) { stateSprintDisableList.Add(state); Logger.LogInfo("Received RT_SprintDisableMessage for " + state);}
        public void RT_AnimationDelayMessage(string state) { stateAnimationDelayList.Add(state); Logger.LogInfo("Received RT_AnimationDelayMessage for " + state);}

        // Registers EntityStates as sprint delayers    
        private bool RT_RegisterAnimationDelay(string state) {
#if DEBUGGY
            Debug.LogWarning("Sprint delay added for : " + state);
#endif
            return stateAnimationDelayList.Add(state);
        }

        // Registers EntityStates as sprint disablers
        private bool RT_RegisterSprintDisable(string state) {
#if DEBUGGY
            Debug.LogWarning("Sprint disabled for : " + state);
#endif
            return stateSprintDisableList.Add(state);
        }

        // Checks if the duration value exists.
        private double SprintDelayTime(CharacterBody targetBody) {
            float duration = 0.0f;
            EntityStateMachine[] stateMachines;
            stateMachines = targetBody.GetComponents<EntityStateMachine>();
            foreach (EntityStateMachine machine in stateMachines) {
                var currentState = machine.state;
                if (currentState == null) { return duration; }
                if (stateAnimationDelayList.Contains(currentState.ToString())) {
                    try { duration = currentState.GetFieldValue<float>("duration"); } catch { }
                    //var stateField = currentState.GetType().GetFieldCached("duration");
                    //if (stateField != null) duration = (float)stateField.GetValue(currentState);
                    return duration;
                }
            }
            return duration;
        }

        // Checks if an EntityState blocks sprinting
        private bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
            EntityStateMachine[] stateMachines;
            stateMachines = targetBody.GetComponents<EntityStateMachine>();
            bool isSprintBlocked = false;
            foreach (EntityStateMachine machine in stateMachines) {
                var currentState = machine.state;
                if (currentState == null) { return false; }
#if DEBUGGY
                if (!knownEntityStates.Contains(currentState.ToString())) {
                    knownEntityStates.Add(currentState.ToString());
                    Debug.LogError("List of Known EntityStates;");
                    foreach (var item in knownEntityStates) {
                        Debug.Log(item);
                    }
                }
#endif
                if (stateSprintDisableList.Contains(currentState.ToString())) { isSprintBlocked = true; }
            }
            return isSprintBlocked;
        }

        public void Awake() {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            double RT_timer = 0.0;
            double RT_animationCancelDelay = 0.15;
            bool RT_isSprinting = false;
            bool RT_animationCancel = false;
            bool RT_walkToggle = false;

            SetupConfiguration();

            // MUL-T
            RT_RegisterSprintDisable("EntityStates.Toolbot.ToolbotDualWield");
            RT_RegisterSprintDisable("EntityStates.Toolbot.ToolbotDualWieldBase");
            RT_RegisterSprintDisable("EntityStates.Toolbot.ToolbotDualWieldStart");
            RT_RegisterSprintDisable("EntityStates.Toolbot.FireNailgun");
            RT_RegisterSprintDisable("EntityStates.Toolbot.AimStunDrone");
            RT_RegisterAnimationDelay("EntityStates.Toolbot.FireGrenadeLauncher");

            // Artificer
            RT_RegisterSprintDisable("EntityStates.Mage.Weapon.Flamethrower");
            RT_RegisterSprintDisable("EntityStates.Mage.Weapon.PrepWall");
            RT_RegisterAnimationDelay("EntityStates.Mage.Weapon.FireFireBolt");
            RT_RegisterAnimationDelay("EntityStates.Mage.Weapon.FireLaserbolt");

            // Bandit
            RT_RegisterSprintDisable("EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState");
            RT_RegisterSprintDisable("EntityStates.Bandit2.Weapon.PrepSidearmResetRevolver");
            RT_RegisterSprintDisable("EntityStates.Bandit2.Weapon.PrepSidearmSkullRevolver");
            RT_RegisterAnimationDelay("EntityStates.Bandit2.Weapon.Bandit2FirePrimaryBase");
            RT_RegisterAnimationDelay("EntityStates.Bandit2.Weapon.FireShotgun2");
            RT_RegisterAnimationDelay("EntityStates.Bandit2.Weapon.Bandit2FireRifle");

            // Engineer
            RT_RegisterSprintDisable("EntityStates.Engi.EngiMissilePainter.Paint");
            RT_RegisterAnimationDelay("EntityStates.Engi.EngiWeapon.FireMines");
            RT_RegisterAnimationDelay("EntityStates.Engi.EngiWeapon.FireSeekerGrenades");

            // Rex
            RT_RegisterSprintDisable("EntityStates.Treebot.Weapon.AimMortar");
            RT_RegisterSprintDisable("EntityStates.Treebot.Weapon.AimMortar2");
            RT_RegisterSprintDisable("EntityStates.Treebot.Weapon.AimMortarRain");
            RT_RegisterAnimationDelay("EntityStates.Treebot.Weapon.FireSyringe");

            // Captain
            RT_RegisterSprintDisable("EntityStates.Captain.Weapon.SetupAirstrike");
            RT_RegisterSprintDisable("EntityStates.Captain.Weapon.SetupAirstrikeAlt");
            RT_RegisterSprintDisable("EntityStates.Captain.Weapon.SetupSupplyDrop");

            // Acrid
            RT_RegisterAnimationDelay("EntityStates.Croco.Slash");

            // Commando
            RT_RegisterAnimationDelay("EntityStates.Commando.CommandoWeapon.FirePistol2");

            // Loader
            RT_RegisterAnimationDelay("EntityStates.Loader.SwingComboFist");

            // On.RoR2.CharacterBody.OnSkillActivated += (orig, self, GenericSkill) => { 
            //     orig(self, GenericSkill); 
            //     Debug.Log( 
            //             GenericSkill.skillDef.skillName  + " | "          // FireFirebolt
            //         + GenericSkill + " | "                                  // MageBody(Clone) (RoR2.GenericSkill)
            //         + GenericSkill.characterBody + " | "                // MageBody(Clone) (RoR2.CharacterBody)
            //         + GenericSkill.characterBody.name + " | "           // MageBody(Clone)
            //         + GenericSkill.characterBody.master + " | "         // CommandoMaster(Clone) (RoR2.CharacterMaster)
            //         + GenericSkill.characterBody.masterObject + " | "   // CommandoMaster(Clone) (UnityEngine.GameObject)           
            //         );
            // }; 

            On.RoR2.PlayerCharacterMasterController.FixedUpdate += delegate (On.RoR2.PlayerCharacterMasterController.orig_FixedUpdate orig, RoR2.PlayerCharacterMasterController self) {
                orig.Invoke(self);
                if (RT_enabled) {
                    RoR2.InputBankTest instanceFieldBodyInputs = self.GetInstanceField<RoR2.InputBankTest>("bodyInputs");
                    if (instanceFieldBodyInputs) {
                        if (self.networkUser && self.networkUser.localUser != null && !self.networkUser.localUser.isUIFocused) {
                            RoR2.CharacterBody instanceFieldBody = self.GetInstanceField<RoR2.CharacterBody>("body");
                            if (instanceFieldBody) {
                                Player inputPlayer = self.networkUser.localUser.inputPlayer;
                                RT_isSprinting = instanceFieldBody.isSprinting;
                                // Periodic sprint checker
                                if (!RT_isSprinting) {
                                    RT_timer += (double)Time.deltaTime;
                                    if (RT_timer >= 0.05) {
                                        if (!RT_animationCancel) {
                                            RT_timer = 0 - SprintDelayTime(instanceFieldBody);
                                        }
                                        if (RT_timer >= 0) {
                                            RT_isSprinting = !ShouldSprintBeDisabledOnThisBody(instanceFieldBody);
                                            RT_animationCancel = false;
                                            RT_timer = 0;
                                        }
                                    }
                                } else { RT_timer = 0; }

                                // Walking Logic
                                if (inputPlayer.GetButton("Sprint")) {
                                    if (RT_isSprinting && HoldSprintToWalk.Value) RT_isSprinting = false;
                                    if (!RT_isSprinting && ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) RT_isSprinting = true;
                                    RT_timer = 0;
                                }
                                // Walk Toggle logic
                                if (!HoldSprintToWalk.Value && inputPlayer.GetButtonDown("Sprint")) {
                                    RT_walkToggle = !RT_walkToggle;
                                }

                                // Animation cancelling logic.
                                if (!RT_animationCancel && RT_timer < -(RT_animationCancelDelay)
                                    && !inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill")
                                    && !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill")) {
                                    RT_timer = -(RT_animationCancelDelay);
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
                                    if ((instanceFieldBody.bodyFlags & CharacterBody.BodyFlags.SprintAnyDirection) == CharacterBody.BodyFlags.None && (Vector3.Dot(aimDirection, moveVector) < self.GetFieldValue<float>("sprintMinAimMoveDot"))) {
                                        RT_isSprinting = false;
                                    }
                                }

                                if (HoldSprintToWalk.Value && RT_walkToggle) RT_walkToggle = false;
                                if (!RT_walkToggle) instanceFieldBodyInputs.sprint.PushState(RT_isSprinting);
                            } // End of if (instanceFieldBody)
                        } // End of if (networkUser) check
                    } // End of if (instanceFieldBodyInputs)
                } // End of "RT_enabled"
            }; // End of FixedUpdate

            // Custom FOV
            On.RoR2.CameraRigController.Update += (orig, self) => {
                orig(self);
                if (RTAutoSprintEx.RT_visuals) {
                    if (CustomFOV.Value >= 1 && CustomFOV.Value <= 180) self.baseFov = CustomFOV.Value;
                    else Logger.LogWarning("CustomFOV value out of range!");
                }
            };

            // Sprinting Crosshair
            IL.RoR2.UI.CrosshairManager.UpdateCrosshair += (il) => {
                if (RTAutoSprintEx.RT_visuals) {
                    ILCursor c = new ILCursor(il);
                    if (DisableSprintingCrosshair.Value) {
                        Logger.LogInfo("Disabling sprinting crosshair:");
                        try {
                            c.Index = 0;
                            c.GotoNext(
                                MoveType.After,
                                x => x.MatchCallvirt<CharacterBody>("get_isSprinting")
                            );
                            c.Emit(OpCodes.Ldc_I4, 0);
                            c.Emit(OpCodes.And);
                        } catch (Exception ex) { Debug.LogError(ex); }
                    }
                }
            };

            //Sprinting FOV change
            IL.RoR2.CameraRigController.Update += (il) => {
                if (RTAutoSprintEx.RT_visuals) {
                    ILCursor c = new ILCursor(il);
                    if (SprintFOVMultiplier.Value >= 0.1 && SprintFOVMultiplier.Value <= 2.0) {
                        Logger.LogInfo("Modifying Sprint FOV Multiplier:");
                        try {
                            c.Index = 0;
                            c.GotoNext(
                                x => x.MatchLdloc(0),
                                x => x.MatchLdarg(0),
                                x => x.MatchLdfld<RoR2.CameraRigController>("targetBody"),
                                x => x.MatchCallvirt<RoR2.CharacterBody>("get_isSprinting")
                            );
                            c.Index += 7;
                            c.Next.Operand = (float)SprintFOVMultiplier.Value;
                        } catch (Exception ex) { Debug.LogError(ex); }
                    } else Logger.LogWarning("SprintFOVMultiplier value out of range!");
                    // Disable Speedlines
                    if (DisableSpeedlines.Value) {
                        Logger.LogInfo("Disabling Speedlines:");
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
                }
                Logger.LogInfo("CameraRigController.Update IL edits done.");
            };
        } // End of Awake

        // CONSOLE COMMANDS
        [RoR2.ConCommand(commandName = "rt_help", flags = ConVarFlags.None, helpText = "List all RTAutoSprintEx console commands.")]
        private static void CCRTHelp(ConCommandArgs args) {
            Debug.Log("'rt_reload_config'. Reload the RTAutoSprintEx2.cfg configuration file");
            Debug.Log("'rt_sprint_enable <bool>'. Default: true. Enables/Disables the sprinting part of the mod.");
            //Debug.Log("'rt_sprintcheat <bool>'. Default: false. Allows you to sprint in any direction.");
            Debug.Log("'rt_fov <int>'. Default: 60. Valid Range: 1-180. Sets the base FOV.");
            //Debug.Log("'rt_fov_multiplier <float>'\t Default: 1,3. Valid Range: 0.5-2.0. How much the camera FOV changes when sprinting.");
            //Debug.Log("'rt_disable_speedlines <bool>'\t Default: false.");
            //Debug.Log("'rt_disable_sprinting_crosshair <bool>'\t Default: true.");
            //Debug.Log("'rt_artificer_flamethrower_toggle <bool>'.  Default: true.");
        }

        [ConCommand(commandName = "rt_reload_config", flags = ConVarFlags.None, helpText = "Reload the com.johnedwa.RTAutoSprintEx.cfg configuration file")]
        private static void CCRTReload(ConCommandArgs args) {
            conf.Reload();
            Debug.Log("Configuration hopefully reloaded. DisableSpeedlines and DisableSprintingCrosshair will require a game restart.");
        }

        [ConCommand(commandName = "rt_sprint_enabled", flags = ConVarFlags.None, helpText = "Enable/Disable the sprinting component of the mod.")]
        private static void CCRTSprintEnable(ConCommandArgs args) {
            try {
                args.CheckArgumentCount(1);
                if (Utils.TryParseBool(args[0], out bool result)) {
                    RTAutoSprintEx.RT_enabled = (bool)result;
                    Debug.Log($"{nameof(RTAutoSprintEx.RT_enabled)}={RTAutoSprintEx.RT_enabled}");
                }
            } catch (Exception ex) { Debug.LogError(ex); }
        }

        [ConCommand(commandName = "rt_fov", flags = ConVarFlags.None, helpText = "Default: 60. Valid Range: 1-180. Sets the base FOV.")]
        private static void CCRTFOV(ConCommandArgs args) {
            try {
                args.CheckArgumentCount(1);
                int? value = args.TryGetArgInt(0);
                if (value.HasValue) {
                    if (value >= 1 && value <= 180) {
                        CustomFOV.Value = (int)value;
                        Debug.Log($"{nameof(CustomFOV)}={value}");
                    }
                }
            } catch (Exception ex) { Debug.LogError(ex); }
        }

        // CONFIGURATION
        public static ConfigFile conf;
        public static ConfigEntry<bool> SprintInAnyDirection { get; set; }
        public static ConfigEntry<bool> HoldSprintToWalk { get; set; }
        public static ConfigEntry<bool> DisableSprintingCrosshair { get; set; }
        public static ConfigEntry<bool> DisableFOVChange { get; set; }
        public static ConfigEntry<bool> DisableSpeedlines { get; set; }
        public static ConfigEntry<int> CustomFOV { get; set; }
        public static ConfigEntry<double> SprintFOVMultiplier { get; set; }
        public static ConfigEntry<bool> DisableAutoSprinting { get; set; }
        public static ConfigEntry<bool> DisableVisualChanges { get; set; }
        public static ConfigEntry<string> EntityStatesSprintingDisabled { get; set; }
        public static ConfigEntry<string> EntityStatesSprintingDelay { get; set; }

        private void SetupConfiguration() {

            string path = Paths.ConfigPath + "\\com.johnedwa.RTAutoSprintEx.cfg";
            // I'm sure there is some other way to do this, but I don't know it.
            /* 
            try {
                var sr = new StreamReader(path);
                var fileContents = sr.ReadToEnd();
                if (fileContents.Contains("ArtificerFlamethrowerToggle")) {
                    sr.Close();
                    var sw = new StreamWriter(path);
                    sw.Flush();
                    sw.Close();
                } else sr.Close();
            } catch {};
            */

            conf = new ConfigFile(path, true);

            HoldSprintToWalk = conf.Bind<bool>(
                "1) Movement", "HoldSprintToWalk", true,
                new ConfigDescription("Walk by holding down the sprint key. If disabled, makes the Sprint key toggle AutoSprinting functionality on and off.",
                new AcceptableValueList<bool>(true, false)));
            SprintInAnyDirection = conf.Bind<bool>(
                "1) Movement", "SprintInAnyDirection", false,
                new ConfigDescription("Cheat, Allows you to sprint in any direction. Please don't use in multiplayer.",
                new AcceptableValueList<bool>(true, false)));
            DisableSprintingCrosshair = conf.Bind<bool>(
                "2) Visual", "DisableSprintingCrosshair", true,
                new ConfigDescription("Disables the useless special sprinting chevron crosshair.",
                new AcceptableValueList<bool>(true, false)));
            CustomFOV = conf.Bind<int>(
                "2) Visual", "FOVValue", 60,
                new ConfigDescription("Sets a custom (vertical) FOV. Game default 60V is roughly 90H.",
                new AcceptableValueRange<int>(1, 180)));
            SprintFOVMultiplier = conf.Bind<double>(
                "2) Visual", "SprintFOVMultiplier", 1.3,
                new ConfigDescription("Sets the sprinting FOV multiplier. Set to 1 to disable.",
                new AcceptableValueRange<double>(0.1, 2)));
            DisableSpeedlines = conf.Bind<bool>(
                "2) Visual", "DisableSpeedlines", false,
                new ConfigDescription("Disables the speedlines effect shown when sprinting.",
                new AcceptableValueList<bool>(true, false)));
            DisableAutoSprinting = conf.Bind<bool>(
                 "3) Misc", "DisabledAutoSprinting", false,
                 new ConfigDescription("Disable the AutoSprinting part of the mod.",
                 new AcceptableValueList<bool>(true, false)));
            DisableVisualChanges = conf.Bind<bool>(
                 "3) Misc", "DisableVisualChanges", false,
                 new ConfigDescription("Disable the FOV and visual changes of the mod.",
                 new AcceptableValueList<bool>(true, false)));

            RTAutoSprintEx.RT_enabled = !DisableAutoSprinting.Value;
            RTAutoSprintEx.RT_visuals = !DisableVisualChanges.Value;
            if (!RTAutoSprintEx.RT_enabled && !RTAutoSprintEx.RT_visuals) Logger.LogInfo("DisabledAutoSprinting and DisableVisualChanges both True, the mod literally does nothing now.");
        } // End of SetupConfiguration()
    } // End of class RTAutoSprintEx
} // End of Namespace