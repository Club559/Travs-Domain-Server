using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.networking.svrPackets;
using db;

namespace wServer.realm.entities
{
    partial class Player
    {
        public List<PlayerQuest> ActiveQuests { get; set; }
        public List<PlayerQuest> FinishedQuests { get; set; }
        public List<PlayerQuest> CompletedQuests { get; set; }

        void HandleQuests(RealmTime time)
        {
            #region Questline: Tutorial
            #region Dodging Bullets
            if (IsObjectiveActive("Dodging Bullets", 0) && Inventory[0] != null)
                FinishObjective("Dodging Bullets", 0);
            #endregion
            #region Chicken Training
            if (IsQuestActive("Chicken Training"))
            {
                if(IsObjectiveActive("Chicken Training", 0) && Owner.Name == "Tutorial")
                {
                    bool chickenFound = false;
                    foreach (var i in Owner.Enemies)
                        if (i.Value.ObjectDesc.ObjectId == "Evil Chicken")
                            chickenFound = true;
                    if(!chickenFound)
                        FinishObjective("Chicken Training", 0);
                }
                if(IsObjectiveActive("Chicken Training", 1) && Inventory[1] != null)
                    FinishObjective("Chicken Training", 1);
            }
            #endregion
            #endregion
        }

        public void QuestCheckDeath(string name)
        {
            switch (name)
            {
                case "Evil Chicken God":
                    {
                        if (IsObjectiveActive("A Poultry Task", 0))
                            FinishObjective("A Poultry Task", 0);
                    } break;
            }
        }

        public bool IsObjectiveActive(string name, int id)
        {
            return ActiveQuests.Where(_ => (_.Quest.QuestId.EqualsIgnoreCase(name) && (_.Objectives.Where(o => o.Id == id && !o.Completed).Count() > 0))).Count() > 0;
        }

        public bool IsQuestActive(string name)
        {
            return ActiveQuests.Where(_ => _.Quest.QuestId.EqualsIgnoreCase(name)).Count() > 0;
        }

        public void FinishObjective(string name, int id)
        {
            if (!IsQuestActive(name))
                return;
            var quest = GetActiveQuest(name);
            foreach (var i in quest.Objectives)
                if (i.Id == id)
                    i.Completed = true;
            bool complete = true;
            foreach (var i in quest.Objectives)
                if (!i.Completed)
                    complete = false;
            if (complete)
                FinishQuest(name);
        }

        public void FinishQuest(string name)
        {
            if (!IsQuestActive(name))
                return;
            var quest = Manager.GameData.Quests[Manager.GameData.IdToQuestType[name]];
            ActiveQuests.Remove(GetActiveQuest(name));
            FinishedQuests.Add(CreatePQuest(quest));
            client.SendPacket(new NotificationPacket
            {
                Color = new ARGB(0x0094FF),
                ObjectId = Id,
                Text = "Quest Finished: " + name
            });
        }

        public string GetQuestString(List<PlayerQuest> list)
        {
            return Utils.GetCommaSepString(list.Select(_ => _.Quest.QuestType).ToArray());
        }

        public PlayerQuest CreatePQuest(Quest quest)
        {
            var pQuest = new PlayerQuest();
            pQuest.Quest = quest;
            pQuest.Id = quest.QuestType;
            pQuest.Name = quest.QuestId;
            pQuest.Objectives = new List<PlayerQuestObjective>();
            foreach (var i in quest.Objectives)
            {
                var pObjective = new PlayerQuestObjective();
                pObjective.Completed = false;
                pObjective.Id = i.Id;
                pObjective.Text = i.Text;
                pQuest.Objectives.Add(pObjective);
            }
            return pQuest;
        }

        public PlayerQuest GetActiveQuest(string name)
        {
            var quest = Manager.GameData.Quests[Manager.GameData.IdToQuestType[name]];
            foreach (var i in ActiveQuests)
                if (i.Quest == quest)
                    return i;
            return null;
        }

        public PlayerQuest GetFinishedQuest(string name)
        {
            var quest = Manager.GameData.Quests[Manager.GameData.IdToQuestType[name]];
            foreach (var i in FinishedQuests)
                if (i.Quest == quest)
                    return i;
            return null;
        }

        public PlayerQuest GetCompletedQuest(string name)
        {
            var quest = Manager.GameData.Quests[Manager.GameData.IdToQuestType[name]];
            foreach (var i in CompletedQuests)
                if (i.Quest == quest)
                    return i;
            return null;
        }

        public bool CanGetQuest(Quest quest)
        {
            if (quest.LevelRequirement > Level)
                return false;
            foreach (var i in ActiveQuests)
                if (i.Quest == quest)
                    return false;
            foreach (var i in FinishedQuests)
                if (i.Quest == quest)
                    return false;
            foreach (var i in CompletedQuests)
                if (i.Quest == quest)
                    return false;
            foreach (var i in quest.QuestRequirement)
            {
                bool found = false;
                foreach (var o in CompletedQuests)
                    if (o.Quest.QuestId == i)
                        found = true;
                if (!found)
                    return false;
            }
            return true;
        }

        public bool HandleRewards(Quest quest)
        {
            if (quest.Rewards.Items.Count > 0)
            {
                int availableSlots = 0;
                for (int i = 4; i < Inventory.Length; i++)
                    if (Inventory[i] == null)
                        availableSlots++;
                if (quest.Rewards.Items.Count > availableSlots)
                {
                    SendError("Not enough inventory space to complete quest");
                    return false;
                }
                var rewardItems = new Queue<Item>();
                foreach (var i in quest.Rewards.Items)
                    rewardItems.Enqueue(Manager.GameData.Items[Manager.GameData.IdToObjectType[i]]);
                for (int i = 4; i < Inventory.Length; i++)
                {
                    Item nextItem = rewardItems.Dequeue();
                    Inventory[i] = nextItem;
                    Inventory.Data[i] = null;
                    if (rewardItems.Count == 0)
                        break;
                }
                UpdateCount++;
            }
            if (quest.Rewards.Exp > 0)
            {
                Experience += quest.Rewards.Exp;
                UpdateCount++;
                CheckLevelUp();
            }
            if (quest.Rewards.Fame > 0)
            {
                Manager.Data.AddPendingAction(db =>
                {
                    CurrentFame = client.Account.Stats.Fame = db.UpdateFame(client.Account, quest.Rewards.Fame);
                }, PendingPriority.Emergent);
                UpdateCount++;
            }
            return true;
        }
    }
}
