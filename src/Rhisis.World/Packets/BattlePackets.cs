using Rhisis.Network;
using Rhisis.Network.Packets;
using Rhisis.World.Game.Common;
using Rhisis.World.Game.Entities;

namespace Rhisis.World.Packets
{
    public static partial class WorldPacketFactory
    {
        public static void SendAddDamage(IPlayerEntity player, ILivingEntity defender, ILivingEntity attacker, AttackFlags attackFlags, int damage)
        {
            using (var packet = new FFPacket())
            {
                packet.StartNewMergedPacket(defender.Id, SnapshotType.DAMAGE);
                packet.Write(attacker.Id);
                packet.Write(damage);
                packet.Write((int)attackFlags);

                if(attackFlags.HasFlag(AttackFlags.AF_FLYING))
                {
                    packet.Write(defender.MovableComponent.DestinationPosition.X);
                    packet.Write(defender.MovableComponent.DestinationPosition.Y);
                    packet.Write(defender.MovableComponent.DestinationPosition.Z);
                    packet.Write(defender.Object.Angle);
                }

                player.Connection.Send(packet);
                SendToVisible(packet, player);
            }
        }

        public static void SendMeleeAttack(IPlayerEntity player, int motion, int targetId, int unknwonParam, int attackFlags)
        {
            using (var packet = new FFPacket())
            {
                packet.StartNewMergedPacket(player.Id, SnapshotType.MELEE_ATTACK);
                packet.Write(motion);
                packet.Write(targetId);
                packet.Write(unknwonParam);
                packet.Write(attackFlags);

                SendToVisible(packet, player);
            }
        }
    }
}