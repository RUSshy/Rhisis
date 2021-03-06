﻿using Ether.Network.Packets;
using System;

namespace Rhisis.Network.Packets.World
{
    /// <summary>
    /// Defines the <see cref="MeleeAttackPacket"/> structure.
    /// </summary>
    public struct MeleeAttackPacket : IEquatable<MeleeAttackPacket>
    {
        /// <summary>
        /// Gets the attack message.
        /// </summary>
        public ObjectMessageType AttackMessage { get; }

        /// <summary>
        /// Gets the object id.
        /// </summary>
        public uint ObjectId { get; }

        /// <summary>
        /// Gets the second parameter.
        /// </summary>
        public int Parameter2 { get; set; }

        /// <summary>
        /// Gets the third parameter.
        /// </summary>
        public int Parameter3 { get; set; }

        /// <summary>
        /// Gets the attack speed.
        /// </summary>
        public float WeaponAttackSpeed { get; }

        public MeleeAttackPacket(INetPacketStream packet)
        {
            this.AttackMessage = (ObjectMessageType)packet.Read<uint>();
            this.ObjectId = packet.Read<uint>();
            this.Parameter2 = packet.Read<int>(); // Always 0
            this.Parameter3 = packet.Read<int>(); // Possibly error number returned from client
            this.WeaponAttackSpeed = packet.Read<float>();
        }

        public bool Equals(MeleeAttackPacket other)
        {
            return this.AttackMessage == other.AttackMessage &&
                   this.ObjectId == other.ObjectId &&
                   this.Parameter2 == other.Parameter2 &&
                   this.Parameter3 == other.Parameter3 &&
                   this.WeaponAttackSpeed == other.WeaponAttackSpeed;
        }
    }
}
