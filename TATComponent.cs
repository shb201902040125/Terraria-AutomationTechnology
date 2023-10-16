using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TAT
{
    internal abstract class TATComponent : ModItem
    {
        TATTileEntity entity;
        internal TATTileEntity TileEntity
        {
            get
            {
                return entity;
            }
            set
            {
                if (Parent is null)
                {
                    entity = value;
                    children.ForEach(c => c.TileEntity = value);
                }
            }
        }
        public TATComponent Parent { get;private set; }
        List<TATComponent> children = new();
        public IReadOnlyList<TATComponent> Children => children;
        public IReadOnlyList<TATComponent> Brothers => Parent?.children ?? new List<TATComponent>();
        public int Depth => 1 + (Parent?.Depth ?? -1);
        public TATComponent Root => Parent?.Root ?? this;
        public void AddComponent(TATComponent component)
        {
            if (!CanAccept(component) || component.CanApplyTo(this))
            {
                return;
            }
            component.Parent = this;
            component.entity = entity;
            children.Add(component);
        }
        public void RemoveComponent(TATComponent component)
        {
            if (component.Parent == this)
            {
                component.Parent = null;
                component.entity = null;
                children.Remove(component);
            }
        }
        public void ForeachLRN(Action<TATComponent> action)
        {
            children.ForEach(c => c.ForeachLRN(action));
            action(this);
        }
        public IEnumerable<TATComponent> EnumLRN()
        {
            foreach (TATComponent component in Children)
            {
                yield return component;
            }
            yield return this;
        }
        public void ForeachNLR(Action<TATComponent> action)
        {
            action(this);
            children.ForEach(c => c.ForeachNLR(action));
        }
        public IEnumerable<TATComponent> EnumNLR()
        {
            yield return this;
            foreach (TATComponent component in Children)
            {
                yield return component;
            }
        }
        public virtual bool CanApplyTo(TATComponent component) => true;
        public virtual bool CanAccept(TATComponent component) => true;
        public virtual void Update() { }
        public virtual void Reset() { }
        public virtual void UpdateFixEffect() { }
        public virtual void DrawEffect(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }
        public virtual void SpecialDraw(int i, int j, SpriteBatch spriteBatch) { }
        public static void Save(TATComponent root, BinaryWriter writer)
        {
            List<TATComponent> list = new();
            Dictionary<TATComponent, int> map = new();
            root.ForeachNLR(c =>
            {
                map[c] = list.Count;
                list.Add(c);
            });
            List<int> ptrs = new();
            list.ForEach(c =>
            {
                if (c.Parent == null)
                {
                    ptrs.Add(-1);
                }
                else
                {
                    ptrs.Add(map[c.Parent]);
                }
            });
            writer.Write(list.Count);
            TagCompound tag = new();
            tag["Items"] = list.ConvertAll(c => c.Item);
            TagIO.Write(tag, writer);
            ptrs.ForEach(writer.Write);
        }
        public static TATComponent Load(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            TagCompound tag = TagIO.Read(reader);
            List<TATComponent> items = tag.Get<List<Item>>("Items").ConvertAll(i => i.ModItem as TATComponent);
            TATComponent root = null;
            for (int i = 0; i < count; i++)
            {
                int ptr = reader.ReadInt32();
                if (ptr == -1)
                {
                    root = items[i];
                }
                else
                {
                    items[ptr].AddComponent(items[i]);
                }
            }
            return root;
        }
    }
}