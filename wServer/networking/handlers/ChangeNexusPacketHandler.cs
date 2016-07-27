using wServer.networking.cliPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class ChangeNexusPacketHandler : PacketHandlerBase<ChangeNexusPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.ChangeNexus; }
        }

        protected override void HandlePacket(Client client, ChangeNexusPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player));
        }

        private void Handle(Player player)
        {
            if (player.Owner == null) return;

            player.Nexus = player.Owner.Name;
            player.SendInfo("Successfully changed nexus location");
        }
    }
}