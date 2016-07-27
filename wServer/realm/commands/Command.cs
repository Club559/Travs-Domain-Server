﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using wServer.realm.entities;

namespace wServer.realm.commands
{
    public abstract class Command
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Command));

        protected Command(string name, string desc = "", string usage = "", int permLevel = 0)
        {
            CommandName = name;
            CommandDesc = desc;
            CommandUsage = usage;
            PermissionLevel = permLevel;
        }

        public string CommandName { get; private set; }
        public string CommandDesc { get; private set; }
        public string CommandUsage { get; private set; }
        public int PermissionLevel { get; private set; }

        protected abstract bool Process(Player player, RealmTime time, string args);

        private static int GetPermissionLevel(Player player)
        {
            switch (player.Client.Account.Rank)
            {
                case 9001:
                    return 0;
                default:
                    return player.Client.Account.Rank;
            }
        }


        public bool HasPermission(Player player)
        {
            return GetPermissionLevel(player) >= PermissionLevel;
        }

        public bool Execute(Player player, RealmTime time, string args)
        {
            if (!HasPermission(player))
            {
                player.SendInfo("You are not an admin");
                return false;
            }

            try
            {
                return Process(player, time, args);
            }
            catch (Exception ex)
            {
                Log.Error("Error when executing the command.", ex);
                player.SendError("Error when executing the command.");
                return false;
            }
        }
    }

    public class CommandManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (CommandManager));

        private readonly Dictionary<string, Command> _cmds;

        public CommandManager()
        {
            _cmds = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
            var t = typeof (Command);
            foreach (var instance in from i in t.Assembly.GetTypes() where t.IsAssignableFrom(i) && i != t select (Command) Activator.CreateInstance(i))
                _cmds.Add(instance.CommandName, instance);
        }

        public IDictionary<string, Command> Commands
        {
            get { return _cmds; }
        }

        public bool Execute(Player player, RealmTime time, string text)
        {
            var index = text.IndexOf(' ');
            var cmd = text.Substring(1, index == -1 ? text.Length - 1 : index - 1);
            var args = index == -1 ? "" : text.Substring(index + 1);

            Command command;
            if (!_cmds.TryGetValue(cmd, out command))
            {
                player.SendError("Unknown command!");
                return false;
            }
            Log.InfoFormat("[Command] <{0}> {1}", player.Name, text);
            return command.Execute(player, time, args);
        }
    }
}