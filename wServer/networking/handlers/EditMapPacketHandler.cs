using Ionic.Zlib;
using System;
using wServer.networking.cliPackets;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.networking.handlers
{
    internal class EditMapPacketHandler : PacketHandlerBase<EditMapPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.EditMap; }
        }

        protected override void HandlePacket(Client client, EditMapPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet.Mode, packet.Type, packet.Position));
        }

        private void Handle(Player player, int mode, int type, Position pos)
        {
            if (player.Owner == null) return;

            var x = (int)pos.X;
            var y = (int)pos.Y;
            player.Owner.HandleMapEdit(player, mode, (ushort)type, x, y);
            //var tile = player.Owner.Map[x, y].Clone();
            //switch (mode)
            //{
            //    case 0: //Tiles
            //        tile.TileId = (ushort)type;
            //        break;
            //    case 1: //Objects
            //        if (tile.ObjType == 0 || type == 0)
            //        {
            //            tile.ObjType = (ushort)type;
            //            if (type != 0)
            //                tile.ObjId = player.Owner.GetNextEntityId();
            //        }
            //        break;
            //}
            //player.Owner.Map[x, y] = tile;
            //if (player.Owner is CustomWorld)
            //{
            //    var dat = (player.Owner as CustomWorld).SaveWorld();
            //    player.Client.Account.Home = dat;
            //    player.Manager.Data.AddPendingAction(db =>
            //    {
            //        db.SaveHome(player.Client.Account, dat);
            //    });
            //}
        }
    }
}