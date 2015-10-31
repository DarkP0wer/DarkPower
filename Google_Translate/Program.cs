using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Windows.Input;

namespace Translate
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public static Boolean IsSayTeam;
        public static Boolean ExitGUI;

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
            [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName,
                string lpWindowName);
            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
        }

        static void Main(string[] args)
        {
            frm.comboBox1.SelectedIndex = 0;
            frm.comboBox2.SelectedIndex = 8;
            Game.OnWndProc += Game_OnGameWndProc;
            ExitGUI = false;
        }

        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.textBox1 = new System.Windows.Forms.TextBox();
                this.textBox2 = new System.Windows.Forms.TextBox();
                this.button1 = new System.Windows.Forms.Button();
                this.comboBox1 = new System.Windows.Forms.ComboBox();
                this.comboBox2 = new System.Windows.Forms.ComboBox();
                this.textBox3 = new System.Windows.Forms.TextBox();
                this.checkBox1 = new System.Windows.Forms.CheckBox();
                this.button2 = new System.Windows.Forms.Button();
                this.linkLabel1 = new System.Windows.Forms.LinkLabel();
                this.linkLabel2 = new System.Windows.Forms.LinkLabel();
                this.checkBox2 = new System.Windows.Forms.CheckBox();
                this.label1 = new System.Windows.Forms.Label();
                this.checkBox3 = new System.Windows.Forms.CheckBox();
                this.label2 = new System.Windows.Forms.Label();
                this.SuspendLayout();
                // 
                // textBox1
                // 
                this.textBox1.Location = new System.Drawing.Point(12, 53);
                this.textBox1.Multiline = false;
                this.textBox1.Name = "textBox1";
                this.textBox1.Size = new System.Drawing.Size(241, 32);
                this.textBox1.TabIndex = 14;
                this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
                // 
                // textBox2
                // 
                this.textBox2.Location = new System.Drawing.Point(259, 53);
                this.textBox2.Multiline = true;
                this.textBox2.Name = "textBox2";
                this.textBox2.Size = new System.Drawing.Size(236, 32);
                this.textBox2.TabIndex = 15;
                // 
                // button1
                // 
                this.button1.Location = new System.Drawing.Point(12, 91);
                this.button1.Name = "button1";
                this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
                this.button1.Size = new System.Drawing.Size(73, 19);
                this.button1.TabIndex = 16;
                this.button1.Text = "Translate";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Click += new System.EventHandler(this.button1_Click);
                this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button1_MouseDown);
                // 
                // comboBox1
                // 
                this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.comboBox1.FormattingEnabled = true;
                this.comboBox1.Cursor = System.Windows.Forms.Cursors.Hand;
                this.comboBox1.Items.AddRange(new object[] {
            "en",
            "ar",
            "be",
            "es",
            "it",
            "zh-CN",
            "de",
            "ru",
            "uk",
            "fr",
            "ja"});
                this.comboBox1.Location = new System.Drawing.Point(38, 27);
                this.comboBox1.Name = "comboBox1";
                this.comboBox1.Size = new System.Drawing.Size(47, 21);
                this.comboBox1.TabIndex = 17;
                // 
                // comboBox2
                // 
                this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.comboBox2.FormattingEnabled = true;
                this.comboBox2.Cursor = System.Windows.Forms.Cursors.Hand;
                this.comboBox2.Items.AddRange(new object[] {
            "en",
            "ar",
            "be",
            "es",
            "it",
            "zh-CN",
            "de",
            "ru",
            "uk",
            "fr",
            "ja"});
                this.comboBox2.Location = new System.Drawing.Point(132, 27);
                this.comboBox2.Name = "comboBox2";
                this.comboBox2.Size = new System.Drawing.Size(51, 21);
                this.comboBox2.TabIndex = 18;
                // 
                // textBox3
                // 
                this.textBox3.Location = new System.Drawing.Point(259, 91);
                this.textBox3.Multiline = true;
                this.textBox3.Name = "textBox3";
                this.textBox3.Size = new System.Drawing.Size(236, 42);
                this.textBox3.TabIndex = 19;
                // 
                // checkBox1
                // 
                this.checkBox1.AutoSize = true;
                this.checkBox1.Cursor = System.Windows.Forms.Cursors.Hand;
                this.checkBox1.Location = new System.Drawing.Point(12, 116);
                this.checkBox1.Name = "checkBox1";
                this.checkBox1.Size = new System.Drawing.Size(133, 17);
                this.checkBox1.TabIndex = 20;
                this.checkBox1.Text = "Translate translated :D";
                this.checkBox1.UseVisualStyleBackColor = true;
                // 
                // button2
                // 
                this.button2.Location = new System.Drawing.Point(91, 28);
                this.button2.Name = "button2";
                this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
                this.button2.Size = new System.Drawing.Size(35, 21);
                this.button2.TabIndex = 21;
                this.button2.Text = "<=>";
                this.button2.UseVisualStyleBackColor = true;
                this.button2.Click += new System.EventHandler(this.button2_Click);
                // 
                // linkLabel1
                // 
                this.linkLabel1.AutoSize = true;
                this.linkLabel1.Location = new System.Drawing.Point(9, 156);
                this.linkLabel1.Name = "linkLabel1";
                this.linkLabel1.Size = new System.Drawing.Size(36, 13);
                this.linkLabel1.TabIndex = 23;
                this.linkLabel1.TabStop = true;
                this.linkLabel1.Text = "Forum";
                this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                // 
                // linkLabel2
                // 
                this.linkLabel2.AutoSize = true;
                this.linkLabel2.Location = new System.Drawing.Point(9, 138);
                this.linkLabel2.Name = "linkLabel2";
                this.linkLabel2.Size = new System.Drawing.Size(96, 13);
                this.linkLabel2.TabIndex = 24;
                this.linkLabel2.TabStop = true;
                this.linkLabel2.Text = "GitHub DarkP0wer";
                this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
                // 
                // checkBox2
                // 
                this.checkBox2.AutoSize = true;
                this.checkBox2.Checked = false;
                this.checkBox2.Cursor = System.Windows.Forms.Cursors.Hand;
                this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
                this.checkBox2.Location = new System.Drawing.Point(91, 93);
                this.checkBox2.Name = "checkBox2";
                this.checkBox2.Size = new System.Drawing.Size(116, 17);
                this.checkBox2.TabIndex = 25;
                this.checkBox2.Text = "Automatically Send";
                this.checkBox2.UseVisualStyleBackColor = true;
                // 
                // label1
                // 
                this.label1.AutoSize = true;
                this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
                this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.label1.ForeColor = System.Drawing.Color.Yellow;
                this.label1.Location = new System.Drawing.Point(12, 9);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(205, 13);
                this.label1.TabIndex = 26;
                this.label1.Text = "▲ Google Translate By DarkPower";
                this.label1.Click += new System.EventHandler(this.label1_Click);
                // 
                // checkBox3
                // 
                this.checkBox3.AutoSize = true;
                this.checkBox3.Checked = true;
                this.checkBox3.Cursor = System.Windows.Forms.Cursors.Hand;
                this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
                this.checkBox3.Location = new System.Drawing.Point(151, 116);
                this.checkBox3.Name = "checkBox3";
                this.checkBox3.Size = new System.Drawing.Size(100, 17);
                this.checkBox3.TabIndex = 27;
                this.checkBox3.Text = "Auto minimazed";
                this.checkBox3.UseVisualStyleBackColor = true;
                // 
                // label2
                // 
                this.label2.AutoSize = true;
                this.label2.Location = new System.Drawing.Point(189, 35);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(294, 13);
                this.label2.TabIndex = 28;
                this.label2.Text = "Click Right Mouse Button on \"Translate\" for send to the chat";
                // 
                // Form1
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.BackColor = System.Drawing.SystemColors.Highlight;
                this.ClientSize = new System.Drawing.Size(505, 175);
                this.ControlBox = false;
                this.Controls.Add(this.label2);
                this.Controls.Add(this.checkBox3);
                this.Controls.Add(this.checkBox2);
                this.Controls.Add(this.linkLabel2);
                this.Controls.Add(this.linkLabel1);
                this.Controls.Add(this.button2);
                this.Controls.Add(this.checkBox1);
                this.Controls.Add(this.textBox3);
                this.Controls.Add(this.comboBox2);
                this.Controls.Add(this.comboBox1);
                this.Controls.Add(this.button1);
                this.Controls.Add(this.textBox2);
                this.Controls.Add(this.textBox1);
                this.Controls.Add(this.label1);
                this.Cursor = System.Windows.Forms.Cursors.SizeAll;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Name = "Form1";
                this.Opacity = 0.75D;
                this.ShowIcon = false;
                this.Text = "Google Translate by DarkPower";
                this.TopMost = true;
                this.Load += new System.EventHandler(this.Form1_Load);
                this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                this.ResumeLayout(false);
                this.PerformLayout();

            }

            public System.Windows.Forms.TextBox textBox1;
            private System.Windows.Forms.TextBox textBox2;
            private System.Windows.Forms.Button button1;
            public System.Windows.Forms.ComboBox comboBox1;
            public System.Windows.Forms.ComboBox comboBox2;
            private System.Windows.Forms.TextBox textBox3;
            private System.Windows.Forms.CheckBox checkBox1;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            public System.Windows.Forms.CheckBox checkBox2;
            public System.Windows.Forms.Label label1;
            private System.Windows.Forms.CheckBox checkBox3;
            private System.Windows.Forms.Label label2;

            private void Form1_Load(object sender, EventArgs e)
            {
                Width = 900; Height = 400;
            }

            private void button1_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (checkBox1.Checked) Game.ExecuteCommand(((IsSayTeam)?"say_team ":"say ") + textBox3.Text);
                    else Game.ExecuteCommand(((IsSayTeam) ? "say_team " : "say ") + textBox2.Text);
                    if (checkBox3.Checked)
                    {
                        Width = 132;
                        Height = 28;
                        Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 10);
                        Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 5);
                        Win32.SetForegroundWindow(Win32.FindWindow(null, "Dota 2"));
                    }
                }
            }

            private void button1_Click(object sender, EventArgs e)
            {
                string result = translate(textBox1.Text, comboBox1.Items[comboBox1.SelectedIndex].ToString(), comboBox2.Items[comboBox2.SelectedIndex].ToString());
                int BeginPos = result.IndexOf("TRANSLATED_TEXT='");
                if (BeginPos > -1)
                {
                    int EndPos = result.IndexOf("'", BeginPos + 17);
                    result = result.Substring(BeginPos + 17, EndPos - BeginPos - 17);
                    textBox2.Text = (result);
                }
                if (checkBox1.Checked)
                {
                    result = translate(textBox2.Text, comboBox2.Items[comboBox2.SelectedIndex].ToString(), comboBox1.Items[comboBox1.SelectedIndex].ToString());
                    BeginPos = result.IndexOf("TRANSLATED_TEXT='");
                    if (BeginPos > -1)
                    {
                        int EndPos = result.IndexOf("'", BeginPos + 17);
                        result = result.Substring(BeginPos + 17, EndPos - BeginPos - 17);
                        textBox3.Text = (result);
                    }
                }
                if (checkBox2.Checked)
                {
                    if (checkBox1.Checked) Game.ExecuteCommand(((IsSayTeam) ? "say_team " : "say ") + textBox3.Text);
                    else Game.ExecuteCommand(((IsSayTeam) ? "say_team " : "say ") + textBox2.Text);
                    label1.Text = "▼ Google Translate By DarkPower - " + ((IsSayTeam) ? "SayTeam" : "SayGlobal");
                    if (checkBox3.Checked)
                    {
                        Width = 132;
                        Height = 28;
                        Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 10);
                        Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 5);
                        Win32.SetForegroundWindow(Win32.FindWindow(null, "Dota 2"));
                    }
                }
            }

            public string translate(string word, string SL, string DL)
            {
                var cookies = new CookieContainer();
                string result;
                ServicePointManager.Expect100Continue = false;
                var request = (HttpWebRequest)WebRequest.Create("http://translate.google.ru/?sl=" + SL + "&tl=" + DL + "&q=" + word);
                request.CookieContainer = cookies;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4";

                using (var requestStream = request.GetRequestStream())

                using (var responseStream = request.GetResponse().GetResponseStream())
                using (var reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
                {
                    result = reader.ReadToEnd();
                }

                return result;
            }

            private void button2_Click(object sender, EventArgs e)
            {
                int n = comboBox1.SelectedIndex;
                comboBox1.SelectedIndex = comboBox2.SelectedIndex;
                comboBox2.SelectedIndex = n;
            }

            private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                System.Diagnostics.Process.Start("https://github.com/DarkP0wer/DarkPower");
            }

            private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                System.Diagnostics.Process.Start("https://www.joduska.me/forum/topic/145393-google-translate/");
            }

            private void Form1_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Win32.ReleaseCapture();
                    Win32.SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }

            private void label1_Click(object sender, EventArgs e)
            {
                if (Width == 505)
                {
                    label1.Text = "▼ Google Translate By DarkPower - " + ((IsSayTeam) ? "SayTeam" : "SayGlobal");
                    Width = 132;
                    Height = 28;
                }
                else
                {
                    label1.Text = "▲ Google Translate By DarkPower - " + ((IsSayTeam) ? "SayTeam" : "SayGlobal");
                    Width = 505;
                    Height = 175;
                }
            }

            private void textBox1_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        {
                            ExitGUI = true;
                            button1.PerformClick();
                            break;
                        }
                }
                e.Handled = true;

                base.OnKeyDown(e);
            }
        }

        static Form1 frm = new Form1();

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen) return;
            try
            {
                if (args.Msg == 0x0101 && args.WParam == 0x0D)
                {
                    if (ExitGUI)
                    {
                        ExitGUI = false;
                        return;
                    }
                    if (Game.IsKeyDown(0x10)) IsSayTeam = false;
                    else IsSayTeam = true;
                    frm.textBox1.Focus();
                    frm.Show();
                    frm.Width = 505;
                    frm.Height = 175;
                    frm.label1.Text = "▲ Google Translate By DarkPower - " + ((IsSayTeam) ? "SayTeam" : "SayGlobal");
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
