using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TAT.Contents.Tiles
{
    internal class TestTile : TATTile
    {
        public override bool IsLoadingEnabled(Mod mod) => TAT.Test;
        public override void SetStaticDefaults()
        {
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            base.SetStaticDefaults();
            TileObjectData.addTile(Type);
        }
    }
    internal class TestTileItem : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => TAT.Test;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TestTile>());
        }
    }
}
