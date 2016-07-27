﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.realm.commands
{
    internal class MarketCommand : Command
    {
        public MarketCommand() : base("market", desc: "gives you access to the marketplace") { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            return true;
        }
    }

    internal class TutorialCommand : Command
    {
        public TutorialCommand() : base("tutorial", desc: "sends you to the tutorial world")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = 2050,
                GameId = World.TUT_ID,
                Name = "Tutorial",
                Key = Empty<byte>.Array,
            });
            return true;
        }
    }

    internal class TradeCommand : Command
    {
        public TradeCommand() : base("trade", desc: "sends a trade invite to the targeted player", usage: "<player name>")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /trade <username>");
                return true;
            }

            Player plr = player.Manager.FindPlayer(args);
            if (plr != null && plr.Owner == player.Owner)
            {
                player.RequestTrade(time, new RequestTradePacket { Name = plr.Name });
                return true;
            }
            return false;
        }
    }

    internal class WhoCommand : Command
    {
        public WhoCommand() : base("who", desc: "lists all players in the current world")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            var visiblePlayers = new List<string>();
            var vanishedPlayers = new List<string>();

            var copy = player.Owner.Players.Values.ToArray();
            if (copy.Length == 0)
                player.SendInfo("Nobody else is online");
            else
            {
                foreach (var p in copy)
                    if (!p.isNotVisible)
                        visiblePlayers.Add(p.Name);
                    else
                        vanishedPlayers.Add(p.Name);
                if (visiblePlayers.Count > 0)
                    player.SendInfo("Players online: " + string.Join(", ", visiblePlayers.ToArray()));
                else
                    player.SendInfo("Nobody else is online");
            }
            return true;
        }
    }

    internal class ServerCommand : Command
    {
        public ServerCommand() : base("server", desc: "shows the name of the current world")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.SendInfo(player.Owner.Name);
            return true;
        }
    }

    internal class PauseCommand : Command
    {
        public PauseCommand() : base("pause", desc: "pauses your game (until you [/pause] again)")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.HasConditionEffect(ConditionEffects.Paused))
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Paused,
                    DurationMS = 0
                });
                player.SendInfo("Game resumed.");
                return true;
            }
            if (player.Owner.EnemiesCollision.HitTest(player.X, player.Y, 8).OfType<Enemy>().Any())
            {
                player.SendError("Not safe to pause.");
                return false;
            }
            player.ApplyConditionEffect(new ConditionEffect
            {
                Effect = ConditionEffectIndex.Paused,
                DurationMS = -1
            });
            player.SendInfo("Game paused.");
            return true;
        }
    }

    internal class TeleportCommand : Command
    {
        public TeleportCommand() : base("teleport", desc: "teleports you to the targeted player", usage: "<player name>")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.Name.EqualsIgnoreCase(args))
            {
                player.SendInfo("You are already at yourself, and always will be!");
                return false;
            }
            if (player.Owner.AllowTeleport == false)
            {
                player.SendError("You are not allowed to teleport in this area!");
                return false;
            }

            foreach (var i in player.Owner.Players)
            {
                if (i.Value.Name.EqualsIgnoreCase(args))
                {
                    if (i.Value.isNotVisible)
                    {
                        player.SendInfo(string.Format("Unable to find player: {0}", args));
                        return false;
                    }
                    if (i.Value.HasConditionEffect(ConditionEffects.Invisible))
                    {
                        player.SendError("You can't teleport to invisible players");
                        return false;
                    }
                    player.Teleport(time, i.Value.Id);
                    return true;
                }
            }
            player.SendInfo(string.Format("Unable to find player: {0}", args));
            return false;
        }
    }

    internal class TellCommand : Command
    {
        public TellCommand() : base("tell", desc: "sends a private message to the targeted player", usage: "<player name> <message>")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string saytext = string.Join(" ", args);
            int index = args.IndexOf(' ');
            if (player.Client.Account.Muted) return false;
            if (!player.NameChosen)
            {
                player.SendError("Choose a name!");
                return false;
            }
            if (saytext.Equals(" ") || saytext == "")
            {
                player.SendError("Usage: /tell <player name> <text>");
                return false;
            }
            string playername = args.Substring(0, index);
            string msg = args.Substring(index + 1);

            if (msg.Trim() == "")
                return false;

            if (String.Equals(player.Name.ToLower(), playername.ToLower()))
            {
                player.SendInfo("Quit telling yourself!");
                return false;
            }

            foreach (var i in player.Manager.Clients.Values)
            {
                if (i.Account.NameChosen && i.Account.Name.EqualsIgnoreCase(playername))
                {
                    if (i.Player == null || i.Player.Owner == null)
                        continue;

                    player.Client.SendPacket(new TextPacket //echo to self
                    {
                        ObjectId = player.Id,
                        BubbleTime = 10,
                        Stars = player.Stars,
                        Name = player.Name,
                        Recipient = i.Account.Name,
                        Text = msg.ToSafeText(),
                        CleanText = ""
                    });

                    i.SendPacket(new TextPacket //echo to /tell player
                    {
                        ObjectId = player.Owner.Id == i.Player.Owner.Id ? player.Id : 0,
                        BubbleTime = (byte)(player.Owner.Id == i.Player.Owner.Id ? 10 : 0),
                        Stars = player.Stars,
                        Name = player.Name,
                        Recipient = i.Account.Name,
                        Text = msg.ToSafeText(),
                        CleanText = ""
                    });
                    return true;
                }
            }
            player.SendError(string.Format("{0} not found.", playername));
            return false;
        }
    }

    internal class HelpCommand : Command
    {
        public HelpCommand() : base("help", desc: "shows your available commands", usage: "<page number>") { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            Command[] cmds = player.Manager.Commands.Commands.Values
                .Where(x => x.HasPermission(player))
                .ToArray();

            int curPage = (args != "") ? Convert.ToInt32(args) : 1;
            double maxPage = Math.Floor((double) cmds.Length/10);

            if (curPage > maxPage + 1)
                curPage = (int) maxPage + 1;
            if (curPage < 1)
                curPage = 1;

            var sb = new StringBuilder(string.Format("Commands <Page {0}/{1}>: \n", curPage, maxPage + 1));

            int y = (curPage - 1) * 10;
            int z = y + 10;

            int commands = cmds.Length;
            if (z > commands)
            {
                z = commands;
            }

            for (int i = y; i < z; i++)
            {
                sb.Append(string.Format("[/{0}{1}]{2}", cmds[i].CommandName, (cmds[i].CommandUsage != "" ? " " + cmds[i].CommandUsage : null), (cmds[i].CommandDesc != "" ? ": " + cmds[i].CommandDesc : null)) + "\n");
            }
            player.SendHelp(sb.ToString());
            return true;
        }
    }

    internal class PartyCommand : Command
    {
        public PartyCommand() : base("party", desc: "shows commands available party commands", usage: "help") { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (args.Trim() != "")
            {
                string cmd = args.Split(' ')[0];
                string left = args.Split(' ').Length > 1 ? string.Join(" ", args.Split(' ').Skip(1)) : "";
                bool inParty = player.Party != null;
                bool leader = inParty && player.Party.Leader == player;
                switch (cmd)
                {
                    case "join":
                        if (left == "")
                        {
                            player.SendInfo("Usage: /party join <username>");
                            return false;
                        }
                        Player target = player.Owner.GetUniqueNamedPlayer(left);
                        if(target == null)
                        {
                            player.SendInfo("Player not found.");
                            return false;
                        }
                        if(target.Party == null)
                        {
                            player.SendInfo("Player is not in a party.");
                            return false;
                        }
                        if(target.Party.Leader != target)
                        {
                            player.SendInfo("You can only join the leader of the party.");
                            return false;
                        }
                        if(!target.Party.Invitations.Contains(player.AccountId))
                        {
                            player.SendInfo("You must be invited to join a party.");
                            return false;
                        }
                        if (player.Party != null)
                            if (player.Party.Leader == player)
                                player.Party.Disband();
                            else
                                player.Party.RemoveMember(player);
                        target.Party.AddMember(player);
                        break;
                    case "invite":
                        if (left == "")
                        {
                            player.SendInfo("Usage: /party invite <username>");
                            return false;
                        }
                        if(inParty && !leader)
                        {
                            player.SendInfo("You must be the leader of a party to invite others.");
                            return false;
                        }
                        if(!inParty)
                        {
                            player.Party = new Party(player);
                            player.UpdateCount++;
                            inParty = true;
                        }
                        Player target2 = player.Owner.GetUniqueNamedPlayer(left);
                        if (target2 == null)
                        {
                            player.SendInfo("Player not found.");
                            return false;
                        }
                        if(target2.Client == null)
                        {
                            player.SendError("Player no longer exists!");
                            return false;
                        }
                        if(target2.Party == player.Party)
                        {
                            player.SendInfo("Player is already in your party.");
                            return false;
                        }
                        if (!player.Party.Invitations.Contains(target2.AccountId))
                            player.Party.Invitations.Add(target2.AccountId);
                        player.Party.SendPacket(new TextPacket
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Recipient = "*Party*",
                            Text = target2.Name + " was invited to the party"
                        }, null);
                        target2.Client.SendPacket(new InvitedToPartyPacket
                        {
                            Name = player.Name,
                            PartyID = player.Party.ID
                        });
                        break;
                    case "kick":
                        if(left == "")
                        {
                            player.SendInfo("Usage: /party kick <username>");
                            return false;
                        }
                        if(!inParty || !leader)
                        {
                            player.SendInfo("You must be the leader of a party to kick others.");
                            return false;
                        }
                        Player target3 = player.Owner.GetUniqueNamedPlayer(left);
                        if (target3 == null)
                        {
                            player.SendInfo("Player not found.");
                            return false;
                        }
                        if (target3.Party != player.Party)
                        {
                            player.SendInfo("Player must be in your party.");
                            return false;
                        }
                        player.Party.SendPacket(new TextPacket
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Recipient = "*Party*",
                            Text = target3.Name + " was kicked from the party"
                        }, null);
                        player.Party.RemoveMember(target3);
                        break;
                    case "disband":
                    case "leave":
                        if(!inParty)
                        {
                            player.SendInfo("You are not in a party.");
                            return false;
                        }
                        if (player.Party.Leader == player)
                            player.Party.Disband();
                        else
                            player.Party.RemoveMember(player);
                        break;
                    case "chat":
                        if(left.Trim() == "")
                        {
                            player.SendInfo("Usage: /party chat <message> or /p <message>");
                            return false;
                        }
                        if(!inParty)
                        {
                            player.SendInfo("You are not in a party.");
                            return false;
                        }
                        player.Manager.Chat.SayParty(player, left.ToSafeText());
                        break;
                    case "help":
                        player.SendHelp("Party commands:\n[/party join <username>]: accept a party invite\n[/party invite <username>]: invite a player to your party (leader only)\n[/party kick <username>]: kick a user from your party (leader only)\n[/party leave]: leave your current party\n[/party chat <message>]: send a message to your party");
                        break;
                    default:
                        player.SendInfo("Invalid command!");
                        player.SendInfo("Type \"/party help\" for commands.");
                        break;
                }
            }
            else
            {
                if(player.Party == null)
                {
                    player.SendInfo("You are not in a party!");
                    player.SendInfo("Type \"/party help\" for commands.");
                    return false;
                }
                player.SendInfo("Party Leader:\n " + player.Party.Leader.Name);
                var members = player.Party.Members.Select(i => i.Name).ToList();
                player.SendInfo("Party Members:\n " + (members.Count > 0 ? string.Join(", ", members.ToArray()) : "None"));
                player.SendInfo("Type \"/party help\" for commands.");
            }
            return true;
        }
    }

    internal class PartyChatCommand : Command
    {
        public PartyChatCommand() : base("p", desc: "send a message to your party", usage: "<message>") { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (args.Trim() == "")
            {
                player.SendInfo("Usage: /party chat <message> or /p <message>");
                return false;
            }
            if (player.Party == null)
            {
                player.SendInfo("You are not in a party.");
                return false;
            }
            player.Manager.Chat.SayParty(player, args.ToSafeText());
            return true;
        }
    }

    internal class HomeCommand : Command
    {
        public HomeCommand() : base("home", desc: "sends you to your home world") { }

        public static Dictionary<int, World> homes = new Dictionary<int, World>();

        protected override bool Process(Player player, RealmTime time, string args)
        {
            World world;
            if(!homes.TryGetValue(player.Client.Account.AccountId, out world))
            {
                world = CustomWorld.Home(player);
                player.Manager.AddWorld(world);
                homes.Add(player.Client.Account.AccountId, world);
            }
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = 2050,
                GameId = world.Id,
                Name = world.Name,
                Key = Empty<byte>.Array,
            });
            return true;
        }
    }
}