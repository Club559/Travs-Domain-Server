using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.networking.cliPackets;
using wServer.realm;
using wServer.networking.svrPackets;
using db;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    class AcceptQuestPacketHandler : PacketHandlerBase<AcceptQuestPacket>
    {
        public override PacketID ID { get { return PacketID.AcceptQuest; } }

        protected override void HandlePacket(Client client, AcceptQuestPacket packet)
        {
            Quest q = client.Manager.GameData.Quests[(ushort)packet.QuestType];
            if (!client.Player.CanGetQuest(q))
                return;
            client.Player.ActiveQuests.Add(client.Player.CreatePQuest(q));
            client.Player.UpdateCount++;
        }
    }
}
