using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TAT
{
    internal class TATTileEntity : ModTileEntity
    {
        public TATComponent BasicComponent { get; internal set; }
        public Vector2 Center { get; private set; }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && TileLoader.GetTile(tile.TileType) is TATTile;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            TileObjectData data = TileObjectData.GetTileData(type, style, alternate);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int width = data.Width;
                int height = data.Height;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height, TileChangeType.None);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            }
            Point16 tileOrigin = data.Origin;
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
            if (TileLoader.GetTile(type) is TATTile tile)
            {
                TATTileEntity entity = ByID[placedEntity] as TATTileEntity;
                tile.SetTileEntityProperties(entity);
                entity.Center = new Vector2(i - tileOrigin.X + data.Width / 2f, j - tileOrigin.Y + data.Height / 2) * 16;
                NetMessage.SendData(MessageID.TileEntitySharing, number: placedEntity, number2: i - tileOrigin.X, number3: j - tileOrigin.Y);
            }
            return placedEntity;
        }
        public override void OnNetPlace()
        {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }
        public void SetBasicComponent(TATComponent component)
        {
            OnKill();
            component.TileEntity = this;
            BasicComponent = component;
        }
        public override void Update()
        {
            UpdateComponent();
        }
        public override void NetSend(BinaryWriter writer)
        {
            if (BasicComponent is not null)
            {
                writer.Write(true);
                TATComponent.Save(BasicComponent, writer);
            }
            else
            {
                writer.Write(false);
            }
        }
        public override void NetReceive(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
                BasicComponent = TATComponent.Load(reader);
                if (BasicComponent is not null)
                {
                    BasicComponent.TileEntity = this;
                }
            }
            else
            {
                BasicComponent = null;
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag[nameof(Center)] = Center;
            if (BasicComponent is not null)
            {
                using MemoryStream stream = new();
                using BinaryWriter writer = new(stream);
                TATComponent.Save(BasicComponent, writer);
                tag[nameof(BasicComponent)] = stream.ToArray();
            }
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet(nameof(Center), out Vector2 v))
            {
                Center = v;
            }
            else
            {
                Point16 p = ByPosition.First(pair => pair.Value.ID == ID).Key;
                var data = TileObjectData.GetTileData(Main.tile[p.X, p.Y]);
                Center = new Vector2(p.X + data.Width / 2f, p.Y + data.Height / 2f) * 16;
            }
            if (tag.TryGet(nameof(BasicComponent), out byte[] dataBytes))
            {
                using MemoryStream stream = new(dataBytes);
                using BinaryReader reader = new(stream);
                try
                {
                    BasicComponent = TATComponent.Load(reader);
                    if (BasicComponent is not null)
                    {
                        BasicComponent.TileEntity = this;
                    }
                }
                catch (Exception e)
                {
                    ModContent.GetInstance<TAT>().Logger.Debug(e);
                    BasicComponent = null;
                }
            }
        }
        public virtual void DrawEffect(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if(BasicComponent is not null)
            {
                foreach(var c in BasicComponent.EnumNLR())
                {
                    c.DrawEffect(i, j, spriteBatch, ref drawData);
                }
            }
        }
        public virtual void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (BasicComponent is not null)
            {
                foreach (var c in BasicComponent.EnumNLR())
                {
                    c.SpecialDraw(i, j, spriteBatch);
                }
            }
        }
        public void UpdateComponent()
        {
            if (BasicComponent is null)
            {
                return;
            }

        }
        public override void OnKill()
        {
            if(BasicComponent is not null)
            {
                BasicComponent.TileEntity = null;
                List<TATComponent> cs = BasicComponent.EnumLRN().ToList();
                foreach(var c in cs)
                {
                    c.Parent?.RemoveComponent(c);
                    Item.NewItem(new EntitySource_TileEntity(this), Center, c.Item);
                }
                BasicComponent = null;
            }
        }
    }
}