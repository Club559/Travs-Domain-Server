using System;
using System.Globalization;
using System.Linq;
using System.Text;
using db;
using wServer.networking;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.setpieces;

namespace wServer.realm.commands
{
  internal class SpawnCommand : Command
  {
    public SpawnCommand() : base("spawn", desc: "spawns the selected amount of the specified object", usage: "[amount] <object name>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var index = args.IndexOf(' ');
      int num;
      var name = args;
      string extras = "";

      if (name.Split('%').Length > 1)
      {
        extras = name.Split('%')[1].Trim();
        name = name.Split('%')[0].Trim();
      }

      if (args.IndexOf(' ') > 0 && int.TryParse(args.Substring(0, args.IndexOf(' ')), out num)) //multi
        name = args.Substring(index + 1);
      else
        num = 1;


      ushort objType;
      if (!player.Manager.GameData.IdToObjectType.TryGetValue(name, out objType) ||
          !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
      {
        player.SendError("Unknown entity!");
        return false;
      }

      for (var i = 0; i < num; i++)
      {
        var entity = Entity.Resolve(player.Manager, objType);
        entity.Move(player.X, player.Y);
        foreach (string item in extras.Split(';'))
        {
          string[] kv = item.Split(':');
          switch (kv[0])
          {
            case "name":
              entity.Name = kv[1];
              break;
            case "size":
              entity.Size = Utils.FromString(kv[1]);
              break;
            case "eff":
              entity.ConditionEffects = (ConditionEffects)Utils.FromString(kv[1]);
              break;
            case "conn":
              (entity as ConnectedObject).Connection =
                  ConnectionInfo.Infos[(uint)Utils.FromString(kv[1])];
              break;
            case "mtype":
              (entity as Merchants).Custom = true;
              (entity as Merchants).MType = Utils.FromString(kv[1]);
              break;
            case "mcount":
              (entity as Merchants).MRemaining = Utils.FromString(kv[1]);
              break;
            case "mtime":
              (entity as Merchants).MTime = Utils.FromString(kv[1]);
              break;
            case "mcost":
              (entity as SellableObject).Price = Utils.FromString(kv[1]);
              break;
            case "mcur":
              (entity as SellableObject).Currency = (CurrencyType)Utils.FromString(kv[1]);
              break;
            case "stars":
              (entity as SellableObject).RankReq = Utils.FromString(kv[1]);
              break;
              //case "nstar":
              //    entity.Stats[StatsType.NameChangerStar] = Utils.FromString(kv[1]); break;
          }
        }
        player.Owner.EnterWorld(entity);
      }
      if (num > 1)
        if (!args.ToLower().EndsWith("s"))
          player.SendInfo("Sucessfully spawned " + num + " : " +
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                              args.Substring(index + 1).ToLower() + "s"));
        else
          player.SendInfo("Sucessfully spawned " + num + " : " +
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                              args.Substring(index + 1).ToLower() + "'"));
      else
        player.SendInfo("Sucessfully spawned " + num + " : " +
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(args.ToLower()));
      return true;
    }
  }

  internal class ToggleEffCommand : Command
  {
    public ToggleEffCommand() : base("eff", desc: "toggles the specified effect on your character", usage: "<effect name>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      ConditionEffectIndex effect;
      if (!Enum.TryParse(args, true, out effect))
      {
        player.SendError("Invalid effect!");
        return false;
      }
      if ((player.ConditionEffects & (ConditionEffects)(1 << (int)effect)) != 0)
      {
        //remove
        player.ApplyConditionEffect(new ConditionEffect
        {
          Effect = effect,
          DurationMS = 0
        });
        player.SendInfo("Sucessfully removed effect : " + args);
      }
      else
      {
        //add
        player.ApplyConditionEffect(new ConditionEffect
        {
          Effect = effect,
          DurationMS = -1
        });
        player.SendInfo("Sucessfully added effect : " + args);
      }
      return true;
    }
  }

  internal class GiveCommand : Command
  {
    public GiveCommand() : base("give", desc: "adds the specified item to your inventory", usage: "<item name> [item data]", permLevel: 2)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var item = args;
      var data = "";
      if (args.IndexOf("{", StringComparison.Ordinal) >= 0 && args.EndsWith("}"))
      {
        item = args.Remove(args.IndexOf("{", StringComparison.Ordinal)).TrimEnd();
        data = args.Substring(args.IndexOf("{", StringComparison.Ordinal));
      }
      ushort objType;
      if (!player.Manager.GameData.IdToObjectType.TryGetValue(item, out objType))
      {
        player.SendError("Unknown item type!");
        return false;
      }
      for (var i = 0; i < player.Inventory.Length; i++)
        if (player.Inventory[i] == null)
        {
          player.Inventory[i] = player.Manager.GameData.Items[objType];
          if (data != "")
            player.Inventory.Data[i] = ItemData.CreateData(data);
          player.UpdateCount++;
          return true;
        }
      player.SendError("Not enough space in inventory!");
      return false;
    }
  }

  internal class TpPosCommand : Command
  {
    public TpPosCommand() : base("tpPos", desc: "teleports you to the X and Y location specified", usage: "<x> <y>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      string[] coordinates = args.Split(' ');
      if (coordinates.Length != 2)
      {
        player.SendError("Invalid coordinates!");
        return false;
      }

      int x, y;
      if (!int.TryParse(coordinates[0], out x) ||
          !int.TryParse(coordinates[1], out y))
      {
        player.SendError("Invalid coordinates!");
        return false;
      }

      player.Move(x + 0.5f, y + 0.5f);
      player.SetNewbiePeriod();
      player.UpdateCount++;
      player.Owner.BroadcastPacket(new GotoPacket
      {
        ObjectId = player.Id,
        Position = new Position
        {
          X = player.X,
          Y = player.Y
        }
      }, null);
      return true;
    }
  }

  internal class SetpieceCommand : Command
  {
    public SetpieceCommand() : base("setpiece", desc: "generates a structure of selected type", usage: "<structure type>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var piece = (ISetPiece)Activator.CreateInstance(Type.GetType(
          "wServer.realm.setpieces." + args, true, true));
      piece.RenderSetPiece(player.Owner, new IntPoint((int)player.X + 1, (int)player.Y + 1));
      return true;
    }
  }

  internal class DebugCommand : Command
  {
    public DebugCommand() : base("debug", desc: "test command that usually don't stay the same", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      switch (args)
      {
        case "locater":
          player.Owner.EnterWorld(new Locater(player));
          return true;
        case "notif":
          player.Client.SendPacket(new GlobalNotificationPacket
          {
            Text = "{\"type\": \"notif\", \"title\": \"Notif Box\", \"text\": \"Notification box that works as it should...\"}"
          });
          return true;
        default:
          player.Client.SendPacket(new GlobalNotificationPacket
          {
            Text = "{\"type\": \"notif\", \"title\": \"Notif Box\", \"text\": \"Notification box that works as it should...\"}"
          });
          return true;
      }
    }

    private class Locater : Enemy
    {
      private readonly Player _player;

      public Locater(Player player)
          : base(player.Manager, 0x0d5d)
      {
        _player = player;
        Move(player.X, player.Y);
        ApplyConditionEffect(new ConditionEffect
        {
          Effect = ConditionEffectIndex.Invincible,
          DurationMS = -1
        });
      }

      public override void Tick(RealmTime time)
      {
        Move(_player.X, _player.Y);
        UpdateCount++;
        base.Tick(time);
      }
    }
  }

  internal class AllOnlineCommand : Command
  {
    public AllOnlineCommand() : base("online", desc: "displays the name, location and IP of all online users", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var sb = new StringBuilder("Users online: \r\n");
      foreach (Client i in player.Manager.Clients.Values)
      {
        if (i.Stage == ProtocalStage.Disconnected || i.Player == null || i.Player.Owner == null) continue;
        sb.AppendFormat("{0}#{1}@{2}\r\n",
            i.Account.Name,
            i.Player.Owner.Name,
            i.Socket.RemoteEndPoint);
      }

      player.SendInfo(sb.ToString());
      return true;
    }
  }

  internal class KillAllCommand : Command
  {
    public KillAllCommand() : base("killAll", desc: "kills all of the targeted enemies", usage: "<enemy name>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      int count = 0;
      foreach (var i in player.Owner.Enemies)
      {
        ObjectDesc desc = i.Value.ObjectDesc;
        if (desc != null &&
            desc.ObjectId != null &&
            desc.ObjectId.ContainsIgnoreCase(args))
        {
          i.Value.Death(time);
          count++;
        }
      }
      player.SendInfo(string.Format("{0} enemy killed!", count));
      return true;
    }
  }

  internal class KillAllXCommand : Command
  {
    public KillAllXCommand() : base("killAllX", desc: "kills all targeted enemies and grants the experience", usage: "<enemy name>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      int count = 0;
      foreach (var i in player.Owner.Enemies)
      {
        ObjectDesc desc = i.Value.ObjectDesc;
        if (desc != null &&
            desc.ObjectId != null &&
            desc.ObjectId.ContainsIgnoreCase(args))
        {
          i.Value.Damage(player, time, i.Value.HP + 1, true, false, null);
          count++;
        }
      }
      player.SendInfo(string.Format("{0} enemy killed!", count));
      return true;
    }
  }

  internal class KickCommand : Command
  {
    public KickCommand() : base("kick", desc: "disconnects the targeted player", usage: "<player name>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (Client i in player.Manager.Clients.Values)
      {
        if (i.Account.Name.EqualsIgnoreCase(args))
        {
          i.Disconnect();
          i.Save();
          player.SendInfo("Player disconnected!");
          return true;
        }
      }
      player.SendError(string.Format("Player '{0}' could not be found!", args));
      return false;
    }
  }

  internal class GetQuestCommand : Command
  {
    public GetQuestCommand() : base("getQuest", desc: "shows the X and Y location of your next quest", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      if (player.Quest == null)
      {
        player.SendError("Player does not have a quest!");
        return false;
      }
      player.SendInfo("Quest location: (" + player.Quest.X + ", " + player.Quest.Y + ")");
      return true;
    }
  }

  internal class OryxSayCommand : Command
  {
    public OryxSayCommand() : base("oryxSay", desc: "displays a message using Oryx the Mad Gods name", usage: "<message>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.Manager.Chat.Oryx(player.Owner, args);
      return true;
    }
  }

  internal class AnnounceCommand : Command
  {
    public AnnounceCommand() : base("announce", desc: "displays a message to all online players", usage: "<message>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.Manager.Chat.Announce(args);
      return true;
    }
  }

  internal class SummonCommand : Command
  {
    public SummonCommand() : base("summon", desc: "summons the targeted player to your location", usage: "<player name>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (var i in player.Owner.Players)
      {
        if (i.Value.Name.EqualsIgnoreCase(args))
        {
          i.Value.Teleport(time, player.Id);
          player.SendInfo("Player summoned!");
          return true;
        }
      }
      player.SendError(string.Format("Player '{0}' could not be found!", args));
      return false;
    }
  }

  internal class KillPlayerCommand : Command
  {
    public KillPlayerCommand() : base("killPlayer", desc: "kills the targeted player", usage: "<player name>", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (Client i in player.Manager.Clients.Values)
      {
        if (i.Account.Name.EqualsIgnoreCase(args))
        {
          i.Player.HP = 0;
          i.Player.Death("Moderator");
          player.SendInfo("Player killed!");
          return true;
        }
      }
      player.SendError(string.Format("Player '{0}' could not be found!", args));
      return false;
    }
  }

  internal class VanishCommand : Command
  {
    public VanishCommand() : base("vanish", desc: "hides from all players", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      if (!player.isNotVisible)
      {
        player.isNotVisible = true;
        player.Owner.PlayersCollision.Remove(player);
        if (player.Pet != null)
          player.Owner.LeaveWorld(player.Pet);
        player.SendInfo("You're now hidden from all players!");
        return true;
      }
      player.isNotVisible = false;

      player.SendInfo("You're now visible to all players!");
      return true;
    }
  }

  internal class SayCommand : Command
  {
    public SayCommand() : base("say", desc: "displays a notification over your characters head", usage: "<message>", permLevel: 1)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new NotificationPacket
        {
          Color = new ARGB(0xff00ff00),
          ObjectId = player.Id,
          Text = args
        });
      return true;
    }
  }

  internal class SaveCommand : Command
  {
    public SaveCommand() : base("save", desc: "saves all active players in the server", permLevel: 4)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (Client i in player.Manager.Clients.Values)
        i.Save();

      player.SendInfo("Saved all Clients!");
      return true;
    }
  }

  internal class DevChatCommand : Command
  {
    public DevChatCommand() : base("d", desc: "writes a message in the developer chat", usage: "<message>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      foreach (Client client in player.Manager.Clients.Values)
        if (client.Account.Rank > 3)
          client.Player.SendText("@[DEV] - " + player.Name + "", args);
      return true;
    }
  }

  internal class PvpArenaCommand : Command
  {
    public PvpArenaCommand() : base("pvparena", desc: "spawns a PvP Arena Portal", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      Entity prtal = Entity.Resolve(player.Manager, "PVP Portal");
      prtal.Move(player.X, player.Y);
      player.Owner.EnterWorld(prtal);
      World w = player.Manager.GetWorld(player.Owner.Id);
      w.Timers.Add(new WorldTimer(30 * 1000, (world, t) => //default portal close time * 1000
      {
        try
        {
          w.LeaveWorld(prtal);
        }
        catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
        {
          Console.Out.WriteLine("Couldn't despawn portal.");
        }
      }));
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new TextPacket
        {
          BubbleTime = 0,
          Stars = -1,
          Name = "",
          Text = "PVP Arena Opened by:" + " " + player.Name
        });
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new NotificationPacket
        {
          Color = new ARGB(0xff00ff00),
          ObjectId = player.Id,
          Text = "PVP Arena Opened by " + player.Name
        });
      return true;
    }
  }

  internal class DuelArenaCommand : Command
  {
    public DuelArenaCommand() : base("duelarena", desc: "spawns a Duel Arena Portal", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      Entity prtal = Entity.Resolve(player.Manager, "Duel Portal");
      prtal.Move(player.X, player.Y);
      player.Owner.EnterWorld(prtal);
      World w = player.Manager.GetWorld(player.Owner.Id);
      w.Timers.Add(new WorldTimer(30 * 1000, (world, t) => //default portal close time * 1000
      {
        try
        {
          w.LeaveWorld(prtal);
        }
        catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
        {
          Console.Out.WriteLine("Couldn't despawn portal.");
        }
      }));
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new TextPacket
        {
          BubbleTime = 0,
          Stars = -1,
          Name = "",
          Text = "Duel Arena Opened by:" + " " + player.Name
        });
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new NotificationPacket
        {
          Color = new ARGB(0xff00ff00),
          ObjectId = player.Id,
          Text = "Duel Arena Opened by " + player.Name
        });
      return true;
    }
  }

  internal class TestingAndStuffCommand : Command
  {
    public TestingAndStuffCommand() : base("testingandstuff", desc: "spawns a Testing and Stuff Portal", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      Entity prtal = Entity.Resolve(player.Manager, "Testing and Stuff");
      prtal.Move(player.X, player.Y);
      player.Owner.EnterWorld(prtal);
      World w = player.Manager.GetWorld(player.Owner.Id);
      w.Timers.Add(new WorldTimer(30 * 1000, (world, t) => //default portal close time * 1000
      {
        try
        {
          w.LeaveWorld(prtal);
        }
        catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
        {
          Console.Out.WriteLine("Couldn't despawn portal.");
        }
      }));
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new TextPacket
        {
          BubbleTime = 0,
          Stars = -1,
          Name = "",
          Text = "Testing & Stuff Opened by:" + " " + player.Name
        });
      foreach (Client i in player.Manager.Clients.Values)
        i.SendPacket(new NotificationPacket
        {
          Color = new ARGB(0xff00ff00),
          ObjectId = player.Id,
          Text = "Testing & Stuff Opened by " + player.Name
        });
      return true;
    }
  }

  internal class StatCommand : Command
  {
    public StatCommand() : base("stats", desc: "sets the selected amount to the specified stat", usage: "<stat name> <amount>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      int index = args.IndexOf(' ');
      int num;
      string stat = args;

      if (args.IndexOf(' ') > 0 && int.TryParse(args.Substring(index), out num))
        stat = args.Substring(0, args.IndexOf(' '));
      else
        num = 1;

      switch (stat)
      {
        case "hp":
          player.Stats[0] = num;
          player.UpdateCount++;
          break;
        case "mp":
          player.Stats[1] = num;
          player.UpdateCount++;
          break;
        case "att":
          player.Stats[2] = num;
          player.UpdateCount++;
          break;
        case "def":
          player.Stats[3] = num;
          player.UpdateCount++;
          break;
        case "spd":
          player.Stats[4] = num;
          player.UpdateCount++;
          break;
        case "vit":
          player.Stats[5] = num;
          player.UpdateCount++;
          break;
        case "wis":
          player.Stats[6] = num;
          player.UpdateCount++;
          break;
        case "dex":
          player.Stats[7] = num;
          player.UpdateCount++;
          break;
        case "all":
          player.Stats[2] = num;
          player.Stats[3] = num;
          player.Stats[4] = num;
          player.Stats[5] = num;
          player.Stats[6] = num;
          player.Stats[7] = num;
          player.UpdateCount++;
          break;
        default:
          player.SendHelp("Usage: /stats <stat name> <amount>");
          break;
      }
      player.SendInfo("Successfully updated " + stat);
      return true;
    }
  }

  internal class UpdateCommand : Command
  {
    public UpdateCommand() : base("update", desc: "updates all players information in the current world", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.Manager.Data.AddPendingAction(db =>
      {
        foreach (var i in player.Owner.Players)
        {
          Account x = db.GetAccount(i.Value.AccountId);
          Player usr = i.Value;

          usr.Name = x.Name;
          usr.Client.Account.Rank = x.Rank;

          usr.UpdateCount++;
        }
      });
      player.SendInfo("Users Updated.");
      return true;
    }
  }

  internal class TeamCommand : Command
  {
    public TeamCommand() : base("team", desc: "sets your current team to the specified id", usage: "<team id>", permLevel: 3)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      try
      {
        player.Team = Convert.ToInt32(args);
        player.SendInfo("Updated to team #" + Convert.ToInt32(args));
        return true;
      }
      catch
      {
        return false;
      }
    }
  }

  internal class BanCommand : Command
  {
    public BanCommand() : base("ban", desc: "bans the specified player from the server", usage: "<player name>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var name = args;

      if (name == "") return false;
      player.Manager.Data.AddPendingAction(db =>
      {
        var cmd = db.CreateQuery();

        cmd.CommandText = "UPDATE accounts SET banned=1 WHERE name=@name LIMIT 1";
        cmd.Parameters.AddWithValue("@name", name);

        if (cmd.ExecuteNonQuery() <= 0) return;
        player.SendInfo("User was successfully banned");
        foreach (var i in player.Manager.Clients.Values.Where(i => i.Account.Name.EqualsIgnoreCase(name)))
        {
          i.Disconnect();
        }
      });
      return true;
    }
  }

  internal class UnbanCommand : Command
  {
    public UnbanCommand()
        : base("unban", desc: "unbans the specified player from the server", usage: "<player name>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var name = args;

      if (name == "") return false;
      player.Manager.Data.AddPendingAction(db =>
      {
        var cmd = db.CreateQuery();

        cmd.CommandText = "UPDATE accounts SET banned=0 WHERE name=@name LIMIT 1";
        cmd.Parameters.AddWithValue("@name", name);

        if (cmd.ExecuteNonQuery() <= 0) return;
        player.SendInfo("User was successfully unbanned");
        foreach (var i in player.Manager.Clients.Values.Where(i => i.Account.Name.EqualsIgnoreCase(name)))
        {
          i.Disconnect();
        }
      });
      return true;
    }
  }

  internal class MuteCommand : Command
  {
    public MuteCommand() : base("mute", desc: "mutes the specified player", usage: "<player name>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var name = args;

      if (name == "") return false;
      player.Manager.Data.AddPendingAction(db =>
      {
        var cmd = db.CreateQuery();

        cmd.CommandText = "UPDATE accounts SET muted=1 WHERE name=@name LIMIT 1";
        cmd.Parameters.AddWithValue("@name", name);

        if (cmd.ExecuteNonQuery() <= 0) return;
        player.SendInfo("User was successfully muted");
        foreach (var i in player.Owner.Players)
        {
          var x = db.GetAccount(i.Value.AccountId);
          var usr = i.Value;

          usr.Client.Account.Muted = x.Muted;

          usr.UpdateCount++;
        }
      });
      return true;
    }
  }

  internal class UnmuteCommand : Command
  {
    public UnmuteCommand() : base("unmute", desc: "unmutes the specified player", usage: "<player name>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var name = args;

      if (name == "") return false;
      player.Manager.Data.AddPendingAction(db =>
      {
        var cmd = db.CreateQuery();

        cmd.CommandText = "UPDATE accounts SET muted=0 WHERE name=@name LIMIT 1";
        cmd.Parameters.AddWithValue("@name", name);

        if (cmd.ExecuteNonQuery() <= 0) return;
        player.SendInfo("User was successfully unmuted");
        foreach (var i in player.Owner.Players)
        {
          var x = db.GetAccount(i.Value.AccountId);
          var usr = i.Value;

          usr.Client.Account.Muted = x.Muted;

          usr.UpdateCount++;
        }
      });
      return true;
    }
  }

  internal class AdminBuffCommand : Command
  {
    public AdminBuffCommand() : base("adminBuff", desc: "updates the item in the specified slot with admin properties", usage: "<slot id>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var index = Convert.ToInt32(args);
      var data = new ItemData
      {
        NamePrefix = "Admin",
        NameColor = 0xFF1297,
        DmgPercentage = 1000,
        Soulbound = true
      };
      if (player.Inventory.Data[index] == null)
        player.Inventory.Data[index] = data;
      else
      {
        player.Inventory.Data[index].NamePrefix = data.NamePrefix;
        player.Inventory.Data[index].NameColor = data.NameColor;
        player.Inventory.Data[index].DmgPercentage = data.DmgPercentage;
        player.Inventory.Data[index].Soulbound = data.Soulbound;
      }
      player.UpdateCount++;
      return true;
    }
  }

  internal class StrangifyCommand : Command
  {
    public StrangifyCommand() : base("strangify", desc: "updates the item in the specified slot with strange properties", usage: "<slot id>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      var index = Convert.ToInt32(args);
      player.SendInfo("Stranged");
      var data = new ItemData
      {
        NamePrefix = "Strange",
        NameColor = 0xFF5A28,
        Strange = true
      };
      if (player.Inventory.Data[index] == null)
        player.Inventory.Data[index] = data;
      else
      {
        player.Inventory.Data[index].NamePrefix = data.NamePrefix;
        player.Inventory.Data[index].NameColor = data.NameColor;
        player.Inventory.Data[index].Strange = data.Strange;
      }
      player.UpdateCount++;
      return true;
    }
  }

  internal class SkinEffectCommand : Command
  {
    public SkinEffectCommand() : base("skinEff", desc: "applies an xml effect to your character", usage: "<xml effect>", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.XmlEffect = args;
      player.UpdateCount++;
      return true;
    }
  }

  internal class CommandCommand : Command
  {
    public CommandCommand() : base("cmd", desc: "executes a command", permLevel: 6)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.Client.SendPacket(new GetTextInputPacket
      {
        Action = "sendCommand",
        Name = "Type the command"
      });
      return true;
    }
  }

  internal class RevealCommand : Command
  {
    public RevealCommand()
        : base("reveal", desc: "reveals the map", permLevel: 2)
    {
    }

    protected override bool Process(Player player, RealmTime time, string args)
    {
      player.Reveal = true;
      return true;
    }
  }
}