using System.Reflection;
using GlobalEnums;
using Modding;
using UnityEngine;

namespace BetterShadowDash
{
    /// <summary>
    /// The main mod class
    /// </summary>
    /// <remarks>Makes shadow dash have the same cooldown as normal dash.</remarks>
    public class BetterShadowDash : Mod
    {
        // Private vars
        private float _defaultShadowDashCD = 0f;
        private int hitCount = 0;
        // private bool _wasDashingLastUpdate;
        /// <summary>
        /// Represents this Mod's instance.
        /// </summary>
        internal static BetterShadowDash Instance;

        /// <summary>
        /// Fetches the Mod Version From AssemblyInfo.AssemblyVersion
        /// </summary>
        /// <returns>Mod's Version</returns>
        public override string GetVersion() => "1.0.13";

        /// <summary>
        /// Called after the class has been constructed.
        /// </summary>
        public override void Initialize()
        {
            //Assign the Instance to the instantiated mod.
            Instance = this;

            Log("Initializing");
            ModHooks.Instance.DashPressedHook += this.OnDash;
            On.HealthManager.TakeDamage += this.EnemyTookDamage;
            
            Log("Initialized");
        }

        public void EnemyTookDamage(On.HealthManager.orig_TakeDamage orig, global::HealthManager self, global::HitInstance hit)
        {
            if (hit.AttackType == AttackTypes.SharpShadow && global::PlayerData.instance.equippedCharm_16  && global::PlayerData.instance.gotShadeCharm)
            {
                this.hitCount++;  //Counter for healing
                //Only heal with sharp shadow while at full MP
                if (atMaxMP())  
                {
                    HeroController.instance.AddHealth(this.hitCount >= 2 ? 1 : 0);  //Heal every 2 hits with sharp shadow.
                    this.hitCount = 0;
                }
                else
                {
                    HeroController.instance.AddMPCharge(16);  //Affected by soul catcher?
                }
                HeroController.instance.ResetAirMoves();  //Reset air dash when hitting enemy with sharp shadow.
                //TODO make sharp shadow cost more?
            }
            orig(self, hit);
        }

        public bool atMaxMP() {
            return PlayerData.instance.GetInt("MPCharge") >= PlayerData.instance.GetInt("maxMP") &&
                       PlayerData.instance.GetInt("MPReserve") >= PlayerData.instance.GetInt("MPReserveMax");
        }

        public void OnDoAttack() {
            HeroController.instance.fsm_thornCounter.SendEvent("THORN COUNTER");
        }

        /// <summary>
        /// Changes default shadow dash CD to be the same as normal dash, depending on equipped dashmaster charm
        /// </summary>
        /// <remarks>
        /// Saves previous sdcd for if we need to restore it.
        /// </remarks>
        /// <param name="dir"></param>
        public bool OnDash() {

            LogDebug("Dashing");
            // Actually, this whole thing could probably just be done once, on start/awake save load?.
            if (_defaultShadowDashCD == 0) {
                _defaultShadowDashCD = HeroController.instance.SHADOW_DASH_COOLDOWN;
            LogDebug("Saved SDC as " + _defaultShadowDashCD);
            }
            if (atMaxMP())
            {
                if (PlayerData.instance.equippedCharm_31)
                {
                    LogDebug("MaxMPdash " + HeroController.instance.DASH_COOLDOWN_CH);
                    HeroController.instance.SHADOW_DASH_COOLDOWN = HeroController.instance.DASH_COOLDOWN_CH;
                }
                else
                {
                    HeroController.instance.SHADOW_DASH_COOLDOWN = HeroController.instance.DASH_COOLDOWN;
                }
            }
            else {
                LogDebug("Resetting SDC to " + _defaultShadowDashCD);
                HeroController.instance.SHADOW_DASH_COOLDOWN = _defaultShadowDashCD;
            }
            //TODO remove shadow dash reset anims.
            return false; //Continue to do normal dash routine.
        }
    }

}
