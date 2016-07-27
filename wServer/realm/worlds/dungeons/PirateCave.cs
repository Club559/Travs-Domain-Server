using wServer.networking;

namespace wServer.realm.worlds
{
    public class PirateCave : World
    {
        public PirateCave()
        {
            Id = PVP;
            Name = "Pirate Cave";
            Background = 0;
            Difficulty = 1;
            SetMusic("dungeon/Abyss of Demons");
        }

        protected override void Init()
        {
            base.FromWorldMap(
                typeof(RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.dungeons.pirateCave.wmap"));
        }

        public override World GetInstance(Client client)
        {
            return Manager.AddWorld(new PVPArena());
        }
    }
}