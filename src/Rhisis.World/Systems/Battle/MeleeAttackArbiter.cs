using Rhisis.Core.Helpers;
using Rhisis.World.Game.Common;
using Rhisis.World.Game.Core;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;
using Rhisis.World.Systems.Inventory;

namespace Rhisis.World.Systems.Battle
{
    /// <summary>
    /// Provides a mechanism to calculate a melee attack result based on the attacker and defender statistics.
    /// </summary>
    public class MeleeAttackArbiter
    {
        public const int MinimalHitRate = 20;
        public const int MaximalHitRate = 96;
        private readonly ILivingEntity _attacker;
        private readonly ILivingEntity _defender;

        /// <summary>
        /// Creates a new <see cref="MeleeAttackArbiter"/> instance.
        /// </summary>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="defender">Defender entity</param>
        public MeleeAttackArbiter(ILivingEntity attacker, ILivingEntity defender)
        {
            this._attacker = attacker;
            this._defender = defender;
        }

        /// <summary>
        /// Gets the melee damages inflicted by an attacker to a defender.
        /// </summary>
        /// <returns><see cref="AttackResult"/></returns>
        public AttackResult OnDamage()
        {
            var attackResult = new AttackResult
            {
                Flags = this.GetAttackFlags()
            };
            
            if (attackResult.Flags.HasFlag(AttackFlags.AF_MISS))
                return attackResult;

            if (this._attacker is IPlayerEntity player)
            {
                Item rightWeapon = player.Inventory.GetItem(x => x.Slot == InventorySystem.RightWeaponSlot);

                if (rightWeapon == null)
                    rightWeapon = InventorySystem.Hand;

                // TODO: GetDamagePropertyFactor()
                int weaponAttack = BattleHelper.GetWeaponAttackDamages(rightWeapon.Data.WeaponType, player);
                int weaponMinAbility = rightWeapon.Data.AbilityMin * 2 + weaponAttack;
                int weaponMaxAbility = rightWeapon.Data.AbilityMax * 2 + weaponAttack;

                attackResult.Damages = RandomHelper.Random(weaponMinAbility, weaponMaxAbility);
            }
            else if (this._attacker is IMonsterEntity monster)
            {
                attackResult.Damages = RandomHelper.Random(monster.Data.AttackMin, monster.Data.AttackMax);
            }

            if (attackResult.Damages < 0)
                attackResult.Damages = 0;

            return attackResult;
        }

        /// <summary>
        /// Gets the <see cref="AttackFlags"/> of this melee attack.
        /// </summary>
        /// <returns></returns>
        private AttackFlags GetAttackFlags()
        {
            // TODO: if attacker mode == ONEKILL_MODE, return AF_GENERIC

            int hitRate = 0;
            int hitRating = this.GetHitRating(this._attacker);
            int escapeRating = this.GetEspaceRating(this._defender);

            if (this._attacker.Type == WorldEntityType.Player && this._defender.Type == WorldEntityType.Monster)
            {
                // Player VS Monster
                hitRate = (int)(((hitRating * 1.6f) / (hitRating + escapeRating)) * 1.5f *
                           (this._attacker.Object.Level * 1.2f / (this._attacker.Object.Level + this._defender.Object.Level)) * 100.0f);
            }
            else if (this._attacker.Type == WorldEntityType.Monster && this._defender.Type == WorldEntityType.Player)
            {
                // Monster VS Player
                hitRate = (int)(((hitRating * 1.5f) / (hitRating + escapeRating)) * 2.0f *
                          (this._attacker.Object.Level * 0.5f / (this._attacker.Object.Level + this._defender.Object.Level * 0.3f)) * 100.0f);
            }
            else 
            {
                // Player VS Player
                hitRate = (int)(((hitRating * 1.6f) / (hitRating + escapeRating)) * 1.5f *
                          (this._attacker.Object.Level * 1.2f / (this._attacker.Object.Level + this._defender.Object.Level)) * 100.0f);
            }

            hitRate = MathHelper.Clamp(hitRate, MinimalHitRate, MaximalHitRate);

            return RandomHelper.Random(0, 100) < hitRate ? AttackFlags.AF_GENERIC : AttackFlags.AF_MISS;
        }

        /// <summary>
        /// Gets the hit rating of an entity.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        private int GetHitRating(ILivingEntity entity)
        {
            if (entity is IPlayerEntity player)
                return player.Statistics.Dexterity; // TODO: add dex bonus
            else if (entity is IMonsterEntity monster)
                return monster.Data.HitRating;

            return 0;
        }

        /// <summary>
        /// Gets the escape rating of an entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int GetEspaceRating(ILivingEntity entity)
        {
            if (entity is IPlayerEntity player)
                return (int)(player.Statistics.Dexterity * 0.5f); // TODO: add dex bonus and DST_PARRY
            else if (entity is IMonsterEntity monster)
                return monster.Data.EscapeRating;

            return 0;
        }
    }
}
