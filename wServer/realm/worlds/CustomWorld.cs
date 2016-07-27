using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terrain;
using wServer.realm.entities;

namespace wServer.realm.worlds
{
    public class CustomWorld : World
    {
        private List<Entity> customEntities = new List<Entity>();
        private byte[] Data;
        public CustomWorld(string name, byte[] data)
        {
            Name = name;
            Background = 0;
            Difficulty = 0;
            SetMusic("dungeon/Island");
            Data = data;
        }

        protected override void Init()
        {
            base.FromWorldMap(new MemoryStream(Data));
        }

        public override int EnterWorld(Entity entity)
        {
            var ret = base.EnterWorld(entity);
            if (!(entity is Player))
                customEntities.Add(entity);
            return ret;
        }

        public override void LeaveWorld(Entity entity)
        {
            if (!(entity is Player))
                customEntities.Remove(entity);
            base.LeaveWorld(entity);
        }

        public override void HandleMapEdit(Player player, int mode, ushort type, int x, int y)
        {
            base.HandleMapEdit(player, mode, type, x, y);
            var dat = SaveWorld();
            player.Client.Account.Home = dat;
            Manager.Data.AddPendingAction(db =>
            {
                db.SaveHome(player.Client.Account, dat);
            });
        }

        public override void EditObject(Player player, ushort type, int x, int y)
        {
            var tile = Map[x, y].Clone();
            if (type != 0 && tile.ObjType != 0)
                return;
            if (type != 0 && !Manager.GameData.ObjectDescs[type].Static)
            {
                if (tile.ObjType != 0)
                    tile.ObjType = 0;
                Entity entity = Entity.Resolve(Manager, type);
                entity.Move(x + 0.5f, y + 0.5f);
                EnterWorld(entity);
                return;
            }
            else if (type != 0)
            {
                tile.ObjType = type;
                tile.ObjId = GetNextEntityId();
            }
            else
            {
                Entity entity = GetObjAt(x, y);
                if (entity != null)
                    LeaveWorld(entity);
                else
                    tile.ObjType = 0;
            }
            Map[x, y] = tile;
        }

        public byte[] SaveWorld()
        {
            var tiles = new TerrainTile[Map.Width, Map.Height];
            for(int y = 0; y < Map.Height; y++)
                for(int x = 0; x < Map.Width; x++)
                {
                    var mTile = Map[x, y];
                    tiles[x, y] = new TerrainTile
                    {
                        TileId = mTile.TileId,
                        TileObj = GetObjIdAt(x, y),
                        Name = mTile.Name,
                        Terrain = (TerrainType)Enum.Parse(typeof(TerrainType), mTile.Terrain.ToString()),
                        Region = (TileRegion)Enum.Parse(typeof(TileRegion), mTile.Region.ToString())
                    };
                }
            return WorldMapExporter.Export(tiles);
        }

        private string GetObjIdAt(int x, int y)
        {
            if (Map[x, y].ObjType != 0)
                return Manager.GameData.ObjectTypeToId[Map[x, y].ObjType];
            return GetObjAt(x, y)?.ObjectDesc.ObjectId;
        }

        private Entity GetObjAt(int x, int y)
        {
            foreach (var i in customEntities)
                if ((int)Math.Floor(i.X) == x && (int)Math.Floor(i.Y) == y)
                    return i;
            return null;
        }

        public static CustomWorld Home(Player player)
        {
            var acc = player.Client.Account;
            var world = new CustomWorld(acc.Name + "'s Home", null);
            if (acc.Home == null)
            {
                using (var stream = typeof(RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.defaultHome.wmap"))
                {
                    world.Data = new byte[stream.Length];
                    stream.Read(world.Data, 0, world.Data.Length);
                }
                acc.Home = world.Data;
                player.Manager.Data.AddPendingAction(db =>
                {
                    db.CreateHome(acc, world.Data);
                });
            }
            else
            {
                world.Data = acc.Home;
            }
            return world;
        }
    }
}
