using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Ensage.Common.Objects;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;

namespace DotaBuff_Overlay
{
    class Program
    {
        #region OVERLAY
        private static readonly DotaTexture DecreaseArrow = Textures.GetTexture("materials/ensage_ui/other/arrow_usual");
        private static readonly DotaTexture IncreaseArrow = Textures.GetTexture("materials/ensage_ui/other/arrow_usual_left");
        private static DotaTexture _texture = Textures.GetTexture("materials/ensage_ui/other/disa");
        private static bool DisplayOverlay { get; set; }


        private static readonly Vector2 OverlaySize = new Vector2(800, 400);
        private static Vector2 OverlayPosition;
        private static Boolean OverlayDrag { get; set; }
        private static Vector2 OverlayDragHeler;

        private static Vector2 arrowRectangleSize;
        private static Vector2 PlayerTextSize;
        private static Vector2 PlayerCaptionTextSize;

        private static int SelectedPlayer = 0;
        private static int SelectedPlayerByMouse = 0;
        #endregion

        private static readonly Ensage.Common.Menu.Menu SubMenu = new Ensage.Common.Menu.Menu("Dota Buff Overlay", "DOTA BUFF", true);
        private static String filename = "Dota_Buff.ini";

        public static Repo[] Repos = new Repo[20];
        private static List<String> PlayersList_HeroName = new List<String>();
        private static List<String> PlayersList_RWA = new List<String>();
        private static List<String> [] PlayersList_LoadedInformation = new List<String> [40];
        private static List<String> [] PlayersInformation = new List<String>[4];
        private static List<String> PlayersList_LoadedSteamID = new List<String>();
        private static List<String> PlayersList_PlayerName = new List<String>();
        private static ulong MatchId;
        private static Font FontText18 = new Font( Drawing.Direct3DDevice9, new FontDescription { FaceName = "Arial", Height = 18, OutputPrecision = FontPrecision.Default,Quality = FontQuality.Default});
        private static Font FontText16 = new Font(Drawing.Direct3DDevice9, new FontDescription { FaceName = "Arial", Height = 16, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });
        private static Font FontText14 = new Font(Drawing.Direct3DDevice9, new FontDescription { FaceName = "Arial", Height = 14, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });
        private static Font FontText12 = new Font(Drawing.Direct3DDevice9, new FontDescription { FaceName = "Arial", Height = 12, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });

        public struct Repo
        {
            public String RepoM;
            //public String RepoT;
            public int GamesPlayed;
            public uint SteamId;
        }

        private static System.Timers.Timer aTimer;

        private static void GetInformation(int PID)
        {
            PlayersInformation[0].Clear(); PlayersInformation[1].Clear(); PlayersInformation[2].Clear(); PlayersInformation[3].Clear();
            PlayersInformation[0].Add(PlayersList_HeroName[PID]);
            if (PlayersList_LoadedSteamID[PID].ToString() == "Loading...")
            {
                PlayersInformation[0].Clear();
                PlayersInformation[0].Add("This persson not loaded or disconnected!");
                PlayersInformation[0].Add("Player Name : " + PlayersList_PlayerName[PID]);
                PlayersInformation[0].Add("Steamid: " + Repos[PID].SteamId);
                PlayersInformation[0].Add("Hero: " + PlayersList_HeroName[PID]);

                PlayersList_LoadedInformation[PID].Clear(); PlayersList_LoadedInformation[PID + 10].Clear();
                PlayersList_LoadedInformation[PID + 20].Clear(); PlayersList_LoadedInformation[PID + 30].Clear();

                for (int i = 0; i < PlayersInformation[0].Count - 1; i++)
                    PlayersList_LoadedInformation[PID].Add(PlayersInformation[0][i]);
                for (int i = 0; i < PlayersInformation[1].Count - 1; i++)
                    PlayersList_LoadedInformation[PID + 10].Add(PlayersInformation[1][i]);
                for (int i = 0; i < PlayersInformation[2].Count - 1; i++)
                    PlayersList_LoadedInformation[PID + 20].Add(PlayersInformation[2][i]);
                for (int i = 0; i < PlayersInformation[3].Count - 1; i++)
                    PlayersList_LoadedInformation[PID + 30].Add(PlayersInformation[3][i]);
                return;
            }

            if (PlayersList_LoadedInformation[PID].Count > 0 && PlayersList_LoadedInformation[PID][0] == PlayersList_LoadedSteamID[PID])
            {
                PlayersInformation[0].Clear();
                for(int i = 0; i < PlayersList_LoadedInformation[PID].Count-1; i++)
                    PlayersInformation[0].Add(PlayersList_LoadedInformation[PID][i]);
            }
            else
            {
                PlayersInformation[0].Clear();
                PlayersInformation[0].Add("Loading...");
                String result = "";
                string steamid = PlayersList_LoadedSteamID[PID];
                if (Convert.ToInt32(PlayersList_LoadedSteamID[PID]) < 10) steamid = "346713680";
                var webRequest = WebRequest.Create("http://www.dotabuff.com/players/" + steamid);
                ((HttpWebRequest)webRequest).UserAgent = ".NET Framework";
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new System.IO.StreamReader(content))
                {
                    var strContent = reader.ReadToEnd();
                    result = strContent;
                }
                String result2 = result;
                int startpos = result.IndexOf("<div class=\"r-table r-only-mobile-5 heroes-overview\">");
                if (startpos == -1)
                {
                    PlayersInformation[0].Clear();
                    PlayersInformation[0].Add("THIS PROFILE IS PRIVATE");
                }
                else if (startpos > -1)
                {
                    int finishpos = result.IndexOf("/article", startpos);
                    result = result.Substring(startpos, finishpos - startpos);
                    int NextPos = result.IndexOf("matches?hero=", 0);
                    PlayersInformation[0].Clear();
                    PlayersInformation[0].Add(PlayersList_LoadedSteamID[PID]); PlayersInformation[1].Add(" "); PlayersInformation[2].Add(" "); PlayersInformation[3].Add(" ");
                    PlayersInformation[0].Add("Hero"); PlayersInformation[1].Add("Matches"); PlayersInformation[2].Add("WinRate"); PlayersInformation[3].Add("KDA");
                    while (NextPos > -1)
                    {
                        String HeroName = result.Substring(NextPos + 13, result.IndexOf("\"", NextPos) - NextPos - 13);
                        PlayersInformation[0].Add(HeroName);
                        //NextPos = ;
                        int Matchespos = result.IndexOf("Matches Played</div><div class=\"r-body\">", NextPos);
                        String Matches = result.Substring(Matchespos + 40, result.IndexOf("<", Matchespos + 40) - Matchespos - 40);
                        PlayersInformation[1].Add(Matches);

                        int WinRatepos = result.IndexOf("Win Rate</div><div class=\"r-body\">", NextPos);
                        String WinRate = result.Substring(WinRatepos + 34, result.IndexOf("<", WinRatepos + 34) - WinRatepos - 34);
                        PlayersInformation[2].Add(WinRate);

                        int KDApos = result.IndexOf("KDA Ratio</div><div class=\"r-body\">", NextPos);
                        String KDA = result.Substring(KDApos + 35, result.IndexOf("<", KDApos + 35) - KDApos - 35);
                        PlayersInformation[3].Add(KDA);
                        NextPos = result.IndexOf("matches?hero=", NextPos + 630);
                    }

                    int startpos2 = result2.IndexOf("<div class=\"r-table r-only-mobile-5 performances-overview\">");
                    if (startpos2 == -1)
                    {
                        PlayersInformation[0].Clear(); PlayersInformation[1].Clear(); PlayersInformation[2].Clear(); PlayersInformation[3].Clear();
                        PlayersInformation[0].Add("THIS PROFILE IS PRIVATE");
                    }
                    else if (startpos2 > -1)
                    {
                        int finishpos2 = result2.IndexOf("</article", startpos2);
                        result2 = result2.Substring(startpos2, finishpos2 - startpos2);
                        int NextPos2 = result2.IndexOf("data-link-to=\"&#47;matches&#47;", 0);
                        //textBox1.Text = result2;
                        PlayersInformation[0].Add(" "); PlayersInformation[1].Add(" "); PlayersInformation[2].Add(" "); PlayersInformation[3].Add(" ");
                        PlayersInformation[0].Add("Last Games:"); PlayersInformation[1].Add(" "); PlayersInformation[2].Add(" "); PlayersInformation[3].Add(" ");
                        PlayersInformation[0].Add("Hero"); PlayersInformation[1].Add("Result"); PlayersInformation[2].Add("Type"); PlayersInformation[3].Add("KDA");
                        while (NextPos2 > -1)
                        {
                            int HeroNamePos = result2.IndexOf("<a href=\"/heroes/", NextPos2);
                            String HeroName = result2.Substring(HeroNamePos + 17, result2.IndexOf("\"", HeroNamePos + 17) - HeroNamePos - 17);
                            PlayersInformation[0].Add(HeroName);

                            int ResultPos = result2.IndexOf("Result</div><div class=\"r-body\"><a class=\"", NextPos2);
                            String Result = result2.Substring(ResultPos + 42, result2.IndexOf("\"", ResultPos + 42) - ResultPos - 42);
                            PlayersInformation[1].Add(Result); 

                            int ResultDPos = result2.IndexOf("<time datetime=\"", NextPos2);
                            String ResultD = result2.Substring(ResultDPos + 16, result2.IndexOf("\"", ResultDPos + 16) - ResultDPos - 16);
                            PlayersInformation[1][PlayersInformation[1].Count-1] += "(" + ResultD + ")";

                            int TypePos = result2.IndexOf("Type</div><div class=\"r-body\">", NextPos2);
                            String TypeG = result2.Substring(TypePos + 30, result2.IndexOf("<", TypePos + 30) - TypePos - 30);
                            PlayersInformation[2].Add(TypeG);

                            int GameModePos = result2.IndexOf("<div class=\"subtext\">", TypePos);
                            String GameMode = result2.Substring(GameModePos + 21, result2.IndexOf("<", GameModePos + 21) - GameModePos - 21);
                            PlayersInformation[2][PlayersInformation[2].Count - 1] += "(" + GameMode + ")";

                            int KDApos = result2.IndexOf("\"kda-record\">", NextPos2);
                            String KDA = result2.Substring(KDApos + 13, result2.IndexOf("</span></span><div class=\"bar bar-default\"", KDApos + 13) - KDApos - 13);
                            for (int i = 0; i < 3; i++)
                            {
                                String substr = "<span class=\"value\">";
                                int n = KDA.IndexOf(substr);
                                KDA = KDA.Remove(n, substr.Length);
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                String substr = "</span>";
                                int n = KDA.IndexOf(substr);
                                KDA = KDA.Remove(n, substr.Length);
                            }
                            PlayersInformation[3].Add(KDA);

                            NextPos2 = result2.IndexOf("data-link-to=\"&#47;matches&#47;", NextPos2 + 630);

                        }
                    }
                }
                if (Repos[PID].RepoM != "-")
                {
                    PlayersInformation[0].Add("SteamID: " + Repos[PID].SteamId);
                    PlayersInformation[0].Add("Mark: " + Repos[PID].RepoM);
                }
                PlayersList_LoadedInformation[PID].Clear(); PlayersList_LoadedInformation[PID + 10].Clear(); 
                PlayersList_LoadedInformation[PID + 20].Clear(); PlayersList_LoadedInformation[PID + 30].Clear();

                for (int i = 0; i < PlayersInformation[0].Count - 1; i++)
                  PlayersList_LoadedInformation[PID].Add(PlayersInformation[0][i]);
                for (int i = 0; i < PlayersInformation[1].Count - 1; i++)
                    PlayersList_LoadedInformation[PID+10].Add(PlayersInformation[1][i]);
                for (int i = 0; i < PlayersInformation[2].Count - 1; i++)
                    PlayersList_LoadedInformation[PID + 20].Add(PlayersInformation[2][i]);
                for (int i = 0; i < PlayersInformation[3].Count - 1; i++)
                    PlayersList_LoadedInformation[PID + 30].Add(PlayersInformation[3][i]);
            }
        }


        class IniFile
        {
            string Path;

            [DllImport("kernel32")]
            static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

            [DllImport("kernel32")]
            static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

            public IniFile(string IniPath)
            {
                Path = new System.IO.FileInfo(IniPath).FullName.ToString();
            }

            public string Read(string Key, string Section = null)
            {
                var RetVal = new StringBuilder(255);
                GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
                return RetVal.ToString();
            }

            public void Write(string Key, string Value, string Section)
            {
                WritePrivateProfileString(Section, Key, Value, Path);
            }

            public void DeleteKey(string Key, string Section)
            {
                Write(Key, null, Section);
            }

            public void DeleteSection(string Section)
            {
                Write(null, null, Section);
            }

            public bool KeyExists(string Key, string Section = null)
            {
                return Read(Key, Section).Length > 0;
            }
        }

        static void Main()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            PlayerTextSize = Drawing.MeasureText("Вася Petro", "Arial", new Vector2(14), FontFlags.None);
            PlayerCaptionTextSize = Drawing.MeasureText("Players: ", "Arial", new Vector2(14), FontFlags.None);
            arrowRectangleSize = new Vector2(20);
            OverlayDrag = false;
            Game.OnWndProc += Game_OnWndProc;
            /*Events.OnClose += (sender, args) =>
            {
                Drawing.OnEndScene -= Drawing_OnDraw;
                Game.OnWndProc -= Game_OnWndProc;
            };*/
            for (int i = 0; i < 40; i++)
            {
                PlayersList_LoadedInformation[i] = new List<String>();
                PlayersList_LoadedInformation[i].Add("NONE");
                if(i < 4)
                {
                    PlayersInformation[i] = new List<String>();
                    PlayersInformation[i].Add("NONE");
                }
            }

            for (int i = 0; i < 10; i++)
            {
                PlayersList_HeroName.Add("NONE");
                PlayersList_LoadedSteamID.Add("NONE");
                PlayersList_PlayerName.Add("NONE");
                PlayersList_RWA.Add("NONE");
                PlayersList_PlayerName.Add("NONE");
            }
            SelectedPlayer = 0;

            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("DBKey", "Menu Hot Key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddToMainMenu();
            DisplayOverlay = true;
            if (!System.IO.File.Exists(filename))
            {
                var IniFile = new IniFile(filename);
            }

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;

            aTimer = new System.Timers.Timer(2500);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            if (FontText16 != null)
                FontText16.OnLostDevice();
            if (FontText18 != null)
                FontText18.OnLostDevice();
            if (FontText14 != null)
                FontText14.OnLostDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            if (FontText16 != null)
                FontText16.OnLostDevice();
            if (FontText18 != null)
                FontText18.OnLostDevice();
            if (FontText14 != null)
                FontText14.OnLostDevice();
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (MatchId != Game.MatchID)
            {
                for (int i = 0; i < 10; i++) PlayersList_LoadedSteamID[i] = "Loading...";
                if(Game.MatchID > 0)
                {
                    MatchId = Game.MatchID;
                    for (int i = 0; i < 10; i++)
                    {
                        PlayersList_PlayerName[i] = "Loading...";
                        PlayersList_RWA[i] = "NULL";
                        Repos[i].RepoM = "-";
                    }
                }
            }

            if (ObjectManager.LocalPlayer == null) return;

            foreach (var p in ObjectManager.GetEntities<Player>().Where(enemy => enemy != null).ToList())
            {
                if(p.ID > 9) continue;
                PlayersList_LoadedSteamID[p.ID] = "" + p.PlayerSteamID;
                PlayersList_PlayerName[p.ID] = "" + p.Name;
                Repos[p.ID].SteamId = p.PlayerSteamID;
                if (p.Hero != null)
                    PlayersList_HeroName[p.ID] = p.Hero.Name;
                else
                   PlayersList_HeroName[p.ID] = "null";

                if (PlayersList_RWA[p.ID] == "NULL")
                {
                    if (System.IO.File.Exists(filename))
                    {
                        var IniFile = new IniFile(filename);
                        if (IniFile.KeyExists("Mark", "" + p.PlayerSteamID))
                        {
                            var Mark = IniFile.Read("Mark", "" + p.PlayerSteamID);
                            var GamesPlayed = IniFile.Read("GamesPlayed", "" + p.PlayerSteamID);
                            Repos[p.ID].SteamId = p.PlayerSteamID;
                            Repos[p.ID].RepoM = Mark;
                            Repos[p.ID].GamesPlayed = Convert.ToInt32(GamesPlayed) + 1;

                            IniFile.Write("GamesPlayed", Repos[p.ID].GamesPlayed.ToString(), "" + p.PlayerSteamID);
                        }
                        else
                        {
                            Repos[p.ID].RepoM = "-";
                            Repos[p.ID].GamesPlayed = 0;
                            IniFile.Write("Mark", "Played", "" + p.PlayerSteamID);
                            IniFile.Write("GamesPlayed", "1", "" + p.PlayerSteamID);
                        }
                    }
                    else Repos[p.ID].RepoM = "-";
                    String text = "";
                    string steamid = p.PlayerSteamID.ToString();
                    if (p.PlayerSteamID < 10) steamid = "346713680";
                    var webRequest = WebRequest.Create("http://www.dotabuff.com/players/" + steamid + "/matches?date=3month&lobby_type=ranked_matchmaking&timezone=Etc%2FUTC");
                    ((HttpWebRequest)webRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0)";
                    webRequest.Method = "GET";
                    using (var response = webRequest.GetResponse())
                    using (var content = response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(content))
                    {
                        var strContent = reader.ReadToEnd();
                        text = strContent;
                    }
                    Console.WriteLine("Load SID{0} PID:{1}", steamid, p.ID);
                    int startpos = text.IndexOf("r-stats-grid r-stats-grid-padded");
                    if (startpos == -1)
                    {
                        PlayersList_RWA[p.ID] = "NONE";
                    }
                    else if (startpos > -1)
                    {
                        int finishpos = text.IndexOf("/article", startpos);
                        text = text.Substring(startpos, finishpos - startpos);
                        int NextPos = text.IndexOf("Matches</small></div><div class=\"kv\"><span class=\"color-stat-win\">", 0);
                        PlayersList_RWA[p.ID] = text.Substring(NextPos + 66, text.IndexOf("<", NextPos + 66) - NextPos - 66);
                        Console.WriteLine("Loaded SID{0} PID:{1} RWA: {2}", steamid, p.ID, PlayersList_RWA[p.ID]);
                    }
                }
            }
        }

        private static bool IsUnderBox()
        {
            return Utils.IsUnderRectangle(
                Game.MouseScreenPosition,
                OverlayPosition.X,
                OverlayPosition.Y,
                OverlaySize.X + 1,
                OverlaySize.Y + 1);
        }

        private static int PlayerSelected()
        {
            for(int i = 0; i < 10; i++)
            {
                if(Utils.IsUnderRectangle(Game.MouseScreenPosition,OverlayPosition.X + 1, OverlayPosition.Y + 55 + i * 20, 180, 20))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsUnderClose()
        {
            return Utils.IsUnderRectangle(
                Game.MouseScreenPosition,
                OverlayPosition.X + OverlaySize.X - 20,
                OverlayPosition.Y + 5,
                21,
                21);
        }

        private static bool IsUnderCaption()
        {
            return Utils.IsUnderRectangle(
                Game.MouseScreenPosition,
                OverlayPosition.X,
                OverlayPosition.Y,
                OverlaySize.X + 1,
                30 + 1);
        }

        private static bool IsUnderDecreaseArrow()
        {
           return Utils.IsUnderRectangle(
                    Game.MouseScreenPosition,
                    OverlayPosition.X + PlayerCaptionTextSize.X + 23,
                    OverlayPosition.Y + 32, 
                    arrowRectangleSize.X,
                    arrowRectangleSize.Y);
        }

        private static bool IsUnderIncreaseArrow()
        {
            return Utils.IsUnderRectangle(
                    Game.MouseScreenPosition,
                    OverlayPosition.X + PlayerCaptionTextSize.X + arrowRectangleSize.X + 40,
                    OverlayPosition.Y + 32,
                    arrowRectangleSize.X,
                    arrowRectangleSize.Y);
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || (ObjectManager.LocalHero != null && ObjectManager.LocalHero.IsAlive))
            {
                return;
            }
            if (ObjectManager.LocalPlayer != null && ObjectManager.LocalHero == null)
            {
                for (int i = 0; i < 10; i++)
                {
                    var text = string.Format("RWinRate: {1} | Games: {2}", i, PlayersList_RWA[i], Repos[i].GamesPlayed);
                    Color text_color;
                    switch (Repos[i].RepoM)
                    {
                        case "GoodGuy": { text_color = Color.Green; break; }
                        case "BadGuy": { text_color = Color.Red; break; }
                        case "Played": { text_color = Color.Yellow; break; }
                        default: text_color = Color.Blue; break;
                    }

                    FontText14.DrawText(null, text, Drawing.Width - 475, (Drawing.Height / 17) * 3 + (int)(Drawing.Height / 22.58) *
                        (
                            (i > 4)
                            ? (i + 1)
                            : i
                        ), text_color);
                }
            }
            if (!DisplayOverlay)  return;
            if (OverlayPosition.IsZero) OverlayPosition = new Vector2(50, 50);
            Color nameColor = !IsUnderBox() ? Color.Yellow : Color.White;

            Color nameColorAlpha = new Color((int)nameColor.R, nameColor.G, nameColor.B, 200);
            
            FontText18.DrawText(null, "DotaBuff", (int)OverlayPosition.X + 10, (int)OverlayPosition.Y + 5, nameColorAlpha);
            if (!IsUnderBox()) { return; }
            FontText16.DrawText(null, "Players: ", (int)OverlayPosition.X + 10, (int)OverlayPosition.Y + 30 + 5, Color.White);
            FontText18.DrawText(null, "X", (int)OverlayPosition.X + (int)OverlaySize.X - 20, (int)OverlayPosition.Y + 5, Color.Red);
            for (int i = 9; i >= 0; i--)
            {
                if(i == SelectedPlayer || SelectedPlayerByMouse == i)
                    FontText16.DrawText(null, PlayersList_PlayerName[i], (int)OverlayPosition.X + 10, (int)OverlayPosition.Y + 55 + i * 20 + 5, Color.White);
                else FontText14.DrawText(null, PlayersList_PlayerName[i], (int)OverlayPosition.X + 10, (int)OverlayPosition.Y + 55 + i * 20 + 5, Color.DarkGreen);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer].Count-1; i++)
            {
                FontText12.DrawText(null, PlayersList_LoadedInformation[SelectedPlayer][i], (int)OverlayPosition.X + 5 + 180, (int)OverlayPosition.Y + 55 + i * 10 + 5, Color.White);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer+10].Count - 1; i++)
            {
                FontText12.DrawText(null, PlayersList_LoadedInformation[SelectedPlayer+10][i], (int)OverlayPosition.X + 5 + 180+90, (int)OverlayPosition.Y + 55 + i * 10 + 5, Color.White);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer + 20].Count - 1; i++)
            {
                FontText12.DrawText(null, PlayersList_LoadedInformation[SelectedPlayer + 20][i], (int)OverlayPosition.X + 5 + 180 + 90+180, (int)OverlayPosition.Y + 55 + i * 10 + 5, Color.White);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer + 30].Count - 1; i++)
            {
                FontText12.DrawText(null, PlayersList_LoadedInformation[SelectedPlayer + 30][i], (int)OverlayPosition.X + 5 + 180 + 90 + 180+90, (int)OverlayPosition.Y + 55 + i * 10 + 5, Color.White);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!DisplayOverlay)
            {
                return;
            }

            if (OverlayPosition.IsZero) OverlayPosition = new Vector2(50,50);

            //FullBox
            Drawing.DrawRect(
                OverlayPosition,
                OverlaySize,
                new Color(20, 20, 20, !IsUnderBox() ? 50 : 100));

            //border
            Drawing.DrawRect(
                OverlayPosition,
                OverlaySize + new Vector2(1),
                !IsUnderBox() ? new Color(0, 0, 0, 100) : new Color(0, 0, 250, 255),
                true);

            //Caption
            Drawing.DrawRect(
                OverlayPosition,
                new Vector2(OverlaySize.X, 30),
                !IsUnderBox() ? new Color(20, 20, 20, 100) : new Color(0, 0, 200, 150));

            Color nameColor = !IsUnderBox() ? Color.Yellow : Color.White;

            Color nameColorAlpha = new Color((int)nameColor.R, nameColor.G, nameColor.B, 200);
            //TextCaption
            Drawing.DrawText(
                "DotaBuff",
                new Vector2(OverlayPosition.X + 10, OverlayPosition.Y + 5),
                new Vector2(18),
                nameColorAlpha,
                FontFlags.None);

            //Close
            Drawing.DrawRect(
                OverlayPosition + new Vector2(OverlaySize.X - 20, 5),
                IsUnderClose() ? new Vector2(20) : new Vector2(18),
                _texture);	

            if (!IsUnderBox())
            {
                return;
            }
			
			//DragLines
			if(IsUnderCaption())
			{
			    Drawing.DrawLine(Game.MouseScreenPosition, Game.MouseScreenPosition+new Vector2(0,25), Color.White);
                //Drawing.DrawLine(Game.MouseScreenPosition + new Vector2(0, 21), Game.MouseScreenPosition + new Vector2(0, 21), Color.Blue);
			}
			
            //Tables
            Drawing.DrawRect(
                OverlayPosition + new Vector2(1,30),
                new Vector2(180, 25),
                new Color(255, 255, 255, 50),
                true);
            Drawing.DrawRect(
                OverlayPosition + new Vector2(180, 30),
                new Vector2(OverlaySize.X-180, 25),
                new Color(255, 255, 255, 50),
                true);
            Drawing.DrawRect(
                OverlayPosition + new Vector2(1, 54),
                new Vector2(180, OverlaySize.Y-54),
                new Color(255, 255, 255, 50),
                true);

            for (int i = 9; i >= 0; i--)
            {
                Drawing.DrawText(
                    "+",
                    new Vector2(OverlayPosition.X+180 - 8 - 30, OverlayPosition.Y + 55 + i * 20 + 5),
                    (Repos[i].RepoM == "GoodGuy" || SelectedPlayerByMouse == i) ? new Vector2(16) : new Vector2(14),
                    Color.Green,
                    FontFlags.None);

                Drawing.DrawText(
                    "-",
                    new Vector2(OverlayPosition.X+180 - 8, OverlayPosition.Y + 55 + i * 20 + 5),
                    (Repos[i].RepoM == "BadGuy" || SelectedPlayerByMouse == i) ? new Vector2(16) : new Vector2(14),
                    Color.Red,
                    FontFlags.None);

                Drawing.DrawText(
                    PlayersList_PlayerName[i],
                    new Vector2(OverlayPosition.X + 10, OverlayPosition.Y + 55 + i * 20 + 5),
                    (i == SelectedPlayer || SelectedPlayerByMouse == i) ? new Vector2(16) : new Vector2(14),
                    (i == SelectedPlayer || SelectedPlayerByMouse == i) ? Color.White : Color.DarkGreen,
                    FontFlags.None);
            }

            Drawing.DrawText(
                    "Like/Ban",
                    new Vector2(OverlayPosition.X + 180 - 45 - 5, OverlayPosition.Y + 30 + 5),
                    new Vector2(16),
                    Color.White,
                    FontFlags.None);

            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer].Count - 1; i++)
            {
                Drawing.DrawText(
                    PlayersList_LoadedInformation[SelectedPlayer][i],
                    new Vector2(OverlayPosition.X + 5 + 180, OverlayPosition.Y + 55 + i * 10 + 5),
                    new Vector2(12),
                    (SelectedPlayerByMouse == SelectedPlayer) ? Color.DarkOrange : Color.White,
                    FontFlags.None);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer+10].Count - 1; i++)
            {
                Drawing.DrawText(
                    PlayersList_LoadedInformation[SelectedPlayer+10][i],
                    new Vector2(OverlayPosition.X + 5 + 180+90, OverlayPosition.Y + 55 + i * 10 + 5),
                    new Vector2(12),
                    (SelectedPlayerByMouse == SelectedPlayer) ? Color.DarkOrange : Color.White,
                    FontFlags.None);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer + 20].Count - 1; i++)
            {
                Drawing.DrawText(
                    PlayersList_LoadedInformation[SelectedPlayer + 20][i],
                    new Vector2(OverlayPosition.X + 5 + 180 + 90+180, OverlayPosition.Y + 55 + i * 10 + 5),
                    new Vector2(12),
                    (SelectedPlayerByMouse == SelectedPlayer) ? Color.DarkOrange : Color.White,
                    FontFlags.None);
            }
            for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer + 30].Count - 1; i++)
            {
                Drawing.DrawText(
                    PlayersList_LoadedInformation[SelectedPlayer + 30][i],
                    new Vector2(OverlayPosition.X + 5 + 180 + 90 + 180+90, OverlayPosition.Y + 55 + i * 10 + 5),
                    new Vector2(12),
                    (SelectedPlayerByMouse == SelectedPlayer) ? Color.DarkOrange : Color.White,
                    FontFlags.None);
            }
            Drawing.DrawRect(
                new Vector2(OverlayPosition.X + 1, OverlayPosition.Y + 55 + SelectedPlayer * 20),
                new Vector2(180, 20),
                new Color(0, 0, 200, 100));
            // 3rd line

            Drawing.DrawText(
                "Players: ",
                new Vector2(OverlayPosition.X + 10, OverlayPosition.Y + 30 + 5),
                new Vector2(16),
                Color.White,
                FontFlags.None);

            var alphaDecArrow = IsUnderDecreaseArrow() ? 25 : -25;

            RoundedRectangle(
                OverlayPosition.X + PlayerCaptionTextSize.X + 23,
                OverlayPosition.Y + 32,
                arrowRectangleSize.X,
                arrowRectangleSize.Y,
                4,
                new Color(100 + alphaDecArrow, 100 + alphaDecArrow, 100 + alphaDecArrow, 200 + alphaDecArrow / 2));

            Drawing.DrawRect(
                OverlayPosition + new Vector2(PlayerCaptionTextSize.X + 25, 35),
                new Vector2(16, 16),
                DecreaseArrow);

            Drawing.DrawText(
                SelectedPlayer.ToString(),
                OverlayPosition + new Vector2(PlayerCaptionTextSize.X + arrowRectangleSize.X + 30, 34),
                new Vector2(16),
                Color.White,
                FontFlags.None);

            var alphaIncArrow = IsUnderIncreaseArrow() ? 25 : -25;

            RoundedRectangle(
                OverlayPosition.X + PlayerCaptionTextSize.X + arrowRectangleSize.X + 40,
                OverlayPosition.Y + 32,
                arrowRectangleSize.X,
                arrowRectangleSize.Y,
                3,
                new Color(100 + alphaIncArrow, 100 + alphaIncArrow, 100 + alphaIncArrow, 200 + alphaIncArrow / 2));

            Drawing.DrawRect(
                OverlayPosition + new Vector2(PlayerCaptionTextSize.X + arrowRectangleSize.X + 43, 35),
                new Vector2(16, 16),
                IncreaseArrow);
        }

        public static void RoundedRectangle(float x, float y, float w, float h, int iSmooth, Color color)
        {
            var pt = new Vector2[4];

            // Get all corners 
            pt[0].X = x + (w - iSmooth);
            pt[0].Y = y + (h - iSmooth);

            pt[1].X = x + iSmooth;
            pt[1].Y = y + (h - iSmooth);

            pt[2].X = x + iSmooth;
            pt[2].Y = y + iSmooth;

            pt[3].X = x + w - iSmooth;
            pt[3].Y = y + iSmooth;

            // Draw cross 
            Drawing.DrawRect(new Vector2(x, y + iSmooth), new Vector2(w, h - (iSmooth * 2)), color);

            Drawing.DrawRect(new Vector2(x + iSmooth, y), new Vector2(w - (iSmooth * 2), h), color);

            float fDegree = 0;

            for (var i = 0; i < 4; i++)
            {
                for (var k = fDegree; k < fDegree + ((Math.PI * 2) / 4f); k += (float)(1 * (Math.PI / 180.0f)))
                {
                    // Draw quarter circles on every corner
                    Drawing.DrawLine(
                        new Vector2(pt[i].X, pt[i].Y),
                        new Vector2(pt[i].X + (float)(Math.Cos(k) * iSmooth), pt[i].Y + (float)(Math.Sin(k) * iSmooth)),
                        color);
                }

                fDegree += (float)(Math.PI * 2) / 4; // quarter circle offset 
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen && args.Msg == (ulong)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == SubMenu.Item("DBKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                DisplayOverlay = !DisplayOverlay;
            if (!DisplayOverlay)  return;
            if (args.Msg == (ulong)Utils.WindowsMessages.WM_LBUTTONUP)
            {
                if (OverlayDrag) { OverlayDrag = false; }
            }
            else if (args.Msg == (ulong)Utils.WindowsMessages.WM_MOUSEMOVE)
            {
                if (OverlayDrag) { OverlayPosition = Game.MouseScreenPosition - OverlayDragHeler; }
                if (IsUnderBox()) SelectedPlayerByMouse = PlayerSelected();
            }
            else if (args.Msg == (ulong)Utils.WindowsMessages.WM_LBUTTONDOWN)
            {
                if (IsUnderCaption()) { OverlayDragHeler = Game.MouseScreenPosition - OverlayPosition; OverlayDrag = !IsUnderClose(); DisplayOverlay = !IsUnderClose(); }
                else if (SelectedPlayerByMouse >= 0) { SelectedPlayer = SelectedPlayerByMouse; GetInformation(SelectedPlayer); }
                else if (IsUnderIncreaseArrow())
                {
                    SelectedPlayer++;
                    if (SelectedPlayer > 9) SelectedPlayer = 0;
                    GetInformation(SelectedPlayer);
                    for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer].Count - 1; i++)
                        Console.WriteLine("Line{0}: {1}",i,PlayersList_LoadedInformation[SelectedPlayer][i]);
                }
                else if (IsUnderDecreaseArrow())
                {
                    SelectedPlayer--;
                    if (SelectedPlayer < 0) SelectedPlayer = 9;
                    GetInformation(SelectedPlayer);
                    for (int i = 0; i < PlayersList_LoadedInformation[SelectedPlayer].Count - 1; i++)
                        Console.WriteLine("Line{0}: {1}", i, PlayersList_LoadedInformation[SelectedPlayer][i]);
                }
            }
        }
    }
}
