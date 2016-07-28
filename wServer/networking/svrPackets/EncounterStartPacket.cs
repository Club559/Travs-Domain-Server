namespace wServer.networking.svrPackets
{
  public class EncounterStartPacket : ServerPacket
  {
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

    }

    protected override void Write(NWriter wtr)
    {

    }
  }
}