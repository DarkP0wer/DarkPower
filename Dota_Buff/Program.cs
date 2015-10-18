using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dota_Buff
{
    class Program
    {

        static void Main(string[] args)
        {
            Game.OnWndProc += Game_OnGameWndProc;
        }

        public class ClipCursor
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                //listBox1 = new System.Windows.Forms.ListBox();
                //listBox2 = new System.Windows.Forms.ListBox();
                webBrowser1 = new System.Windows.Forms.WebBrowser();
                this.comboBox1 = new System.Windows.Forms.ComboBox();
                this.comboBox2 = new System.Windows.Forms.ComboBox();
                button1 = new System.Windows.Forms.Button();
                label1 = new System.Windows.Forms.Label();
                linkLabel1 = new System.Windows.Forms.LinkLabel();
                linkLabel2 = new System.Windows.Forms.LinkLabel();
                SuspendLayout();
                /*// 
                // listBox1
                // 
                listBox1.Enabled = false;
                listBox1.FormattingEnabled = true;
                listBox1.Location = new System.Drawing.Point(12, 29);
                listBox1.Name = "listBox1";
                listBox1.Size = new System.Drawing.Size(149, 108);
                listBox1.TabIndex = 0;
                listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
                // */
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
                /*// listBox2
                // 
                listBox2.FormattingEnabled = true;
                listBox2.Location = new System.Drawing.Point(12, 143);
                listBox2.Name = "listBox2";
                listBox2.Size = new System.Drawing.Size(149, 108);
                listBox2.TabIndex = 3;
                listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
                // */
                // 
                // comboBox1
                // 
                comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                comboBox1.FormattingEnabled = true;
                comboBox1.Location = new System.Drawing.Point(12, 29);
                comboBox1.Name = "comboBox1";
                comboBox1.Size = new System.Drawing.Size(121, 21);
                comboBox1.TabIndex = 8;
                comboBox1.Enabled = false;
                // 
                // comboBox2
                // 
                comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                comboBox2.FormattingEnabled = true;
                comboBox2.Location = new System.Drawing.Point(12, 143);
                comboBox2.Name = "comboBox2";
                comboBox2.Size = new System.Drawing.Size(121, 21);
                comboBox2.TabIndex = 9;
                // 
                // button1
                // 
                button1.BackColor = System.Drawing.SystemColors.ControlLightLight;
                button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                button1.Location = new System.Drawing.Point(2, 2);
                button1.Name = "button1";
                button1.Size = new System.Drawing.Size(38, 21);
                button1.TabIndex = 4;
                button1.Text = "DB";
                button1.UseVisualStyleBackColor = true;
                button1.Click += new System.EventHandler(this.button1_Click);
                // 
                // label1
                // 
                label1.AutoSize = true;
                label1.Location = new System.Drawing.Point(39, 6);
                label1.Name = "label1";
                label1.Size = new System.Drawing.Size(122, 13);
                label1.TabIndex = 5;
                label1.Text = "In Game Press SHIFT+1";
                // 
                // linkLabel1
                // 
                linkLabel1.AutoSize = true;
                linkLabel1.LinkColor = System.Drawing.Color.White;
                linkLabel1.Location = new System.Drawing.Point(12, 263);
                linkLabel1.Name = "linkLabel1";
                linkLabel1.Size = new System.Drawing.Size(73, 13);
                linkLabel1.TabIndex = 6;
                linkLabel1.TabStop = true;
                linkLabel1.Text = "Forum Thread";
                linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                // 
                // linkLabel2
                // 
                linkLabel2.AutoSize = true;
                linkLabel2.LinkColor = System.Drawing.Color.White;
                linkLabel2.Location = new System.Drawing.Point(12, 288);
                linkLabel2.Name = "linkLabel2";
                linkLabel2.Size = new System.Drawing.Size(89, 13);
                linkLabel2.TabIndex = 7;
                linkLabel2.TabStop = true;
                linkLabel2.Text = "GitHub Darp0wer";
                linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
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
                Controls.Add(comboBox2);
                Controls.Add(webBrowser1);
                Controls.Add(comboBox1);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                Name = "Form1";
                ShowIcon = false;
                Text = "DotaBuff by DarkPower";
                TopMost = true;
                FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
                Load += new System.EventHandler(this.Form1_Load);
                MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                ResumeLayout(false);
                PerformLayout();
            }

            //public System.Windows.Forms.ListBox listBox1;
            //public System.Windows.Forms.ListBox listBox2;
            private System.Windows.Forms.WebBrowser webBrowser1;
            public System.Windows.Forms.ComboBox comboBox1;
            public System.Windows.Forms.ComboBox comboBox2;
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;

            private void Form1_Load(object sender, EventArgs e)
            {
                Width = 900; Height = 400;
            }

            private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
            {
                webBrowser1.Navigate("http://www.dotabuff.com/players/" + comboBox1.Items[comboBox1.SelectedIndex].ToString());
            }

            private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
            {
                comboBox1.SelectedIndex = comboBox2.SelectedIndex;
            }

            private void Form1_FormClosing(object sender, FormClosingEventArgs e)
            {
                e.Cancel = true;
            }

            private void button1_Click(object sender, EventArgs e)
            {
                if (Width != 900)
                {
                    Width = 900; Height = 400;
                }
                else
                {
                    Width = 42; Height = 30;
                }
            }

            private void Form1_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
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
                    frm.Width = 900; frm.Height = 400;
                    frm.Show();
                    var player = ObjectMgr.LocalPlayer;
                    if (player == null) return;
                    var enemies = ObjectMgr.GetEntities<Player>().Where(enemy => enemy != null).ToList();
                    /*string[] lines = new string[1];
                    lines[0] = "Name \tID";*/
                    frm.comboBox1.Items.Clear();
                    frm.comboBox2.Items.Clear(); ;
                    foreach (var enemy in enemies)
                    {
                        if (enemy == null || !enemy.IsValid) continue;
                        uint id = enemy.PlayerSteamID;
                        if (id > 0)
                        {
                            //Console.WriteLine(enemy.Name + "\t\t" + id);
                            //Array.Resize<string>(ref lines, lines.Length + 1);
                            //lines[lines.Length-1] = enemy.Name  + "\t\t" + id;
                            frm.comboBox1.Items.Add(id);
                            frm.comboBox2.Items.Add(enemy.Name);
                        }
                    }
                    //System.IO.File.WriteAllLines("Info.txt", lines);
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    MessageBox.Show("Error: {0}", e.Source);
                throw;
            }
        }
    }
}
