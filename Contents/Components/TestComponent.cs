using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace TAT.Contents.Components
{
    internal class TestComponent : TATComponent
    {
        int timer;
        int randomType;
        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            randomType = Main.rand.Next(4);
        }
        public override void Update()
        {
            if (timer > 0)
            {
                timer--;
                return;
            }
            if (randomType == 0)
            {
                float dis = 640000;
                NPC target = null;
                foreach (NPC n in Main.npc)
                {
                    if (!n.active || n.friendly || n.immortal || !n.CanBeChasedBy())
                    {
                        continue;
                    }
                    if (n.boss)
                    {
                        target = n;
                        break;
                    }
                    float d = Vector2.DistanceSquared(n.Center, TileEntity.Center);
                    if (d < dis)
                    {
                        dis = d;
                        target = n;
                    }
                }
                if (target != null)
                {
                    var p = Projectile.NewProjectileDirect(null, TileEntity.Center, Vector2.Normalize(target.Center - TileEntity.Center) * 10, ProjectileID.CrystalBullet, 80, 0, Main.myPlayer);
                    p.tileCollide = false;
                    p.ignoreWater = true;
                    timer = 20;
                }
            }
            else if (randomType == 1)
            {
                foreach (NPC n in Main.npc)
                {
                    if (!n.active || n.friendly || n.immortal || Vector2.DistanceSquared(n.Center, TileEntity.Center) > 640000)
                    {
                        continue;
                    }
                    n.life -= 12;
                    CombatText.NewText(n.Hitbox, CombatText.LifeRegenNegative, 12);
                    n.HitEffect(0, 12);
                    n.checkDead();
                }
                timer = 60;
            }
            else if (randomType == 2)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active || player.dead || Vector2.DistanceSquared(player.Center, TileEntity.Center) > 640000)
                    {
                        continue;
                    }
                    int c = Math.Min(8, player.statLifeMax2 - player.statLife);
                    player.statLife += c;
                    if (c > 5)
                    {
                        CombatText.NewText(player.Hitbox, CombatText.LifeRegen, c);
                    }
                }
            }
            else
            {
                foreach (NPC n in Main.npc)
                {
                    if (!n.active || n.friendly || n.immortal || Vector2.DistanceSquared(n.Center, TileEntity.Center) > 409600)
                    {
                        continue;
                    }
                    Vector2 v = Vector2.Normalize(n.Center - TileEntity.Center) * 8 * n.knockBackResist;
                    if (v.Y > 0)
                    {
                        v.Y *= -1;
                    }
                    n.velocity += v;
                }
            }
        }
        public override void DrawEffect(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            float r = randomType switch
            {
                0 or 1 or 2 => 800,
                3 => (float)640,
                _ => throw new Exception(),
            };
            Color c = randomType switch
            {
                0 => Color.Red,
                1 => Color.Orange,
                2 => Color.Green,
                3 => Color.Blue,
                _ => throw new Exception()
            };
            Vector2 Circle(double t, float dis, double rot = 0, float a = 1, float b = 1)
            {
                return new Vector2((float)(a * Math.Cos(t)), (float)(b * Math.Sin(t))).RotatedBy(rot) * dis;
            }
            void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float wide, Color color)
            {
                Texture2D texture = TextureAssets.MagicPixel.Value;
                Vector2 unit = end - start;
                spriteBatch.Draw(texture, start + unit / 2 - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, unit.ToRotation() + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(wide, unit.Length()), SpriteEffects.None, 0f);
            }
            void DrawCircle(Vector2 center, float dis, bool toScreen = true, float rot = 0, float a = 1, float b = 1)
            {
                Vector2 offset = toScreen ? Vector2.Zero : (Vector2.One * Main.offScreenRange);
                for (int i = 0; i < 36; i++)
                {
                    Vector2 start = Circle(MathHelper.ToRadians(i * 10), dis, rot, a, b) + center + offset;
                    Vector2 end = Circle(MathHelper.ToRadians((i + 1) * 10), dis, rot, a, b) + center + offset;
                    DrawLine(Main.spriteBatch, start, end, 1f, c);
                }
            }
            DrawCircle(TileEntity.Center, r, Main.drawToScreen);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                TileEntity.Center - Main.screenPosition + (Main.drawToScreen ? Vector2.Zero : (Vector2.One * Main.offScreenRange)),
                new Rectangle(0, 0, 1, 1),
                c,
                0,
                Vector2.One / 2,
                16,
                SpriteEffects.None,
                0);
        }
    }
}
