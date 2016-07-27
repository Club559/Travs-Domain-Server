namespace wServer.networking.cliPackets
{
    public class ChangeNexusPacket : ClientPacket
    {
        public override PacketID ID
        {
            get { return PacketID.ChangeNexus; }
        }

        public override Packet CreateInstance()
        {
            return new ChangeNexusPacket();
        }

        protected override void Read(NReader rdr) {}

        protected override void Write(NWriter wtr) {}
    }
}