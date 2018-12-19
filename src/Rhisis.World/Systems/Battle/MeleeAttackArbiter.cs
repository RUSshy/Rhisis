﻿using Rhisis.Core.Data;
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

            int attackMin = 0;
            int attackMax = 0;

            if (this._attacker is IPlayerEntity player)
            {
                Item rightWeapon = player.Inventory.GetItem(x => x.Slot == InventorySystem.RightWeaponSlot);

                if (rightWeapon == null)
                    rightWeapon = InventorySystem.Hand;

                // TODO: GetDamagePropertyFactor()
                int weaponAttack = BattleHelper.GetWeaponAttackDamages(rightWeapon.Data.WeaponType, player);
                attackMin = rightWeapon.Data.AbilityMin * 2 + weaponAttack;
                attackMax = rightWeapon.Data.AbilityMax * 2 + weaponAttack;
            }
            else if (this._attacker is IMonsterEntity monster)
            {
                attackMin = monster.Data.AttackMin;
                attackMax = monster.Data.AttackMax;
            }

            if (this.IsCriticalAttack(this._attacker, attackResult.Flags))
            {
                attackResult.Flags |= AttackFlags.AF_CRITICAL;
                this.CalculateCriticalDamages(ref attackMin, ref attackMax);

                if (this.IsKnockback(attackResult.Flags))
                    attackResult.Flags |= AttackFlags.AF_FLYING;
            }

            attackResult.Damages = RandomHelper.Random(attackMin, attackMax);
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

        /// <summary>
        /// Check if the attacker's melee attack is a critical hit.
        /// </summary>
        /// <param name="attacker">Attacker</param>
        /// <param name="currentAttackFlags">Attack flags</param>
        /// <returns></returns>
        public bool IsCriticalAttack(ILivingEntity attacker, AttackFlags currentAttackFlags)
        {
            if (currentAttackFlags.HasFlag(AttackFlags.AF_MELEESKILL) || currentAttackFlags.HasFlag(AttackFlags.AF_MAGICSKILL))
                return false;

            float criticalJobFactor = attacker is IPlayerEntity player ? player.PlayerData.JobData.Critical : 1f;
            int criticalProbability = (int)((attacker.Statistics.Dexterity / 10) * criticalJobFactor);
            // TODO: add DST_CHR_CHANCECRITICAL to criticalProbability

            if (criticalProbability < 0)
                criticalProbability = 0;

            // TODO: check if player is in party and if it has the MVRF_CRITICAL flag

            return RandomHelper.Random(0, 100) < criticalProbability;
        }

        public void CalculateCriticalDamages(ref int attackMin, ref int attackMax)
        {
            float criticalMin = 1.1f;
            float criticalMax = 1.4f;

            if (this._attacker.Object.Level > this._defender.Object.Level)
            {
                if (this._defender.Type == WorldEntityType.Monster)
                {
                    criticalMin = 1.2f;
                    criticalMax = 2.0f;
                }
                else
                {
                    criticalMin = 1.4f;
                    criticalMax = 1.8f;
                }
            }

            float criticalBonus = 1; // TODO: 1 + (DST_CRITICAL_BONUS / 100)

            if (criticalBonus < 0.1f)
                criticalBonus = 0.1f;

            attackMin = (int)(attackMin * criticalMin * criticalBonus);
            attackMax = (int)(attackMax * criticalMax * criticalBonus);
        }

        public bool IsKnockback(AttackFlags attackerAttackFlags)
        {
            bool knockbackChance = RandomHelper.Random(0, 100) < 15;

            if (this._defender.Type == WorldEntityType.Player)
                return false;

            if (this._attacker is IPlayerEntity player)
            {
                var weapon = player.Inventory[InventorySystem.RightWeaponSlot];

                if (weapon == null)
                    weapon = InventorySystem.Hand;
                if (weapon.Data.WeaponType == WeaponType.MELEE_YOYO || attackerAttackFlags.HasFlag(AttackFlags.AF_FORCE))
                    return false;
            }

            bool canFly = false;

            // TODO: if is flying, return false
            if ((this._defender.Object.MovingFlags & ObjectState.OBJSTA_DMG_FLY_ALL) == 0 && this._defender is IMonsterEntity monster)
            {
                canFly = monster.Data.Class != MoverClassType.RANK_SUPER && 
                    monster.Data.Class != MoverClassType.RANK_MATERIAL && 
                    monster.Data.Class != MoverClassType.RANK_MIDBOSS;
            }

            return canFly && knockbackChance;
        }
    }
}
