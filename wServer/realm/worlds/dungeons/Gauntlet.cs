﻿using wServer.networking;

namespace wServer.realm.worlds
{
    public class Gauntlet : World
    {
        public Gauntlet()
        {
            Id = PVP;
            Name = "The Gauntlet";
            Background = 0;
            Difficulty = 1;
            SetMusic("dungeon/Haunted Cemetary");
        }

        protected override void Init()
        {
            base.FromWorldMap(
                typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.dungeons.gauntlet.wmap"));
        }

        public override World GetInstance(Client client)
        {
            return Manager.AddWorld(new PVPArena());
        }
    }
}