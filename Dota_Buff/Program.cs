using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Runtime.InteropServices;
//using System.Text;

namespace Dota_Buff
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        #region CFG
        /*
        public static String filename = "Dota_Buff.ini";
        public static string[] KeysName = new string[] { "SHIFT+1 (!)", "SHIFT+5 (%)" };
        public static char[] KeysValue = new char[] { '!', '%' };
        public static char OpenKey;*/
        public static Boolean IsFormClose;
        //**
        #endregion


        /*class IniFile
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
        }*/

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
            frm.comboBox1.Items.Clear();
            /*for (int i = 0; i < KeysName.Length; i++)
            {
                frm.comboBox1.Items.Add(KeysName[i]);
            }*/
            /*if (System.IO.File.Exists(filename))
            {
                var IniFile = new IniFile(filename);
                var k = IniFile.Read("OpenKey", "HotKeys");
                OpenKey = KeysValue[int.Parse(k)];
                frm.comboBox1.SelectedIndex = int.Parse(k);
            }
            else
            {
                var IniFile = new IniFile(filename);
                IniFile.Write("OpenKey", "0", "HotKeys");
                OpenKey = KeysValue[0];
                frm.comboBox1.SelectedIndex = 0;
                //System.IO.File.SetAttributes(filename, System.IO.FileAttributes.System);
            }*/
            //OpenKey = KeysValue[0];
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
                this.webBrowser1 = new System.Windows.Forms.WebBrowser();
                button1 = new System.Windows.Forms.Button();
                label1 = new System.Windows.Forms.Label();
                linkLabel1 = new System.Windows.Forms.LinkLabel();
                linkLabel2 = new System.Windows.Forms.LinkLabel();
                button2 = new System.Windows.Forms.Button();
                checkBox1 = new System.Windows.Forms.CheckBox();
                comboBox1 = new System.Windows.Forms.ComboBox();
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
                // webBrowser1
                // 
                webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                webBrowser1.Location = new System.Drawing.Point(167, 6);
                webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
                webBrowser1.Name = "webBrowser1";
                webBrowser1.ScriptErrorsSuppressed = true;
                webBrowser1.Size = new System.Drawing.Size(786, 408);
                webBrowser1.TabIndex = 2;
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
                // checkBox1
                // 
                checkBox1.AutoSize = true;
                checkBox1.Location = new System.Drawing.Point(12, 272);
                checkBox1.Name = "checkBox1";
                checkBox1.Size = new System.Drawing.Size(120, 17);
                checkBox1.TabIndex = 9;
                checkBox1.Text = "Use default browser";
                checkBox1.UseVisualStyleBackColor = true;
                checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
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
                Controls.Add(this.webBrowser1);
                Controls.Add(listBox1);
                Controls.Add(checkBox1);
                Controls.Add(button2);
                Controls.Add(comboBox1);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                Name = "Form1";
                ShowIcon = false;
                Text = "DotaBuff by DarkPower";
                TopMost = true;
                Load += new System.EventHandler(Form1_Load);
                MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
                ResumeLayout(false);
                PerformLayout();
            }

            public System.Windows.Forms.ListBox listBox1;
            public System.Windows.Forms.ListBox listBox2;
            private System.Windows.Forms.WebBrowser webBrowser1;
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.CheckBox checkBox1;
            public System.Windows.Forms.ComboBox comboBox1;

            private void Form1_Load(object sender, EventArgs e)
            {
                Width = 900; Height = 400;
            }

            private void checkBox1_CheckedChanged(object sender, EventArgs e)
            {
                if (checkBox1.Checked)
                {
                    webBrowser1.Visible = false;
                }
                else
                {
                    webBrowser1.Visible = true;
                }
            }

            private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
                try
                {
                    if (checkBox1.Checked)
                    {
                        System.Diagnostics.Process.Start("http://www.dotabuff.com/players/" + listBox1.Items[listBox1.SelectedIndex].ToString());
                    }
                    else
                    {
                        webBrowser1.Navigate("http://www.dotabuff.com/players/" + listBox1.Items[listBox1.SelectedIndex].ToString());
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
                /*if (OpenKey != KeysValue[comboBox1.SelectedIndex])
                {
                    String str = "" + comboBox1.SelectedIndex;
                    var IniFile = new IniFile(filename);
                    IniFile.Write("OpenKey", str, "HotKeys");
                }
                OpenKey = KeysValue[comboBox1.SelectedIndex];*/
            }

            private void button1_Click(object sender, EventArgs e)
            {
                if (Width != 900)
                {
                    Width = 900; Height = 400;
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

        }

    
        static Form1 frm = new Form1();

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame || frm == null) return;
            try
            {
                if (args.WParam == '!')
                {
                    if (IsFormClose)
                    {
                        Win32.MessageBox(0, "You close form!\r\n Reload script for openning!", "Dota_Buff", 0);
                        return;
                    }
                    frm.Width = 900; frm.Height = 400;
                    frm.Show();
                    var player = ObjectMgr.LocalPlayer;
                    if (player == null) return;
                    var enemies = ObjectMgr.GetEntities<Player>().Where(enemy => enemy != null).ToList();
                    /*string[] lines = new string[1];
                    lines[0] = "Name \tID";*/
                    frm.listBox1.Items.Clear();
                    frm.listBox2.Items.Clear(); ;
                    foreach (var enemy in enemies)
                    {
                        if (enemy == null || enemy.IsFakeClient) continue;
                        uint id = enemy.PlayerSteamID;
                        if (id > 0)
                        {
                            //Console.WriteLine(enemy.Name + "\t\t" + id);
                            //Array.Resize<string>(ref lines, lines.Length + 1);
                            //lines[lines.Length-1] = enemy.Name  + "\t\t" + id;
                            frm.listBox1.Items.Add(id);
                            frm.listBox2.Items.Add(enemy.Name);
                        }
                    }
                    //System.IO.File.WriteAllLines("Info.txt", lines);
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
