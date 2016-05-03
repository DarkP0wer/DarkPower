using System;
using System.Linq;
using Ensage.Common.Menu;
using Ensage.Common;
using Ensage;
using Ensage.Common.Extensions;

namespace echo_sabre
{
    class Program
    {
        private static readonly Ensage.Common.Menu.Menu Menu = new Ensage.Common.Menu.Menu("Echo_Sabre abuse", "Echo_Sabre", true);

        static void Main(string[] args)
        {
            Menu.AddItem(new Ensage.Common.Menu.MenuItem("Key228", "Double Tap this key.").SetValue(new KeyBind('K', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnWndProc += Game_OnGameWndProc;
        }

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame) return;
            try
            {
                if (args.Msg == 0x0101 && args.WParam == Menu.Item("Key228").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                {
                    if (ObjectManager.LocalHero == null) return;
                    var me = ObjectManager.LocalHero;
                    var items = me.Inventory.Items;
                    foreach (var item in items.Where(item => item.Name == "item_echo_sabre"))
                    {
                        me.DropItem(item, me.NetworkPosition, false);
                    }

                    var droppedItems = ObjectManager.GetEntities<PhysicalItem>().Where(x => x.Distance2D(me) < 250 && x.Item.Name == "item_echo_sabre").Reverse().ToList();
                    foreach (var item in droppedItems)
                    {
                        me.PickUpItem(item, false);
                        me.Attack(Game.MousePosition, false);
                    }
                }
            }
            catch{ }
        }
    }
}
