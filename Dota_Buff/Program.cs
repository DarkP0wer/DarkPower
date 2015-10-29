using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace Dota_Buff
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        #region CFG
        
        public static String filename = "Dota_Buff";
        public static string[] KeysName = new string[] { "SHIFT+5 (%)", "SHIFT+1 (!)" };
        public static ulong[] KeysValue = new ulong[] { '%', '!' };
        public static int OpenKey;
        public static Boolean IsFormClose;
        //**
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
                Path = new System.IO.FileInfo(IniPath+ ".ini").FullName.ToString();
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
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int MessageBox(int hWnd, String text,
                String caption, uint type);
            /*[System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);*/
        }

        static void Main(string[] args)
        {
            Game.OnWndProc += Game_OnGameWndProc;
            IsFormClose = false;
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
                label1 = new System.Windows.Forms.Label();
                linkLabel1 = new System.Windows.Forms.LinkLabel();
                linkLabel2 = new System.Windows.Forms.LinkLabel();
                button2 = new System.Windows.Forms.Button();
                radioButton1 = new System.Windows.Forms.RadioButton();
                radioButton3 = new System.Windows.Forms.RadioButton();
                comboBox1 = new System.Windows.Forms.ComboBox();
                textBox1 = new System.Windows.Forms.TextBox();
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
                textBox1.Location = new System.Drawing.Point(167, 23);
                textBox1.Multiline = true;
                textBox1.Name = "textBox1";
                textBox1.Size = new System.Drawing.Size(621, 365);
                textBox1.TabIndex = 14;
                // 
                // comboBox1
                // 
                comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                comboBox1.FormattingEnabled = true;
                comboBox1.Location = new System.Drawing.Point(79, 23);
                comboBox1.Name = "comboBox1";
                comboBox1.Size = new System.Drawing.Size(82, 21);
                comboBox1.TabIndex = 10;
                comboBox1.SelectedIndexChanged += new System.EventHandler(comboBox1_SelectedIndexChanged);
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
                // label1
                // 
                label1.AutoSize = true;
                label1.Location = new System.Drawing.Point(2, 26);
                label1.Name = "label1";
                label1.Size = new System.Drawing.Size(122, 13);
                label1.TabIndex = 5;
                label1.Text = "HotKey:";
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
                radioButton3.Location = new System.Drawing.Point(370, 6);
                radioButton3.Name = "radioButton3";
                radioButton3.Size = new System.Drawing.Size(277, 17);
                radioButton3.TabIndex = 17;
                radioButton3.Text = "Use default browser (open df browser in new window)";
                radioButton3.UseVisualStyleBackColor = true;
                radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
                // 
                // Form1
                // 
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                BackColor = System.Drawing.SystemColors.Highlight;
                ClientSize = new System.Drawing.Size(965, 426);
                ControlBox = false;
                Controls.Add(linkLabel2);
                Controls.Add(linkLabel1);
                Controls.Add(label1);
                Controls.Add(button1);
                Controls.Add(listBox2);
                Controls.Add(listBox1);
                Controls.Add(radioButton3);
                Controls.Add(radioButton1);
                Controls.Add(textBox1);
                Controls.Add(button2);
                Controls.Add(comboBox1);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                Name = "Form1";
                ShowIcon = false;
                Text = "DotaBuff by DarkPower";
                TopMost = true;
                Load += new System.EventHandler(Form1_Load);
                MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
                FormClosed += new System.Windows.Forms.FormClosedEventHandler(Form1_FormClosed);
                ResumeLayout(false);
                PerformLayout();
            }

            public System.Windows.Forms.ListBox listBox1;
            public System.Windows.Forms.ListBox listBox2;
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.RadioButton radioButton1;
            private System.Windows.Forms.RadioButton radioButton3;
            public System.Windows.Forms.ComboBox comboBox1;
            private System.Windows.Forms.TextBox textBox1;

            private void Form1_Load(object sender, EventArgs e)
            {
                Width = 900; Height = 400;
                IsFormClose = false;
                frm.comboBox1.Items.Clear();
                for (int i = 0; i < KeysName.Length; i++)
                {
                    frm.comboBox1.Items.Add(KeysName[i]);
                }
                if (System.IO.File.Exists(filename))
                {
                    var IniFile = new IniFile(filename);
                    var k = IniFile.Read("OpenKey", "HotKeys");
                    OpenKey = int.Parse(k);
                    frm.comboBox1.SelectedIndex = int.Parse(k);
                    Console.WriteLine("Load with file");
                    Console.WriteLine(OpenKey);
                }
                else
                {
                    var IniFile = new IniFile(filename);
                    IniFile.Write("OpenKey", "0", "HotKeys");
                    OpenKey = 0;
                    frm.comboBox1.SelectedIndex = 0;
                    //System.IO.File.SetAttributes(filename, System.IO.FileAttributes.System);
                }
            }

            private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
                try
                {
                    if (radioButton1.Checked)
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
                        int startpos = result.IndexOf("<div class=\"r-table r-only-mobile-5 heroes-overview\">");
                        if (startpos == -1) textBox1.Text = "THIS PROFILE IS PRIVATE";
                        else if (startpos > -1)
                        {
                            int finishpos = result.IndexOf("/article", startpos);
                            result = result.Substring(startpos, finishpos - startpos);
                            textBox1.Text = result;
                            int CountHero = 0;
                            int NextPos = result.IndexOf("matches?hero=", 0);
                            //\t\t\tMost played heros:\r\n
                            textBox1.Text = "Hero\t\tMatches\t\tWinRate\t\tKDA\r\n";
                            while (NextPos > -1)
                            {
                                CountHero++;
                                String HeroName = result.Substring(NextPos + 13, result.IndexOf("\"", NextPos) - NextPos - 13);
                                textBox1.Text = textBox1.Text + HeroName;
                                //NextPos = ;
                                int Matchespos = result.IndexOf("Matches Played</div><div class=\"r-body\">", NextPos);
                                String Matches = result.Substring(Matchespos + 40, result.IndexOf("<", Matchespos + 40) - Matchespos - 40);
                                textBox1.Text = textBox1.Text + ((HeroName.Length > 9) ? ("\t") : ("\t\t")) + Matches;

                                int WinRatepos = result.IndexOf("Win Rate</div><div class=\"r-body\">", NextPos);
                                String WinRate = result.Substring(WinRatepos + 34, result.IndexOf("<", WinRatepos + 34) - WinRatepos - 34);
                                textBox1.Text = textBox1.Text + "\t\t" + WinRate;

                                int KDApos = result.IndexOf("KDA Ratio</div><div class=\"r-body\">", NextPos);
                                String KDA = result.Substring(KDApos + 35, result.IndexOf("<", KDApos + 35) - KDApos - 35);
                                textBox1.Text = textBox1.Text + "\t\t" + KDA + "\r\n";

                                NextPos = result.IndexOf("matches?hero=", NextPos + 630);

                            }
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
                        System.Windows.Forms.MessageBox.Show("Error: " + e2.Source);
                    throw;
                }
            }

            private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
            {
                listBox1.SelectedIndex = listBox2.SelectedIndex;
            }

            private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (OpenKey != comboBox1.SelectedIndex)
                {
                    String str = "" + comboBox1.SelectedIndex;
                    var IniFile = new IniFile(filename);
                    IniFile.Write("OpenKey", str, "HotKeys");
                }
                OpenKey = comboBox1.SelectedIndex;
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

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame) return;
            try
            {
                Console.WriteLine(OpenKey);
                if (OpenKey > KeysValue.Length-1 || OpenKey < 0) { OpenKey = 0; Win32.MessageBox(0, "Your HotKey changed to SHIFT+5 (%)", "Dota_Buff", 0); }
                if (args.WParam == KeysValue[OpenKey])
                {
                    if (IsFormClose)
                    {
                        Win32.MessageBox(0, "You close form!\r\n Reload script for openning!", "Dota_Buff", 0);
                        return;
                    }
                    frm.Width = 800; frm.Height = 400;
                    frm.Show();
                    var enemies = ObjectMgr.GetEntities<Player>().Where(enemy => enemy != null).ToList();
                    frm.listBox1.Items.Clear();
                    frm.listBox2.Items.Clear(); ;
                    foreach (var enemy in enemies)
                    {
                        if (enemy == null || enemy.IsFakeClient) continue;
                        uint id = enemy.PlayerSteamID;
                        if (id > 0)
                        {
                            frm.listBox1.Items.Add(id);
                            frm.listBox2.Items.Add(enemy.Name);
                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    MessageBox.Show("Error: " + e.Source);
                throw;
            }
        }
    }
}
