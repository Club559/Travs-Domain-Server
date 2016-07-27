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
    class CompleteQuestPacketHandler : PacketHandlerBase<CompleteQuestPacket>
    {
        public override PacketID ID { get { return PacketID.CompleteQuest; } }

        protected override void HandlePacket(Client client, CompleteQuestPacket packet)
        {
            Quest q = client.Manager.GameData.Quests[(ushort)packet.QuestType];
            PlayerQuest finishedQuest;
            if ((finishedQuest = client.Player.GetFinishedQuest(q.QuestId)) == null)
                return;
            if (!client.Player.HandleRewards(q))
                return;
            client.Player.FinishedQuests.Remove(finishedQuest);
            client.Player.CompletedQuests.Add(client.Player.CreatePQuest(q));
            client.Player.UpdateCount++;
        }
    }
}
