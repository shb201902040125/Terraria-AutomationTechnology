using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TAT
{
    internal abstract class TATTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TATTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileID.Sets.PreventsSandfall[Type] = true;
            TileID.Sets.AvoidedByMeteorLanding[Type] = true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<TATTileEntity>().Kill(i, j);
        }
        public override bool RightClick(int i, int j)
        {
            if (Harmony.Utils.TileUtils.TryGetTileEntityAs(i, j, out TATTileEntity tat))
            {
                // TODO 打开组件UI
                return true;
            }
            return base.RightClick(i, j);
        }
        public virtual void SetTileEntityProperties(TATTileEntity entity)
        {

        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            var p = Harmony.Utils.TileUtils.GetTopLeftTileInMultitile(i, j);
            if (p.X == i && p.Y == j && Harmony.Utils.TileUtils.TryGetTileEntityAs(i, j, out TATTileEntity entity))
            {
                entity.DrawEffect(i,j,spriteBatch, ref drawData);
            }
        }
        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var p = Harmony.Utils.TileUtils.GetTopLeftTileInMultitile(i, j);
            if (p.X == i && p.Y == j && Harmony.Utils.TileUtils.TryGetTileEntityAs(i, j, out TATTileEntity entity))
            {
                entity.SpecialDraw(i, j, spriteBatch);
            }
        }
    }
}
