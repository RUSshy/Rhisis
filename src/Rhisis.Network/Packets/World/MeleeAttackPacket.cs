using Ether.Network.Packets;
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

<<<<<<< HEAD
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
=======
        public int AttackFlags { get; }

>>>>>>> Add melee attack miss flag
        public float WeaponAttackSpeed { get; }

        public MeleeAttackPacket(INetPacketStream packet)
        {
<<<<<<< HEAD
            this.AttackMessage = (ObjectMessageType)packet.Read<uint>();
            this.ObjectId = packet.Read<uint>();
            this.Parameter2 = packet.Read<int>(); // Always 0
            this.Parameter3 = packet.Read<int>(); // Possibly error number returned from client
=======
            this.AttackMessage = packet.Read<int>();
            this.ObjectId = packet.Read<int>();
            packet.Read<int>(); // Always 0; don't need to store it
            this.AttackFlags = packet.Read<int>() & 0xFFFF; // Attack flags ?!
>>>>>>> Add melee attack miss flag
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
