
/*
THINGS TO DO:
    Risk of Options 2 config
    Custom survivor disable/config
    Artificer Hold To Cast
    HoldSprintToWalk and walking auto-cancelling casts.
*/

#define DEBUGGY

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
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(R2API.R2API.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    
    public class RTAutoSprintEx : BaseUnityPlugin {
        public const string
            NAME = "RTAutoSprintEx",
            GUID = "com.johnedwa." + NAME,
            VERSION = "2.0.0";

        private static bool RT_enabled = true;
        private static bool RT_visuals = true;

#if DEBUGGY
        internal HashSet<string> knownEntityStates = new HashSet<string>();
#endif
        internal HashSet<string> statesWhichDisableSprint = new HashSet<string>();
        internal HashSet<string> statesWhichDelaySprint = new HashSet<string>();


        public static ConfigFile conf;  
        public static ConfigEntry<bool> SprintInAnyDirection { get; set; }
        public static ConfigEntry<bool> HoldSprintToWalk { get; set; }
        public static ConfigEntry<bool> ArtificerFlamethrowerToggle { get; set; }
        public static ConfigEntry<bool> DisableSprintingCrosshair { get; set; }
        //public static ConfigEntry<double> AnimationCancelDelay { get; set; }
        public static ConfigEntry<bool> DisableFOVChange { get; set; }
        public static ConfigEntry<bool> DisableSpeedlines { get; set; }
        public static ConfigEntry<int> CustomFOV { get; set; }        
        public static ConfigEntry<double> SprintFOVMultiplier { get; set; }
        public static ConfigEntry<bool> DisableAutoSprinting { get; set; }    
        public static ConfigEntry<bool> DisableVisualChanges { get; set; }     
            

        public void Awake() {

            double RT_timer = 0.0;
            double RT_animationCancelDelay = 0.15;
            bool RT_isSprinting = false;
            bool RT_animationCancel = false;
            bool RT_walkToggle = false;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            conf = new ConfigFile(Paths.ConfigPath + "\\com.johnedwa.RTAutoSprintEx.cfg", true);

            HoldSprintToWalk = conf.Bind<bool>(
                "1) Movement", "HoldSprintToWalk", true, 
                new ConfigDescription("Walk by holding down the sprint key. If disabled, makes the Sprint key toggle AutoSprinting functionality on and off.", 
                new AcceptableValueList<bool>(true, false)));
            SprintInAnyDirection = conf.Bind<bool>(
                "1) Movement", "SprintInAnyDirection", false, 
                new ConfigDescription("Cheat, Allows you to sprint in any direction.", 
                new AcceptableValueList<bool>(true, false)));
            DisableSprintingCrosshair = conf.Bind<bool>(
                "2) Visual", "DisableSprintingCrosshair", true, 
                new ConfigDescription("Disables the (useless) sprinting crosshair.", 
                new AcceptableValueList<bool>(true, false)));
            CustomFOV = conf.Bind<int>(
                "2) Visual", "FOVValue", -1, 
                new ConfigDescription("Change FOV. Game default is 60, set to -1 to disable change.", 
                new AcceptableValueRange<int>(-1, 359)));
            DisableFOVChange = conf.Bind<bool>(
                "2) Visual", "DisableFOVChange", false, 
                new ConfigDescription("Disables FOV change when sprinting", 
                new AcceptableValueList<bool>(true, false)));
            SprintFOVMultiplier = conf.Bind<double>(
                "2) Visual", "SprintFOVMultiplier", -1, 
                new ConfigDescription("Sets a custom sprinting FOV multiplier. Game default is 1.3, set to -1 to disable change.", 
                new AcceptableValueRange<double>(-1, 3)));
            DisableSpeedlines = conf.Bind<bool>(
                "2) Visual", "DisableSpeedlines", false, 
                new ConfigDescription("Disables speedlines while sprinting", 
                new AcceptableValueList<bool>(true, false)));
           DisableAutoSprinting = conf.Bind<bool>(
                "3) Misc", "DisabledAutoSprinting", false, 
                new ConfigDescription("Disable the AutoSprinting part of the mod and leaves only the FOV and visual changes.", 
                new AcceptableValueList<bool>(true, false)));
           DisableVisualChanges = conf.Bind<bool>(
                "3) Misc", "DisableVisualChanges", false, 
                new ConfigDescription("Disable the FOV and visual changes  of the mod and leaves only the AutoSprinting part.", 
                new AcceptableValueList<bool>(true, false)));

            RTAutoSprintEx.RT_enabled = !DisableAutoSprinting.Value;
            RTAutoSprintEx.RT_visuals = !DisableVisualChanges.Value;

            //CustomSurvivors = conf.Bind<string>("", "CustomSurvivorDisable", "", new ConfigDescription("List of custom survivors names that are disabled. The name is printed to the chat and log at spawn. Example: 'CustomSurvivorDisable: = SNIPER_NAME AKALI'"));
            //ArtificerFlamethrowerToggle = conf.Bind<bool>("", "ArtificerFlamethrowerToggle", true, new ConfigDescription("Artificer: Sprinting cancels the flamethrower, therefore it either has to disable AutoSprint for a moment, or you need to keep the button held down\ntrue: Flamethrower is a toggle, cancellable by hitting Sprint or casting M2\nfalse: Flamethrower is cast when the button is held down (binding to side mouse button recommended).", new AcceptableValueList<bool>(true, false)));

            // MUL-T
            RegisterSprintDisabler("EntityStates.Toolbot.ToolbotDualWield");
            RegisterSprintDisabler("EntityStates.Toolbot.ToolbotDualWieldBase");
            RegisterSprintDisabler("EntityStates.Toolbot.ToolbotDualWieldStart");
            RegisterSprintDisabler("EntityStates.Toolbot.FireNailgun");
            RegisterSprintDisabler("EntityStates.Toolbot.AimStunDrone");
            RegisterDelayer("EntityStates.Toolbot.FireGrenadeLauncher");

            // Artificer
            RegisterSprintDisabler("EntityStates.Mage.Weapon.Flamethrower");
            RegisterSprintDisabler("EntityStates.Mage.Weapon.PrepWall");
            RegisterDelayer("EntityStates.Mage.Weapon.FireFireBolt");
            RegisterDelayer("EntityStates.Mage.Weapon.FireLaserbolt");

            // Bandit
            RegisterSprintDisabler("EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState");
            RegisterSprintDisabler("EntityStates.Bandit2.Weapon.PrepSidearmResetRevolver");
            RegisterSprintDisabler("EntityStates.Bandit2.Weapon.PrepSidearmSkullRevolver");
            RegisterDelayer("EntityStates.Bandit2.Weapon.Bandit2FirePrimaryBase");
            RegisterDelayer("EntityStates.Bandit2.Weapon.FireShotgun2");
            RegisterDelayer("EntityStates.Bandit2.Weapon.Bandit2FireRifle");

            // Engineer
            RegisterSprintDisabler("EntityStates.Engi.EngiMissilePainter.Paint");
            RegisterDelayer("EntityStates.Engi.EngiWeapon.FireMines");
            RegisterDelayer("EntityStates.Engi.EngiWeapon.FireSeekerGrenades");

            // Rex
            RegisterSprintDisabler("EntityStates.Treebot.Weapon.AimMortar");
            RegisterSprintDisabler("EntityStates.Treebot.Weapon.AimMortar2");
            RegisterSprintDisabler("EntityStates.Treebot.Weapon.AimMortarRain");
            RegisterDelayer("EntityStates.Treebot.Weapon.FireSyringe");

            // Captain
            RegisterSprintDisabler("EntityStates.Captain.Weapon.SetupAirstrike");
            RegisterSprintDisabler("EntityStates.Captain.Weapon.SetupSupplyDrop");

            // Acrid
            RegisterDelayer("EntityStates.Croco.Slash");

            // Commando
            RegisterDelayer("EntityStates.Commando.CommandoWeapon.FirePistol2");

            // Loader
            RegisterDelayer("EntityStates.Loader.SwingComboFist");

            // Custom Characters and other mods
                // Artificer Extended
                RegisterDelayer("AltArtificerExtended.EntityStates.FireIceShard");
                RegisterDelayer("AltArtificerExtended.EntityStates.FireLaserbolts");
                RegisterDelayer("AltArtificerExtended.EntityStates.FireSnowBall");

            
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

                                if (!RT_isSprinting) {
                                    RT_timer += (double)Time.deltaTime;
                                    if (RT_timer >= 0.1) {
                                        if (!RT_animationCancel) { RT_timer = 0 - SprintDelayTime(instanceFieldBody); }
                                        if (RT_timer >= 0) {
                                            RT_timer = 0;
                                            RT_isSprinting = !ShouldSprintBeDisabledOnThisBody(instanceFieldBody);
                                            RT_animationCancel = false;
                                        }
                                    }
                                } else { RT_timer = 0; }

                                if (inputPlayer.GetButton("Sprint")) {
                                    if (RT_isSprinting && HoldSprintToWalk.Value) RT_isSprinting = false;
                                    if (!RT_isSprinting && ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) RT_isSprinting = true;
                                    RT_timer = 0;
                                }

                                if (!HoldSprintToWalk.Value && inputPlayer.GetButtonDown("Sprint")){
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
                                    if ((Vector3.Dot(aimDirection, moveVector) <= self.GetFieldValue<float>("sprintMinAimMoveDot"))) {
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
                    if (CustomFOV.Value > 0 && CustomFOV.Value != self.baseFov && CustomFOV.Value < 360) self.baseFov = CustomFOV.Value;
                }
            };

            // Sprinting Crosshair
            IL.RoR2.UI.CrosshairManager.UpdateCrosshair += (il) => {
                if (RTAutoSprintEx.RT_visuals) {
                    ILCursor c = new ILCursor(il);
                    if (DisableSprintingCrosshair.Value) {
                        Debug.Log("RtAutoSprintEx: Disabling sprinting crosshair:");
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
                    if (DisableFOVChange.Value) {
                        Debug.Log("RtAutoSprintEx: Disabling Sprint FOV Change:");
                        try {
                            c.Index = 0;
                            c.GotoNext(
                                x => x.MatchLdloc(0),
                                x => x.MatchLdarg(0),
                                x => x.MatchLdfld<RoR2.CameraRigController>("targetBody"),
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
                                x => x.MatchLdfld<RoR2.CameraRigController>("targetBody"),
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
                }
                Debug.Log("RtAutoSprintEx: CameraRigController.Update IL edits done.");
            };
        } // End of Awake

        // Registers EntityStates as sprint disablers
        public void RegisterSprintDisabler(string state){
#if DEBUGGY
            Debug.LogWarning("Sprint disabled for : " + state);
#endif
            statesWhichDisableSprint.Add(state);
        }

        // Registers EntityStates as sprint delayers    
        public void RegisterDelayer(string state){
#if DEBUGGY
            Debug.LogWarning("Sprint delay added for : " + state);
#endif
            statesWhichDelaySprint.Add(state);
        }

        public double SprintDelayTime(CharacterBody targetBody) {
            float duration = 0.0f;
            EntityStateMachine[] stateMachines;
            stateMachines = targetBody.GetComponents<EntityStateMachine>();
            foreach (EntityStateMachine machine in stateMachines) {
                var currentState = machine.state;
                if (currentState == null) { return duration; }
                if (statesWhichDelaySprint.Contains(currentState.ToString())) {
                    //try { duration = (double) currentState.GetFieldValue<float>("duration");} catch {}
                    var stateField = currentState.GetType().GetFieldCached("duration");
                    if (stateField != null) duration = (float)stateField.GetValue(currentState);
                    return duration;
                }
            }
            return duration;
        }

        // Checks if an EntityState blocks sprinting
        public bool ShouldSprintBeDisabledOnThisBody(CharacterBody targetBody) {
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
                if (statesWhichDisableSprint.Contains(currentState.ToString())) { isSprintBlocked = true; }
            }
            return isSprintBlocked;
        }
        
        // Console Commands
        [RoR2.ConCommand(commandName = "rt_help", flags = ConVarFlags.None, helpText = "List all RTAutoSprintEx console commands.")]
        private static void CCRTHelp(ConCommandArgs args) {
            Debug.Log("'rt_reload'. Reload the RTAutoSprintEx2.cfg configuration file.");
            Debug.Log("'rt_sprint_enable <bool>'. Default: true. Enables/Disables the sprinting part of the mod.");
            Debug.Log("'rt_visual_enable <bool>'. Default: true. Enables/Disables the visual part of the mod.");            
            //Debug.Log("'rt_sprintcheat <bool>'. Default: false. Allows you to sprint in any direction.");
            //Debug.Log("'rt_fov <int>'. Default: 60. Valid Range: 1-359. Sets the base FOV");
            //Debug.Log("'rt_disable_fov_change <bool>'\t Default false.");
            //Debug.Log("'rt_fov_multiplier <float>'\t Default: 1,3. Valid Range: 0.5-2.0. How much the camera FOV changes when sprinting.");
            //Debug.Log("'rt_disable_speedlines <bool>'\t Default: false.");
            //Debug.Log("'rt_disable_sprinting_crosshair <bool>'\t Default: true.");
            //Debug.Log("'rt_artificer_flamethrower_toggle <bool>'.  Default: true.");
            //Debug.Log("Rest of the options aren't currently run-time editable, you have to change them in the config:");
            //Debug.Log("AnimationCancelDelay, HoldSprintToWalk, DisableSprintingCrosshair, DisableSpeedlines, DisableFOVChange, SprintFOVMultiplier");
        }

        [ConCommand(commandName = "rt_reload", flags = ConVarFlags.None, helpText = "Reload the com.johnedwa.RTAutoSprintEx.cfg configuration file.")]
        private static void CCRTReload(ConCommandArgs args) {
            conf.Reload();
            Debug.Log("Configuration reloaded");
            
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

        [ConCommand(commandName = "rt_visual_enabled", flags = ConVarFlags.None, helpText = "Enable/Disable the visual component of the mod.")]
        private static void CCRTVisualEnable(ConCommandArgs args) {
            try {
                args.CheckArgumentCount(1);
                if (Utils.TryParseBool(args[0], out bool result)) {
                    RTAutoSprintEx.RT_enabled = (bool)result;
                    Debug.Log($"{nameof(RTAutoSprintEx.RT_visuals)}={RTAutoSprintEx.RT_visuals}");
                }
            } catch (Exception ex) { Debug.LogError(ex); }
        }
    } // End of class RTAutoSprintEx
} // End of Namespace