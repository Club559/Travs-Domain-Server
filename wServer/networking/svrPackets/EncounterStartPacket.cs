namespace wServer.networking.svrPackets
{
  public class EncounterStartPacket : ServerPacket
  {
    public string Pokemon { get; set; }

    public override PacketID ID
    {
      get { return PacketID.EncounterStart; }
    }

    public override Packet CreateInstance()
    {
      return new EncounterStartPacket();
    }

    protected override void Read(NReader rdr)
    {
      Pokemon = rdr.ReadUTF();
    }

    protected override void Write(NWriter wtr)
    {
      wtr.WriteUTF(Pokemon);
    }
  }
}