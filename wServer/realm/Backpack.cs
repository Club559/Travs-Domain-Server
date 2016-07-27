using db;
using System;
using System.Collections;
using System.Collections.Generic;
using wServer.realm.entities;

namespace wServer.realm
{
    public class Backpack : Inventory
    {
        public Backpack(IContainer parent, int size, JsonItem bag) : this(parent, new Item[size], new ItemData[size], bag)
        {
        }

        public Backpack(IContainer parent, Item[] items, ItemData[] datas, JsonItem bag)
            : base(parent, items, datas)
        {
            this.Size = items.Length;
            this.Bag = bag;
        }

        public int Size { get; set; }
        public JsonItem Bag { get; set; }
    }
}