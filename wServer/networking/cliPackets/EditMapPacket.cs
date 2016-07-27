namespace wServer.networking.cliPackets
{
    public class EditMapPacket : ClientPacket
    {
        public int Mode { get; set; }
        public int Type { get; set; }
        public Position Position { get; set; }

        public override PacketID ID
        {
            get { return PacketID.EditMap; }
        }

        public override Packet CreateInstance()
        {
            return new EditMapPacket();
        }

        protected override void Read(NReader rdr)
        {
            Mode = rdr.ReadInt32();
            Type = rdr.ReadInt32();
            Position = Position.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Mode);
            wtr.Write(Type);
            Position.Write(wtr);
        }
    }
}