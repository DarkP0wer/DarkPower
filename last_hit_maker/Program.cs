using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace last_hit_maker
{
    class Program
    {
        private static List<string> _creepNames;
        private static Font _text;

        static void Main(string[] args)
        {
            _creepNames = new List<string>()
            {
                "npc_dota_neutral_alpha_wolf", "npc_dota_neutral_black_dragon", "npc_dota_neutral_big_thunder_lizard", "npc_dota_neutral_centaur_khan", "$npc_dota_neutral_dark_troll_warlord", "npc_dota_neutral_enraged_wildkin", 
                "npc_dota_neutral_forest_troll_high_priest", "npc_dota_neutral_ghost", "npc_dota_neutral_granite_golem", "npc_dota_neutral_gnoll_assassin", "npc_dota_neutral_harpy_storm", "npc_dota_neutral_mud_golem", "npc_dota_neutral_ogre_magi", 
                "npc_dota_neutral_polar_furbolg_ursa_warrior", "npc_dota_neutral_satyr_hellcaller", "npc_dota_neutral_satyr_soulstealer", "npc_dota_neutral_kobold_taskmaster",
                "npc_dota_creep_badguys_melee", "npc_dota_creep_badguys_melee_diretide", "npc_dota_creep_badguys_melee_upgraded",
                "npc_dota_creep_badguys_ranged", "npc_dota_creep_badguys_ranged_diretide", "npc_dota_creep_badguys_ranged_upgraded",
                "npc_dota_creep_goodguys_melee", "npc_dota_creep_goodguys_melee_diretide", "npc_dota_creep_goodguys_melee_upgraded",
                "npc_dota_creep_goodguys_ranged", "npc_dota_creep_goodguys_ranged_diretide", "npc_dota_creep_goodguys_ranged_upgraded",
                "npc_dota_goodguys_siege", "npc_dota_goodguys_siege_diretide", "npc_dota_goodguys_siege_upgraded",
                "npc_dota_badguys_siege", "npc_dota_badguys_siege_diretide", "npc_dota_badguys_siege_upgraded"
            };
            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 13,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;

        }
        
        private static double Damage(Unit me)
        {
            Ability quelling_blade;
            quelling_blade =  me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_QuellingBlade);
            if (quelling_blade != null) return me.DamageAverage * 1.40 + me.BonusDamage;
            else return me.DamageAverage + me.BonusDamage;
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            _text.OnLostDevice();
        }


        static void Drawing_OnEndScene(EventArgs args)//static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
                return;
            var player = ObjectMgr.LocalPlayer;
            if (player == null || !player.IsAlive || player.Hero == null || player.Team == Team.Observer || Game.GameTime > 1800 || player.Hero.DamageAverage > 150)
                return;
            double dmg = Damage(player.Hero);
            var creeps = ObjectMgr.GetEntities<Creep>().Where(x => x.IsSpawned && x.IsAlive && x.Team != player.Team).OrderBy(x => x.CreateTime).ToList();

            var drawList = new List<List<Creep>>();
            // Check if these creeps are in our namelist
            foreach (var creep in
                        from creep in creeps
                        let name = creep.Name
                        where _creepNames.Contains(name)
                        select creep)
            {
                Vector2 screenPos;
                if (Drawing.WorldToScreen(creep.Position, out screenPos))
                {
                    // Add creeps to lists to detect stacks
                    var found = false;
                    foreach (var l in drawList)
                    {
                            l.Add(creep);
                            found = true;
                            break;
                    }
                    // Create new list if new stack has been found
                    if (!found)
                        drawList.Add(new List<Creep> { creep });
                }
            }
            foreach (var l in drawList)
            {
                var counter = 1;
                foreach (var creep in l)
                {
                    Vector2 screenPos;
                    if (creep.IsAlive && Drawing.WorldToScreen(creep.Position, out screenPos))
                    {
                        double damage = (dmg * (1 - creep.DamageResist) + 1);
                        if (creep.Health > 0 && creep.Health < damage)
                        {
                            var text = string.Format("{0}h", creep.Health-damage);
                            var textSize = Drawing.MeasureText(text, "Arial", Drawing.DefaultTextSize, FontFlags.DropShadow);
                            _text.DrawText(null, text, (int)(screenPos.X - textSize.X / 2), (int)(screenPos.Y - textSize.Y / 2), Color.Red);
                        }
                        else if (creep.Health < damage + 88)
                        {
                            var text = string.Format("{0}h", creep.Health - damage);
                            var textSize = Drawing.MeasureText(text, "Arial", Drawing.DefaultTextSize, FontFlags.DropShadow);
                            _text.DrawText(null, text, (int)(screenPos.X - textSize.X / 2), (int)(screenPos.Y - textSize.Y / 2), Color.Yellow);
                        }    
                    }
                }
            }
        }

        static float GetDistance(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
        }
    }
}
