using System;
using System.Linq;
using Ensage.Common.Menu;
using System.Threading;
using Ensage.Common;
using Ensage;
using Ensage.Common.Extensions;

namespace echo_sabre
{
    class Program
    {
        private static readonly Ensage.Common.Menu.Menu Menu = new Ensage.Common.Menu.Menu("Fast attack with echo_sabre", "test", true);

        static void Main(string[] args)
        {
            Menu.AddItem(new Ensage.Common.Menu.MenuItem("HoldKey228", "HoldKey").SetValue(new KeyBind('K', KeyBindType.Press)));
            Menu.AddItem(new Ensage.Common.Menu.MenuItem("RefreshKey228", "RefreshKey").SetValue(new KeyBind('K', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
        }

        private static Boolean IsWork = false;

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Game.IsKeyDown(Menu.Item("HoldKey228").GetValue<KeyBind>().Key))
            {
                if (ObjectManager.LocalHero == null) return;
                var me = ObjectManager.LocalHero;
                if (!IsWork)
                {
                    var items = me.Inventory.Items;
                    foreach (var item in items.Where(item => item.Name == "item_echo_sabre"))
                    {
                        me.DropItem(item, me.NetworkPosition, false);
                        IsWork = true;
                    }
                    Thread.Sleep(1);
                };

                var droppedItems = ObjectManager.GetEntities<PhysicalItem>().Where(x => x.Distance2D(me) < 200).Reverse().ToList();
                foreach (var item in droppedItems)
                {
                    Console.WriteLine(item.Name);
                    me.PickUpItem(item, true);
                    me.Attack(Game.MousePosition, true);
                }
            }

            else if (Game.IsKeyDown(Menu.Item("RefreshKey228").GetValue<KeyBind>().Key))
            {
                IsWork = false;
            }
        }
    }
}
