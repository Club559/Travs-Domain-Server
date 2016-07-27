using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.networking.cliPackets
{
    public class AcceptQuestPacket : ClientPacket
    {
        public int QuestType { get; set; }

        public override PacketID ID { get { return PacketID.AcceptQuest; } }
        public override Packet CreateInstance() { return new AcceptQuestPacket(); }

        protected override void Read(NReader rdr)
        {
            QuestType = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(QuestType);
        }
    }
}
