using System;
using System.Linq;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using SharpDX;
using SharpDX.Direct3D9;

namespace Dota_Buff
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        #region CFG
        private static String version = "v1.4";
        private static readonly Ensage.Common.Menu.Menu SubMenu = new Ensage.Common.Menu.Menu("Dota Buff " + version, "DOTA BUFF", true);
        private static Boolean IsPlayersLoad;
        private static String filename = "Dota_Buff.ini";
        private static String[] LoadedInformation = new String[20];
        private static String[] LoadedSteamID = new String[20];
        private static String[] RWA = new String[20];

        private static String[] _HeroName = new String[20];
        private static String[] _PlayerName = new String[20];
        private static String GameIp;
        private static int players_count;
        public struct Repo
        {
            public String RepoM;
            public String RepoT;
            public int GamesPlayed;
            public uint SteamId;
        }
        public static Repo[] Repos = new Repo[20];
        private static Boolean IsFormClose;
        private static Font FontArray;
        private static System.Timers.Timer aTimer;
        #endregion


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

        public class Win32
        {
            [DllImportAttribute("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [DllImportAttribute("user32.dll")]
            public static extern bool ReleaseCapture();
            /*[System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);*/
            [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName,
                string lpWindowName);
            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
            public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
            {
                var clr = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(text, arguments);
                Console.ForegroundColor = clr;
            }
        }

        static void Main(string[] args)
        {
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("DBKey", "Menu hot key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddToMainMenu();
            IsFormClose = false;
            frm.comboBox2.SelectedIndex = 0;
            for (int i = 0; i < 20; i++)
            {
                RWA[i] = "NULL";
            }
            if (!System.IO.File.Exists(filename))
            {
                var IniFile = new IniFile(filename);
                IniFile.Write("TEST", "1", "TEST");
            }
            FontArray = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 12,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnWndProc += Game_OnGameWndProc;
            aTimer = new System.Timers.Timer(2500);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (GameIp != Game.IPAddress)
            {
                GameIp = Game.IPAddress;
                players_count = 0;
                frm.listBox1.Items.Clear();
                frm.listBox2.Items.Clear();
                for (int i = 0; i < 10; i++)
                {
                    frm.listBox1.Items.Add("Loading...");
                    frm.listBox2.Items.Add("Loading...");
                }
            }

            if (players_count < 11)
            {
                if (ObjectMgr.LocalPlayer != null)
                {
                    var ps = ObjectMgr.GetEntities<Player>().Where(enemy => enemy != null).ToList();
                    foreach (var p in ps)
                    {
                        players_count++;
                        frm.listBox1.Items[p.ID] = "" + p.PlayerSteamID;
                        frm.listBox2.Items[p.ID] = "" + p.Name;
                        _PlayerName[p.ID] = p.Name;
                        Repos[p.ID].SteamId = p.PlayerSteamID;
                        if (p.Hero != null)
                        {
                            _HeroName[p.ID] = p.Hero.Name;
                        }
                    }
                }
            }

            if (ObjectMgr.LocalPlayer != null && ObjectMgr.LocalPlayer.Hero == null)
            {
                IsPlayersLoad = true;
                for (int i = 0; i < 20; i++)
                {
                    Player p = null;
                    p = ObjectMgr.GetPlayerById((uint)i);
                    if (p == null)
                    {
                        RWA[i] = "Loading...";
                        Repos[i].RepoM = "-";
                        continue;
                    }
                    if (RWA[i] == "NULL")
                    {
                        if (System.IO.File.Exists(filename))
                        {
                            var IniFile = new IniFile(filename);
                            if (IniFile.KeyExists("Mark", "" + p.PlayerSteamID))
                            {
                                var Mark = IniFile.Read("Mark", "" + p.PlayerSteamID);
                                var RepoText = IniFile.Read("RepoText", "" + p.PlayerSteamID);
                                var GamesPlayed = IniFile.Read("GamesPlayed", "" + p.PlayerSteamID);
                                Repos[i].SteamId = p.PlayerSteamID;
                                Repos[i].RepoM = Mark;
                                Repos[i].RepoT = RepoText;
                                Repos[i].GamesPlayed = Convert.ToInt32(GamesPlayed) + 1;

                                IniFile.Write("GamesPlayed", Repos[i].GamesPlayed.ToString(), "" + p.PlayerSteamID);
                            }
                            else
                            {
                                Repos[i].RepoM = "-";
                                Repos[i].GamesPlayed = 0;
                                IniFile.Write("Mark", "Played", "" + p.PlayerSteamID);
                                IniFile.Write("RepoText", "None", "" + p.PlayerSteamID);
                                IniFile.Write("GamesPlayed", "1", "" + p.PlayerSteamID);
                            }
                        }
                        else Repos[i].RepoM = "-";
                        RWA[i] = "Loading inf...";
                        String text = "";
                        var webRequest = WebRequest.Create("http://www.dotabuff.com/players/" + p.PlayerSteamID + "/matches?date=patch_6.86s&hero=&skill_bracket=&lobby_type=ranked_matchmaking&game_mode=&region=&faction=&duration=&timezone=Etc%2FUTC");
                        ((HttpWebRequest)webRequest).UserAgent = ".NET Framework Example Client";
                        webRequest.Method = "GET";
                        using (var response = webRequest.GetResponse())
                        using (var content = response.GetResponseStream())
                        using (var reader = new System.IO.StreamReader(content))
                        {
                            var strContent = reader.ReadToEnd();
                            text = strContent;
                        }
                        int startpos = text.IndexOf("r-stats-grid r-stats-grid-padded");
                        if (startpos == -1)
                        {
                            RWA[p.ID] = "NONE";
                        }
                        else if (startpos > -1)
                        {
                            int finishpos = text.IndexOf("/article", startpos);
                            text = text.Substring(startpos, finishpos - startpos);
                            int NextPos = text.IndexOf("Matches</small></div><div class=\"kv\"><span class=\"color-stat-win\">", 0);
                            RWA[p.ID] = text.Substring(NextPos + 66, text.IndexOf("<", NextPos) - NextPos - 66);
                        }
                    }
                    else if (RWA[i] == "Loading...")
                    {
                        RWA[i] = "NULL";
                    }
                }
            }
            else
            {
                if (IsPlayersLoad)
                {
                    for (int i = 0; i < 20; i++) RWA[i] = "NULL";
                    IsPlayersLoad = false;
                }
            }
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                if (FontArray != null)
                    FontArray.OnLostDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            for (var i = 0; i <= 20; i++)
                if (FontArray != null)
                    FontArray.OnLostDevice();
        }

        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                listBox1 = new System.Windows.Forms.ListBox();
                listBox2 = new System.Windows.Forms.ListBox();
                button1 = new System.Windows.Forms.Button();
                linkLabel1 = new System.Windows.Forms.LinkLabel();
                linkLabel2 = new System.Windows.Forms.LinkLabel();
                button2 = new System.Windows.Forms.Button();
                radioButton1 = new System.Windows.Forms.RadioButton();
                radioButton3 = new System.Windows.Forms.RadioButton();
                textBox1 = new System.Windows.Forms.TextBox();
                this.button3 = new System.Windows.Forms.Button();
                this.comboBox2 = new System.Windows.Forms.ComboBox();
                this.textBox2 = new System.Windows.Forms.TextBox();
                SuspendLayout();
                // 
                // listBox1
                // 
                listBox1.Enabled = false;
                listBox1.FormattingEnabled = true;
                listBox1.Location = new System.Drawing.Point(12, 44);
                listBox1.Name = "listBox1";
                listBox1.Size = new System.Drawing.Size(149, 108);
                listBox1.TabIndex = 0;
                listBox1.SelectedIndexChanged += new System.EventHandler(listBox1_SelectedIndexChanged);
                // 
                // textBox1
                // 
                textBox1.Location = new System.Drawing.Point(167, 44);
                textBox1.Multiline = true;
                textBox1.Name = "textBox1";
                textBox1.Size = new System.Drawing.Size(621, 344);
                textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                textBox1.TabIndex = 14;
                // 
                // listBox2
                // 
                listBox2.FormattingEnabled = true;
                listBox2.Location = new System.Drawing.Point(12, 158);
                listBox2.Name = "listBox2";
                listBox2.Size = new System.Drawing.Size(149, 108);
                listBox2.TabIndex = 3;
                listBox2.SelectedIndexChanged += new System.EventHandler(listBox2_SelectedIndexChanged);
                //
                // button1
                // 
                button1.BackColor = System.Drawing.SystemColors.ControlLightLight;
                button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                button1.Location = new System.Drawing.Point(2, 2);
                button1.Name = "button1";
                button1.Size = new System.Drawing.Size(80, 21);
                button1.TabIndex = 4;
                button1.Text = "DB HIDE";
                button1.UseVisualStyleBackColor = true;
                button1.Click += new System.EventHandler(button1_Click);
                // 
                // linkLabel1
                // 
                linkLabel1.AutoSize = true;
                linkLabel1.LinkColor = System.Drawing.Color.White;
                linkLabel1.Location = new System.Drawing.Point(9, 316);
                linkLabel1.Name = "linkLabel1";
                linkLabel1.Size = new System.Drawing.Size(73, 13);
                linkLabel1.TabIndex = 6;
                linkLabel1.TabStop = true;
                linkLabel1.Text = "Forum Thread";
                linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
                // 
                // linkLabel2
                // 
                linkLabel2.AutoSize = true;
                linkLabel2.LinkColor = System.Drawing.Color.White;
                linkLabel2.Location = new System.Drawing.Point(9, 338);
                linkLabel2.Name = "linkLabel2";
                linkLabel2.Size = new System.Drawing.Size(89, 13);
                linkLabel2.TabIndex = 7;
                linkLabel2.TabStop = true;
                linkLabel2.Text = "GitHub Darp0wer";
                linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel2_LinkClicked);
                // 
                // button2
                //
                button2.BackColor = System.Drawing.SystemColors.ControlLightLight;
                button2.Location = new System.Drawing.Point(12, 364);
                button2.Name = "button2";
                button2.Size = new System.Drawing.Size(86, 24);
                button2.TabIndex = 8;
                button2.Text = "Close";
                button2.UseVisualStyleBackColor = true;
                button2.Click += new System.EventHandler(this.button2_Click);
                // 
                // radioButton1
                // 
                radioButton1.AutoSize = true;
                radioButton1.Checked = true;
                radioButton1.Location = new System.Drawing.Point(167, 6);
                radioButton1.Name = "radioButton1";
                radioButton1.Size = new System.Drawing.Size(197, 17);
                radioButton1.TabIndex = 15;
                radioButton1.TabStop = true;
                radioButton1.Text = "Parse Mode (only Most Played Hero)";
                radioButton1.UseVisualStyleBackColor = true;
                radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
                // 
                // radioButton3
                // 
                radioButton3.AutoSize = true;
                radioButton3.Location = new System.Drawing.Point(370, 4);
                radioButton3.Name = "radioButton3";
                radioButton3.Size = new System.Drawing.Size(119, 17);
                radioButton3.TabIndex = 17;
                radioButton3.Text = "Use default browser";
                radioButton3.UseVisualStyleBackColor = true;
                radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
                // 
                // button3
                // 
                button3.Location = new System.Drawing.Point(495, 1);
                button3.Name = "button3";
                button3.Size = new System.Drawing.Size(70, 20);
                button3.TabIndex = 19;
                button3.Text = "Mark";
                button3.UseVisualStyleBackColor = true;
                button3.Click += new System.EventHandler(this.button3_Click);
                // 
                // comboBox2
                // 
                comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                comboBox2.FormattingEnabled = true;
                comboBox2.Items.AddRange(new object[] {
            "BadGuy",
            "GoodGuy",
            "Played",
            "NONE"});
                comboBox2.Location = new System.Drawing.Point(495, 21);
                comboBox2.Name = "comboBox2";
                comboBox2.Size = new System.Drawing.Size(70, 21);
                comboBox2.TabIndex = 20;
                // 
                // textBox2
                // 
                textBox2.Location = new System.Drawing.Point(571, 12);
                textBox2.Name = "textBox2";
                textBox2.Size = new System.Drawing.Size(217, 20);
                textBox2.TabIndex = 21;
                textBox2.Text = "Some text";
                textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                // 
                // Form1
                // 
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                BackColor = System.Drawing.SystemColors.Highlight;
                ClientSize = new System.Drawing.Size(800, 400);
                ControlBox = false;
                Controls.Add(textBox2);
                Controls.Add(comboBox2);
                Controls.Add(button3);
                Controls.Add(linkLabel2);
                Controls.Add(linkLabel1);
                Controls.Add(button1);
                Controls.Add(listBox2);
                Controls.Add(listBox1);
                Controls.Add(radioButton3);
                Controls.Add(radioButton1);
                Controls.Add(textBox1);
                Controls.Add(button2);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                Name = "Form1";
                ShowIcon = false;
                Text = "DotaBuff by DarkPower";
                TopMost = true;
                this.Opacity = 0.85D;
                Load += new System.EventHandler(Form1_Load);
                MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
                FormClosed += new System.Windows.Forms.FormClosedEventHandler(Form1_FormClosed);
                ResumeLayout(false);
                PerformLayout();
            }

            public System.Windows.Forms.ListBox listBox1;
            public System.Windows.Forms.ListBox listBox2;
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.RadioButton radioButton1;
            private System.Windows.Forms.RadioButton radioButton3;
            private System.Windows.Forms.TextBox textBox1;
            private System.Windows.Forms.Button button3;
            public System.Windows.Forms.ComboBox comboBox2;
            public System.Windows.Forms.TextBox textBox2;

            private void Form1_Load(object sender, EventArgs e)
            {
                Width = 800; Height = 400;
                IsFormClose = false;
            }

            private string Win1251ToUTF8(string source)
            {

                Encoding utf8 = Encoding.GetEncoding("utf-8");
                Encoding win1251 = Encoding.GetEncoding("windows-1251");

                byte[] utf8Bytes = win1251.GetBytes(source);
                byte[] win1251Bytes = Encoding.Convert(win1251, utf8, utf8Bytes);
                source = win1251.GetString(win1251Bytes);
                return source;

            }

            internal static string UTF8ToWin1251(string sourceStr)
            {
                Encoding utf8 = Encoding.UTF8;
                Encoding win1251 = Encoding.GetEncoding("Windows-1251");

                byte[] utf8Bytes = utf8.GetBytes(sourceStr);
                byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
                return win1251.GetString(win1251Bytes);
            }

            private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
                try
                {
                    if (radioButton1.Checked)
                    {
                        if (listBox1.Items[listBox1.SelectedIndex].ToString() == "Loading...")
                        {
                            textBox1.Text = "This persson not loaded or disconnected!";
                            textBox1.Text += "\r\nPlayer Name : " + _PlayerName[listBox1.SelectedIndex];
                            textBox1.Text += "\r\nSteamid: " + Repos[listBox1.SelectedIndex].SteamId;
                            textBox1.Text += "\r\nHero: " + _HeroName[listBox1.SelectedIndex];

                            textBox1.Text = UTF8ToWin1251(textBox1.Text);
                            textBox2.Text = Win1251ToUTF8(textBox1.Text);
                            return;
                        }
                        if (LoadedSteamID[listBox1.SelectedIndex] == listBox1.Items[listBox1.SelectedIndex].ToString())
                        {
                            textBox1.Text = LoadedInformation[listBox1.SelectedIndex];
                        }
                        else
                        {

                            textBox1.Text = "Loading...";
                            String result = "";

                            var webRequest = WebRequest.Create("http://www.dotabuff.com/players/" + listBox1.Items[listBox1.SelectedIndex].ToString());
                            ((HttpWebRequest)webRequest).UserAgent = ".NET Framework Example Client";
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
                                textBox1.Text = "THIS PROFILE IS PRIVATE";
                            }
                            else if (startpos > -1)
                            {
                                int finishpos = result.IndexOf("/article", startpos);
                                result = result.Substring(startpos, finishpos - startpos);
                                int NextPos = result.IndexOf("matches?hero=", 0);
                                textBox1.Text = "Hero\t\t\tMatches\t\tWinRate\t\tKDA\r\n";
                                while (NextPos > -1)
                                {
                                    String HeroName = result.Substring(NextPos + 13, result.IndexOf("\"", NextPos) - NextPos - 13);
                                    textBox1.Text += HeroName;
                                    //NextPos = ;
                                    int Matchespos = result.IndexOf("Matches Played</div><div class=\"r-body\">", NextPos);
                                    String Matches = result.Substring(Matchespos + 40, result.IndexOf("<", Matchespos + 40) - Matchespos - 40);
                                    textBox1.Text += ((HeroName.Length > 9) ? ("\t\t") : ("\t\t\t")) + Matches;

                                    int WinRatepos = result.IndexOf("Win Rate</div><div class=\"r-body\">", NextPos);
                                    String WinRate = result.Substring(WinRatepos + 34, result.IndexOf("<", WinRatepos + 34) - WinRatepos - 34);
                                    textBox1.Text += "\t\t" + WinRate;

                                    int KDApos = result.IndexOf("KDA Ratio</div><div class=\"r-body\">", NextPos);
                                    String KDA = result.Substring(KDApos + 35, result.IndexOf("<", KDApos + 35) - KDApos - 35);
                                    textBox1.Text += "\t\t" + KDA + "\r\n";

                                    NextPos = result.IndexOf("matches?hero=", NextPos + 630);

                                }

                                int startpos2 = result2.IndexOf("<div class=\"r-table r-only-mobile-5 performances-overview\">");
                                if (startpos2 == -1)
                                {
                                    textBox1.Text += "THIS PROFILE IS PRIVATE";
                                }
                                else if (startpos2 > -1)
                                {
                                    int finishpos2 = result2.IndexOf("</article", startpos2);
                                    result2 = result2.Substring(startpos2, finishpos2 - startpos2);
                                    int NextPos2 = result2.IndexOf("data-link-to=\"&#47;matches&#47;", 0);
                                    //textBox1.Text = result2;
                                    textBox1.Text += "Last Games:\r\nHero\t\t\tResult\t\t\t\tType\t\t\tKDA\r\n";
                                    while (NextPos2 > -1)
                                    {
                                        int HeroNamePos = result2.IndexOf("<a href=\"/heroes/", NextPos2);
                                        String HeroName = result2.Substring(HeroNamePos + 17, result2.IndexOf("\"", HeroNamePos + 17) - HeroNamePos - 17);
                                        textBox1.Text += HeroName;
                                        int ResultPos = result2.IndexOf("Result</div><div class=\"r-body\"><a class=\"", NextPos2);
                                        String Result = result2.Substring(ResultPos + 42, result2.IndexOf("\"", ResultPos + 42) - ResultPos - 42);
                                        textBox1.Text += ((HeroName.Length > 9) ? ("\t\t") : ("\t\t\t")) + Result;

                                        int ResultDPos = result2.IndexOf("<time datetime=\"", NextPos2);
                                        String ResultD = result2.Substring(ResultDPos + 16, result2.IndexOf("\"", ResultDPos + 16) - ResultDPos - 16);
                                        textBox1.Text += "(" + ResultD + ")";

                                        int TypePos = result2.IndexOf("Type</div><div class=\"r-body\">", NextPos2);
                                        String TypeG = result2.Substring(TypePos + 30, result2.IndexOf("<", TypePos + 30) - TypePos - 30);
                                        textBox1.Text += "\t" + TypeG;

                                        int GameModePos = result2.IndexOf("<div class=\"subtext\">", TypePos);
                                        String GameMode = result2.Substring(GameModePos + 21, result2.IndexOf("<", GameModePos + 21) - GameModePos - 21);
                                        textBox1.Text += "(" + GameMode + ")";

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
                                        textBox1.Text += "\t\t" + KDA + "\r\n";

                                        NextPos2 = result2.IndexOf("data-link-to=\"&#47;matches&#47;", NextPos2 + 630);

                                    }
                                }
                            }
                            if (Repos[listBox1.SelectedIndex].RepoM != "-")
                            {
                                textBox1.Text += "\r\nSteamID: " + Repos[listBox1.SelectedIndex].SteamId;
                                textBox1.Text += "\r\nMark: " + Repos[listBox1.SelectedIndex].RepoM;
                                textBox1.Text += "\r\nText About: " + Repos[listBox1.SelectedIndex].RepoT;
                                textBox2.Text = Repos[listBox1.SelectedIndex].RepoT;
                            }
                            LoadedInformation[listBox1.SelectedIndex] = textBox1.Text;
                            LoadedSteamID[listBox1.SelectedIndex] = listBox1.Items[listBox1.SelectedIndex].ToString();

                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("http://www.dotabuff.com/players/" + listBox1.Items[listBox1.SelectedIndex].ToString());
                    }
                }
                catch (Exception e2)
                {
                    if (e2.Source != null)
                        Win32.PrintEncolored("Dota_Buff Error: " + e2.Source, ConsoleColor.Red);
                    throw;
                }
            }

            private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
            {
                listBox1.SelectedIndex = listBox2.SelectedIndex;
            }

            private void button1_Click(object sender, EventArgs e)
            {
                if (Width != 800)
                {
                    Width = 800; Height = 400;
                }
                else
                {
                    Width = 1; Height = 1;
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 10);
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 5);
                    Win32.SetForegroundWindow(Win32.FindWindow(null, "Dota 2"));
                }
            }

            private void button3_Click(object sender, EventArgs e)
            {
                if (listBox1.Items[listBox1.SelectedIndex].ToString() == "Loading...")
                {
                    Win32.PrintEncolored("Dota_Buff: Player not loaded...", ConsoleColor.Red);
                    var enemies = ObjectMgr.GetEntities<Hero>().Where(enemy => enemy != null).ToList();
                    foreach (var enemy in enemies)
                    {
                        if (_HeroName[listBox1.SelectedIndex] == enemy.Name && enemy.Player == null)
                        {
                            if (System.IO.File.Exists(filename))
                            {
                                var IniFile = new IniFile(filename);
                                IniFile.Write("Mark", comboBox2.Items[comboBox2.SelectedIndex].ToString(), Repos[listBox1.SelectedIndex].SteamId.ToString());
                                IniFile.Write("RepoText", frm.textBox2.Text, Repos[listBox1.SelectedIndex].SteamId.ToString());
                                Repos[listBox1.SelectedIndex].RepoM = comboBox2.Items[comboBox2.SelectedIndex].ToString();
                                Repos[listBox1.SelectedIndex].RepoT = frm.textBox2.Text;
                            }
                            else
                            {
                                Win32.PrintEncolored("Dota_Buff: File " + filename + " not founded!", ConsoleColor.Red);
                            }
                        }
                    }
                }
                else
                {
                    if (System.IO.File.Exists(filename))
                    {
                        var IniFile = new IniFile(filename);
                        IniFile.Write("Mark", comboBox2.Items[comboBox2.SelectedIndex].ToString(), listBox1.Items[listBox1.SelectedIndex].ToString());
                        IniFile.Write("RepoText", frm.textBox2.Text, listBox1.Items[listBox1.SelectedIndex].ToString());
                        Repos[listBox1.SelectedIndex].RepoM = comboBox2.Items[comboBox2.SelectedIndex].ToString();
                        Repos[listBox1.SelectedIndex].RepoT = frm.textBox2.Text;
                    }
                    else
                    {
                        Win32.PrintEncolored("Dota_Buff: File " + filename + " not founded!", ConsoleColor.Red);
                    }
                }
            }

            private void button2_Click(object sender, EventArgs e)
            {
                IsFormClose = true;
                Close();
            }

            private void Form1_FormClosed(object sender, FormClosedEventArgs e)
            {
                IsFormClose = true;
            }

            private void Form1_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Win32.ReleaseCapture();
                    Win32.SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }

            private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                System.Diagnostics.Process.Start("https://github.com/DarkP0wer/DarkPower");
            }

            private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                System.Diagnostics.Process.Start("https://www.joduska.me/forum/topic/137479-dota-buff/");
            }

            private void radioButton1_CheckedChanged(object sender, EventArgs e)
            {
                textBox1.Visible = true;
            }

            private void radioButton3_CheckedChanged(object sender, EventArgs e)
            {
                textBox1.Visible = false;
            }

        }


        static Form1 frm = new Form1();

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
            {
                return;
            }
            if (IsPlayersLoad)
            {
                for (int i = 0; i < 10; i++)
                {
                    var text = string.Format("RWinRate: {1} | Games: {2}", i, RWA[i], Repos[i].GamesPlayed);
                    Color text_color;
                    switch (Repos[i].RepoM)
                    {
                        case "GoodGuy":
                            {
                                text_color = Color.Green;
                                break;
                            }
                        case "BadGuy":
                            {
                                text_color = Color.Red;
                                break;
                            }
                        case "Played":
                            {
                                text_color = Color.Yellow;
                                break;
                            }
                        default:
                            text_color = Color.Blue;
                            break;
                    }

                    FontArray.DrawText(null, text, Drawing.Width - 475, (Drawing.Height / 17) * 3 + (int)(Drawing.Height / 22.58) *
                        (
                            (i > 4)
                            ? (i + 1)
                            : i
                        )
                        , text_color);
                }
            }
        }

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame) return;
            try
            {
                if (args.Msg == 0x0101 && args.WParam == SubMenu.Item("DBKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                {
                    if (IsFormClose)
                    {
                        Win32.PrintEncolored("Dota_Buff: You close form! Reload script for openning!", ConsoleColor.Red);
                        return;
                    }
                    frm.Width = 800; frm.Height = 400;
                    frm.Show();
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    Win32.PrintEncolored("Dota_Buff Error: " + e.Source, ConsoleColor.Red);
                throw;
            }
        }
    }
}
