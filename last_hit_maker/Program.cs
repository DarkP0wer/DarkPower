using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ensage;
using SharpDX;
using SharpDX.Direct3D9;

namespace last_hit_maker
{
    class Program
    {

        static void Main(string[] args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static double Damage(Unit me)
        {
            Ability quelling_blade;
            quelling_blade =  me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_QuellingBlade);
            if (quelling_blade != null) return me.DamageAverage * 1.40 + me.BonusDamage;
            else return me.DamageAverage + me.BonusDamage;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || Game.GameTime > 1800 || player.Hero.DamageAverage > 130)
                return;
            double dmg = Damage(player.Hero);
            
            var creeps = ObjectMgr.GetEntities<Creep>().Where(x => x.Team != player.Team && x.IsVisible && x.IsAlive && x.IsSpawned);
            foreach (var creep in creeps)
            {
                double damage = (dmg * (1 - creep.DamageResist) + 1);
                if(creep.Health> 0 && creep.Health < damage)
                {
                    Vector2 screenPos;
                    if (Drawing.WorldToScreen(creep.Position, out screenPos))
                    {
                        var text = string.Format("{0} ◘", (int)(creep.Health - damage));
                        var textSize = Drawing.MeasureText(text, "Arial", Drawing.DefaultTextSize, FontFlags.DropShadow);
                        Drawing.DrawText(text, new Vector2(screenPos.X - textSize.X / 2, screenPos.Y - textSize.Y / 2),
                            Color.Red, FontFlags.DropShadow);
                    }
                }
				else if( creep.Health < damage+88) 
                {
                    Vector2 screenPos;
                    if (Drawing.WorldToScreen(creep.Position, out screenPos))
                    {
                        var text = string.Format("{0} ◘", (int)(creep.Health - damage));
                        var textSize = Drawing.MeasureText(text, "Arial", Drawing.DefaultTextSize, FontFlags.DropShadow);
                        Drawing.DrawText(text, new Vector2(screenPos.X - textSize.X / 2, screenPos.Y - textSize.Y / 2),
                            Color.Yellow, FontFlags.DropShadow);
                    }
                }
                
            }
        }

    }
}
