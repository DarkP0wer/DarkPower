using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Weaver_Helper
{
    class Program
    {

        #region Menu
        public static Point[] xy1 = new Point[1];
        public static Boolean[] IsOnTarget = new Boolean[1];
        public static Point[] xy2 = new Point[1];
        public static string[] str = new string[1];
        public static Font[] _font = new Font[1];
        public static Color[] text_color = new Color[1];
        public static int rect_w, rect_h; static Boolean IsMenuOpen = false;

        public static void Add(string text, Color s, int text_height)
        {
            Array.Resize<Font>(ref _font, _font.Length + 1);
            Array.Resize<Point>(ref xy1, xy1.Length + 1);
            Array.Resize<Point>(ref xy2, xy2.Length + 1);
            Array.Resize<string>(ref str, str.Length + 1);
            Array.Resize<Color>(ref text_color, text_color.Length + 1);
            Array.Resize<Boolean>(ref IsOnTarget, IsOnTarget.Length + 1);

            int n = _font.Length - 1;
            var pos1 = new Vector2(xy1[0].X, xy2[n - 1].Y);
            var pos2 = new Vector2(xy2[0].X, (xy2[0].Y - xy1[0].Y) * n + xy1[0].Y);

            IsOnTarget[n] = false;
            xy1[n].X = xy1[0].X;
            xy2[n].X = xy2[0].X;
            xy1[n].Y = xy2[n - 1].Y;
            xy2[n].Y = (xy2[0].Y - xy1[0].Y) + xy2[n - 1].Y;
            text_color[n] = s;
            str[n] = text;

            _font[n] = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = text_height,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               }
            );

            //Drawing.DrawRect(pos1, pos2, new Color(0xE4, 0xDD, 0xDD, 0x20));
            _font[n].DrawText(null, text, xy1[n].X + 5, (rect_w % 2) + xy1[n].Y, s);

        }

        public static void Menu_Label1(int x1, int y1, string text, Color s, int rect_width, int rect_heigth, int text_height)
        {
            rect_w = rect_width; rect_h = rect_heigth; text_color[0] = s;
            xy1[0].X = x1;
            xy2[0].X = x1 + rect_width;
            xy1[0].Y = y1;
            xy2[0].Y = y1 + rect_heigth;
            str[0] = text;
            IsOnTarget[0] = false; IsMenuOpen = false;
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                return;
            _font[0] = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = text_height,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               }
            );
        }

        #endregion

        private static int MomentDownHealth;
        private static int MinHealthDownToDown;
        private static Boolean IsScriptEnabled;
        private static System.Timers.Timer aTimer;

        static void Main(string[] args)
        {

            Game.OnWndProc += Game_OnGameWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Menu_Label1(40, 40, "Menu", Color.Red, 150, 20, 17);
            Add("MomentDownHealth - 400", Color.Blue, 15);
            Add("MinChangedHealthToUse - 400", Color.Blue, 15);
            Add("Script (NUMPAD0 or click) - Enabled", Color.Blue, 15);
            MomentDownHealth = 400;
            MinHealthDownToDown = 400;
            IsScriptEnabled = true;

            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
        }

        private static uint[] LH = new uint[5];

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            var player = ObjectMgr.LocalPlayer;
            if (!Game.IsInGame || player == null || player.Hero == null || Game.IsPaused) return;
            Ability ult = player.Hero.Spellbook.Spell4;
            if (ult.Name != null && "weaver_time_lapse" == ult.Name)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (LH[i] == null) LH[i] = player.Hero.Health;
                }
                LH[4] = LH[3]; LH[3] = LH[2]; LH[2] = LH[1]; LH[1] = LH[0]; LH[0] = player.Hero.Health;
                if(LH[4] - player.Hero.Health > 0)
                    str[0] = "Menu " + LH[4] + "(+" + ((int)LH[4] - (int)player.Hero.Health) + ")";
                else str[0] = "Menu " + LH[4] + "(-" + ((int)player.Hero.Health - (int)LH[4]) + ")"; 

                if (ult.Level >= 1 && !(ult.Cooldown > 0) && IsScriptEnabled)
                {
                    if (((int)LH[0] - (int)player.Hero.Health) >= MomentDownHealth)
                    {
                            ult.UseAbility();
                            Console.WriteLine("Cast ult 1 weaver");
                    }
                    if (((int)LH[4] - (int)player.Hero.Health) >= MinHealthDownToDown)
                    {
                        if (ult.Cooldown > 0 || !IsScriptEnabled) return;
                        ult.UseAbility();
                        Console.WriteLine("Cast2");
                    }

                    if (player.Hero.Health <= MinHealthDownToDown && player.Hero.Health+100 <= LH[4])
                    {
                        if (ult.Cooldown > 0 || !IsScriptEnabled) return;
                        ult.UseAbility();
                        Console.WriteLine("Cast3");
                    }
                }
            }
        }
        
        public static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                return;

            for (int i = 1; i < _font.Length; i++)
            {
                if (_font[i] == null || (!IsMenuOpen)) continue;
                xy1[i].X = xy1[0].X;
                xy2[i].X = xy2[0].X;
                xy1[i].Y = xy2[i - 1].Y;
                xy2[i].Y = (xy2[0].Y - xy1[0].Y) + xy2[i - 1].Y;
                _font[i].DrawText(null, str[i], xy1[i].X, xy1[i].Y, (IsOnTarget[i]) ? Color.HotPink : text_color[i]);
            }

            if (_font[0] != null ) _font[0].DrawText(null, str[0], xy1[0].X, xy1[0].Y, (IsOnTarget[0]) ? Color.HotPink : text_color[0]);
           
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            for (int i = 0; i < _font.Length; i++)
            {
                _font[i].OnResetDevice();
            }
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
           for (int i = 0; i < _font.Length; i++)
            {
                _font[i].OnLostDevice();
            }
        }

        public class ClipCursor
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);
        }

       private static readonly uint WM_LBUTTONUP = 0x0202;
       private static readonly uint WM_MOUSEMOVE = 0x0200;
       private const int WM_KEYUP = 0x0101;
       private const int WM_KEYDOWN = 0x0105;
       private static Boolean IsMenuMoved=false;

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen) return;
            
            Point CursorPosition;
            ClipCursor.GetCursorPos(out CursorPosition);

            if (args.Msg == WM_KEYUP && args.WParam == 0x60)
            {
                IsScriptEnabled = !IsScriptEnabled;
                str[3] = "Script (NUM_PAD1 or click) - " + ((IsScriptEnabled) ? "Enabled" : "Disabled");
            }
            else if (args.Msg == WM_LBUTTONUP)
            {
                for (int i = 0; i < _font.Length; i++)
                {
                    if ((CursorPosition.X >= xy1[i].X && CursorPosition.Y >= xy1[i].Y) && (CursorPosition.X <= xy2[i].X && CursorPosition.Y <= xy2[i].Y))
                    {
                        Console.WriteLine("Clicked " + i + " item");
                        switch (i)
                        {
                            case 0:
                                {
                                    IsMenuOpen = !IsMenuOpen;
                                    break;
                                }
                            case 1:
                                {
                                    MomentDownHealth += 100;
                                    if (MomentDownHealth >= 1000) MomentDownHealth = 300;
                                    str[1] = "MomentDownHealt - " + MomentDownHealth;
                                    break;
                                }
                            case 2:
                                {
                                    MinHealthDownToDown += 100;
                                    if (MinHealthDownToDown > 900) MinHealthDownToDown = 200;
                                    str[2] = "MinChangedHealthToUse - " + MinHealthDownToDown;
                                    break;
                                }
                            case 3:
                                {
                                    IsScriptEnabled = !IsScriptEnabled;
                                    str[3] = "Script (NUMPAD0 or click) - " + ((IsScriptEnabled) ? "Enabled" : "Disabled");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unknow item (weawer helper)");
                                    break;
                                }
                        }
                    }
                }
            }
            else if (args.Msg == WM_MOUSEMOVE)
            {
                for (int i = 0; i < _font.Length; i++)
                {
                    if (CursorPosition.X >= xy1[i].X && CursorPosition.Y >= xy1[i].Y && CursorPosition.X <= xy2[i].X && CursorPosition.Y <= xy2[i].Y)
                    {
                        IsOnTarget[i] = true;

                    }
                    else IsOnTarget[i] = false;
                }
                if (IsMenuMoved)
                {
                    if((Control.MouseButtons & MouseButtons.Right) != 0)
                    {
                        xy1[0].X = CursorPosition.X;
                        xy2[0].X = CursorPosition.X + rect_w;
                        xy1[0].Y = CursorPosition.Y;
                        xy2[0].Y = CursorPosition.Y + rect_h;
                    }
                    else IsMenuMoved = false;
                }
                if (!IsMenuMoved && (Control.MouseButtons & MouseButtons.Right) != 0)
                {
                    if ((CursorPosition.X >= xy1[0].X && CursorPosition.Y >= xy1[0].Y) && (CursorPosition.X <= xy2[0].X && CursorPosition.Y <= xy2[0].Y))
                    {
                        IsMenuMoved = true;
                    }
                }
            }

        }
    }
}
