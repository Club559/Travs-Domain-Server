using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm;
using wServer.realm.worlds;

namespace wServer.networking.handlers
{
    internal class EscapePacketHandler : PacketHandlerBase<EscapePacket>
    {
        public override PacketID ID
        {
            get { return PacketID.Escape; }
        }

        protected override void HandlePacket(Client client, EscapePacket packet)
        {
            if (client.Player.Owner.Name == client.Player.Nexus)
            {
                client.Player.SendInfo("You are already in your nexus!");
            }
            else if (!client.Player.Owner.AllowNexus)
            {
                if (client.Player.Owner is PVPArena && client.Player.Owner.Players.Count == 1)
                {
                    client.Player.GoToNexus();
                    return;
                }
                client.Player.SendInfo("You cannot nexus now!");
            }
            else
            {
                client.Player.GoToNexus();
            }
        }
    }
}