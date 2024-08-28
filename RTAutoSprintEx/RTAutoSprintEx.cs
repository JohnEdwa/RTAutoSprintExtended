#define DEBUGGY

using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Rewired; 
using UnityEngine;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;


namespace RTAutoSprintEx {
    [BepInPlugin("com.johnedwa.RTAutoSprintEx", "RTAutoSprintEx", "3.0.0")]

    public class RTAutoSprintEx : BaseUnityPlugin {

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
                    try { duration = currentState.GetInstanceField<float>("duration"); } catch { }
                    return duration;
                }
            }
            return duration;
        }

        // Checks if an EntityState blocks sprinting
        private bool ShouldSprintBeDisabledOnThisBody(RoR2.CharacterBody targetBody) {
            EntityStateMachine[] stateMachines;
            stateMachines = targetBody.GetComponents<EntityStateMachine>();
            bool isSprintBlocked = false;
            foreach (EntityStateMachine machine in stateMachines) {
                var currentState = machine.state;
                if (currentState == null) { return false; }
                if (PrintEntityStates.Value) {
                    if (!knownEntityStates.Contains(currentState.ToString())) {
                        knownEntityStates.Add(currentState.ToString());
                        Debug.LogError("List of Known EntityStates;");
                        foreach (var item in knownEntityStates) {
                            Debug.Log(item);
                        }
                    }
                }
                if (stateSprintDisableList.Contains(currentState.ToString())) { isSprintBlocked = true; }
            }
            return isSprintBlocked;
        }

        public void Awake() {
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

            // Void thing
            RT_RegisterAnimationDelay("EntityStates.VoidSurvivor.Weapon.FireHandBeam");
            RT_RegisterAnimationDelay("EntityStates.VoidSurvivor.Weapon.ChargeCorruptHandBeam");
            RT_RegisterSprintDisable("EntityStates.VoidSurvivor.Weapon.FireCorruptHandBeam");

            // Railgunner
            RT_RegisterAnimationDelay("EntityStates.Railgunner.Weapon.FirePistol");
            RT_RegisterSprintDisable("EntityStates.Railgunner.Scope.WindUpScopeLight");
            RT_RegisterSprintDisable("EntityStates.Railgunner.Scope.ActiveScopeLight");
            RT_RegisterSprintDisable("EntityStates.Railgunner.Scope.WindUpScopeHeavy");
            RT_RegisterSprintDisable("EntityStates.Railgunner.Scope.ActiveScopeHeavy");

            // CHEF

            // Seeker

            // [SECRET]

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

            On.RoR2.PlayerCharacterMasterController.Update += (orig, self) => {
                orig(self);
                    InputBankTest instanceFieldBodyInputs = self.GetInstanceField<InputBankTest>("bodyInputs");
                    if (instanceFieldBodyInputs) {
                        if (self.networkUser && self.networkUser.localUser != null && !self.networkUser.localUser.isUIFocused) {
                            CharacterBody instanceFieldBody = self.GetInstanceField<CharacterBody>("body");
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

                                // Walk Toggle logic
                                if (!HoldSprintToWalk.Value && inputPlayer.GetButtonDown("Sprint") && !ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) {
                                    RT_walkToggle = !RT_walkToggle;
                                } else if (inputPlayer.GetButton("Sprint")) {
                                    if (RT_isSprinting && HoldSprintToWalk.Value) RT_isSprinting = false;
                                    if (!RT_isSprinting && ShouldSprintBeDisabledOnThisBody(instanceFieldBody)) RT_isSprinting = true;
                                    RT_timer = 0;
                                }


                                // Animation cancelling logic.
                                if (!RT_animationCancel && RT_timer < -(RT_animationCancelDelay)
                                    && !inputPlayer.GetButton("PrimarySkill") && !inputPlayer.GetButton("SecondarySkill")
                                    && !inputPlayer.GetButton("SpecialSkill") && !inputPlayer.GetButton("UtilitySkill")) {
                                    RT_timer = -(RT_animationCancelDelay);
                                    RT_animationCancel = true;
                                }

                                // Angle check disables sprinting if the movement angle is too large
                                if (RT_isSprinting) {
                                    Vector3 aimDirection = instanceFieldBodyInputs.aimDirection;
                                    aimDirection.y = 0f;
                                    aimDirection.Normalize();
                                    Vector3 moveVector = instanceFieldBodyInputs.moveVector;
                                    moveVector.y = 0f;
                                    moveVector.Normalize();
                                    if (instanceFieldBody.bodyFlags == CharacterBody.BodyFlags.None && (Vector3.Dot(aimDirection, moveVector) < self.GetInstanceField<float>("sprintMinAimMoveDot"))) {
                                        RT_isSprinting = false;
                                    }
                                }

                                if (HoldSprintToWalk.Value && RT_walkToggle) RT_walkToggle = false;
                                if (!RT_walkToggle) instanceFieldBodyInputs.sprint.PushState(RT_isSprinting);
                            } // End of if (instanceFieldBody)
                        } // End of if (networkUser) check
                    } // End of if (instanceFieldBodyInputs)
            }; // End of FixedUpdate
        } // End of Awake

        // CONFIGURATION
        public static ConfigFile conf;
        public static ConfigEntry<bool> PrintEntityStates { get; set; }
        public static ConfigEntry<bool> HoldSprintToWalk { get; set; }
        public static ConfigEntry<bool> DisableSprintingCrosshair { get; set; }

        private void SetupConfiguration() {

            string path = Paths.ConfigPath + "\\com.johnedwa.RTAutoSprintEx.cfg";
            conf = new ConfigFile(path, true);

            PrintEntityStates = conf.Bind<bool>(
                "0) Debug", "PrintEntityStates", true,
                new ConfigDescription("Prints all encountered EntityStates to the console. Useful for figuring out custom survivor Delay and Block names for AutoSprintAddon.",
                new AcceptableValueList<bool>(true, false)));
            HoldSprintToWalk = conf.Bind<bool>(
                "1) Movement", "HoldSprintToWalk", true,
                new ConfigDescription("Walk by holding down the sprint key. If disabled, makes the Sprint key toggle AutoSprinting functionality on and off.",
                new AcceptableValueList<bool>(true, false)));
            DisableSprintingCrosshair = conf.Bind<bool>(
                "2) Visual", "DisableSprintingCrosshair", true,
                new ConfigDescription("Disables the useless special sprinting chevron crosshair.",
                new AcceptableValueList<bool>(true, false)));
        } // End of SetupConfiguration()
    } // End of class RTAutoSprintEx
} // End of Namespace