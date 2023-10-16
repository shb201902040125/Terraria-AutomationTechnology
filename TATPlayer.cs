using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ID;
using TAT.Contents.Components;

namespace TAT
{
    internal class TATPlayer : ModPlayer
    {
        protected override bool CloneNewInstances => true;
        public override void PostUpdate()
        {
            if (PlayerInput.Triggers.Old.MouseLeft && !PlayerInput.Triggers.Current.MouseLeft && Player.HeldItem.pick == 0)
            {
                if (WorldGen.InWorld(Player.tileTargetX, Player.tileTargetY))
                {
                    if (Harmony.Utils.TileUtils.TryGetTileEntityAs(Player.tileTargetX, Player.tileTargetY, out TATTileEntity entity))
                    {
                        if (entity.BasicComponent is null)
                        {
                            if (Player.HeldItem.ModItem is TATComponent component)
                            {
                                Item item = new(Player.HeldItem.type);
                                entity.SetBasicComponent(item.ModItem as TATComponent);
                                Player.ConsumeItem(Player.HeldItem.type);
                            }
                        }
                        else
                        {
                            if (entity.BasicComponent is null)
                            {
                                return;
                            }
                            Item.NewItem(null, entity.Center, entity.BasicComponent.Item);
                            entity.BasicComponent = null;
                        }
                    }
                }
            }
        }
    }
}