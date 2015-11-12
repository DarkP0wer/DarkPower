using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Net;


namespace HOST_HACKS
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        #region CFG

        public static String filename = "Host_Hacks.ini";
        public static string[] KeysName = new string[] { "NUMPAD0", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6", "NUMPAD7", "NUMPAD8", "NUMPAD9" };
        public static ulong[] KeysValue = new ulong[] { 96, 97, 98, 99, 100, 101, 102, 103, 104, 105 };
        public static int OpenKey;
        public static Boolean IsFormClose;
        public struct AdressAndValue
        {
            public long Adress;
            public long Value;
        }
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
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out()] byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress,
              byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
        }

        public static AdressAndValue Pointer(string ProcessName, object Address, int[] Offsets, Boolean ReadOnly, int WValue)
        {
            long BaseAddy = -1;
            AdressAndValue res = new AdressAndValue();

            Process[] P = Process.GetProcessesByName(ProcessName);
            if (P.Length == 0)
            {
                res.Adress = -1;
                res.Value = -1;
                return res;
            }

            if (Address.GetType() == typeof(Int32))
                BaseAddy = Convert.ToInt64(Address);

            else if (Address.GetType() == typeof(String))
            {
                string[] tmp = Convert.ToString(Address).Split('+');
                foreach (ProcessModule M in P[0].Modules)
                    if (M.ModuleName.ToLower() == tmp[0].ToLower())
                        BaseAddy = M.BaseAddress.ToInt64() + int.Parse(tmp[1], NumberStyles.HexNumber);
            }
            else
            {
                res.Adress = BaseAddy;
                int o = 0;
                byte[] buff2 = new byte[64];
                Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(BaseAddy), buff2, 64, ref o);
                BaseAddy = BitConverter.ToInt64(buff2, 0);
                res.Value = BaseAddy;
                return res;
            }

            byte[] buff = new byte[64];
            long Final_Address = -1;
            for (int i = 0; i < Offsets.Length; i++)
            {
                int o = 0;
                Final_Address = BaseAddy + Offsets[i];
                Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(BaseAddy + Offsets[i]), buff, 64, ref o);
                BaseAddy = BitConverter.ToInt64(buff, 0);
            }

            if (Final_Address != -1 && !ReadOnly)
            {
                int bytesWritten = 0;
                byte[] buffer = BitConverter.GetBytes(WValue);

                Win32.WriteProcessMemory(P[0].Handle, Final_Address, buffer, buffer.Length, ref bytesWritten);
            }

            res.Adress = Final_Address;
            res.Value = BaseAddy;
            return res;
        }
        static void Main(string[] args)
        {
            #region FormC
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
            }
            else
            {
                var IniFile = new IniFile(filename);
                IniFile.Write("OpenKey", "0", "HotKeys");
                OpenKey = 0;
                frm.comboBox1.SelectedIndex = 0;
                //System.IO.File.SetAttributes(filename, System.IO.FileAttributes.System);
            }
            #endregion
            Game.OnWndProc += Game_OnGameWndProc;
        }

        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.button1 = new System.Windows.Forms.Button();
                this.label1 = new System.Windows.Forms.Label();
                this.linkLabel1 = new System.Windows.Forms.LinkLabel();
                this.linkLabel2 = new System.Windows.Forms.LinkLabel();
                this.button2 = new System.Windows.Forms.Button();
                this.comboBox1 = new System.Windows.Forms.ComboBox();
                this.panel1 = new System.Windows.Forms.Panel();
                this.button3 = new System.Windows.Forms.Button();
                this.textBox1 = new System.Windows.Forms.TextBox();
                this.label2 = new System.Windows.Forms.Label();
                this.button4 = new System.Windows.Forms.Button();
                this.panel1.SuspendLayout();
                this.SuspendLayout();
                // 
                // button1
                // 
                this.button1.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.button1.Location = new System.Drawing.Point(2, 2);
                this.button1.Name = "button1";
                this.button1.Size = new System.Drawing.Size(80, 21);
                this.button1.TabIndex = 4;
                this.button1.Text = "HH HIDE";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Click += new System.EventHandler(this.button1_Click);
                // 
                // label1
                // 
                this.label1.AutoSize = true;
                this.label1.Location = new System.Drawing.Point(2, 26);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(45, 13);
                this.label1.TabIndex = 5;
                this.label1.Text = "HotKey:";
                // 
                // linkLabel1
                // 
                this.linkLabel1.AutoSize = true;
                this.linkLabel1.LinkColor = System.Drawing.Color.White;
                this.linkLabel1.Location = new System.Drawing.Point(9, 56);
                this.linkLabel1.Name = "linkLabel1";
                this.linkLabel1.Size = new System.Drawing.Size(73, 13);
                this.linkLabel1.TabIndex = 6;
                this.linkLabel1.TabStop = true;
                this.linkLabel1.Text = "Forum Thread";
                this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                // 
                // linkLabel2
                // 
                this.linkLabel2.AutoSize = true;
                this.linkLabel2.LinkColor = System.Drawing.Color.White;
                this.linkLabel2.Location = new System.Drawing.Point(9, 78);
                this.linkLabel2.Name = "linkLabel2";
                this.linkLabel2.Size = new System.Drawing.Size(89, 13);
                this.linkLabel2.TabIndex = 7;
                this.linkLabel2.TabStop = true;
                this.linkLabel2.Text = "GitHub DarkP0wer";
                this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
                // 
                // button2
                // 
                this.button2.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.button2.Location = new System.Drawing.Point(12, 119);
                this.button2.Name = "button2";
                this.button2.Size = new System.Drawing.Size(86, 24);
                this.button2.TabIndex = 8;
                this.button2.Text = "Close";
                this.button2.UseVisualStyleBackColor = true;
                this.button2.Click += new System.EventHandler(this.button2_Click);
                // 
                // comboBox1
                // 
                this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.comboBox1.FormattingEnabled = true;
                this.comboBox1.Location = new System.Drawing.Point(53, 23);
                this.comboBox1.Name = "comboBox1";
                this.comboBox1.Size = new System.Drawing.Size(108, 21);
                this.comboBox1.TabIndex = 10;
                this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
                // 
                // panel1
                // 
                this.panel1.BackColor = System.Drawing.Color.DeepSkyBlue;
                this.panel1.Controls.Add(this.label2);
                this.panel1.Controls.Add(this.button3);
                this.panel1.Controls.Add(this.textBox1);
                this.panel1.Location = new System.Drawing.Point(167, 5);
                this.panel1.Name = "panel1";
                this.panel1.Size = new System.Drawing.Size(330, 138);
                this.panel1.TabIndex = 14;
                this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                // 
                // button3
                // 
                this.button3.Location = new System.Drawing.Point(110, 21);
                this.button3.Name = "button3";
                this.button3.Size = new System.Drawing.Size(69, 20);
                this.button3.TabIndex = 16;
                this.button3.Text = "SetGold";
                this.button3.UseVisualStyleBackColor = true;
                this.button3.Click += new System.EventHandler(this.button3_Click);
                // 
                // textBox1
                // 
                this.textBox1.Location = new System.Drawing.Point(3, 21);
                this.textBox1.Name = "textBox1";
                this.textBox1.Size = new System.Drawing.Size(101, 20);
                this.textBox1.TabIndex = 14;
                this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label2
                // 
                this.label2.AutoSize = true;
                this.label2.Location = new System.Drawing.Point(4, 5);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(29, 13);
                this.label2.TabIndex = 17;
                this.label2.Text = "Gold";
                // 
                // button4
                // 
                this.button4.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.button4.Location = new System.Drawing.Point(12, 94);
                this.button4.Name = "button4";
                this.button4.Size = new System.Drawing.Size(86, 24);
                this.button4.TabIndex = 15;
                this.button4.Text = "Info";
                this.button4.UseVisualStyleBackColor = true;
                this.button4.Click += new System.EventHandler(this.button4_Click);
                // 
                // Form1
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.BackColor = System.Drawing.SystemColors.Highlight;
                this.ClientSize = new System.Drawing.Size(506, 152);
                this.ControlBox = false;
                this.Controls.Add(this.button4);
                this.Controls.Add(this.panel1);
                this.Controls.Add(this.linkLabel2);
                this.Controls.Add(this.linkLabel1);
                this.Controls.Add(this.label1);
                this.Controls.Add(this.button1);
                this.Controls.Add(this.button2);
                this.Controls.Add(this.comboBox1);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
                this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                this.Name = "Form1";
                this.Opacity = 0.85D;
                this.ShowIcon = false;
                this.Text = "Host_Hacks";
                this.TopMost = true;
                this.panel1.ResumeLayout(false);
                this.panel1.PerformLayout();
                this.ResumeLayout(false);
                this.PerformLayout();

            }

            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            private System.Windows.Forms.Button button2;
            public System.Windows.Forms.ComboBox comboBox1;
            private System.Windows.Forms.Panel panel1;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Button button3;
            private System.Windows.Forms.TextBox textBox1;
            private System.Windows.Forms.Button button4;

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
                if (Width != 506)
                {
                    Width = 506; Height = 152;
                }
                else
                {
                    Width = 1; Height = 1;
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 10);
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 5);
                    Win32.SetForegroundWindow(Win32.FindWindow(null, "Dota 2"));
                }
            }

            private void button2_Click(object sender, EventArgs e)
            {
                IsFormClose = true;
                Close();
            }

            private void button3_Click(object sender, EventArgs e)
            {
                label2.Text = Pointer("dota2", "server.dll+1C5FC70", new int[] { 0, 0x2c8, 0x18 }, false, Convert.ToInt32(textBox1.Text)).Value.ToString("X");
            }

            private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)/* && (e.KeyChar != '.')*/)
                {
                    e.Handled = true;
                }
                /*only allow one decimal point
                if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }*/
            }

            private void button4_Click(object sender, EventArgs e)
            {
                Win32.MessageBox(0, "HACKS for HOST.\r\nDON'T USE if you haven't ANTIVAC\r\nIt works inly on NOT dedicated servers. ", "HOST_HACKS", 0);
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

        }


        static Form1 frm = new Form1();

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame) return;
            try
            {
                if (OpenKey > KeysValue.Length - 1 || OpenKey < 0) { OpenKey = 0; Win32.MessageBox(0, "Your HotKey changed to NUMPAD0", "HOST_HACKS", 0); }
                if (args.Msg == 0x0101 && args.WParam == KeysValue[OpenKey])
                {
                    if (IsFormClose)
                    {
                        Win32.MessageBox(0, "You close form!\r\n Reload script for openning!", "HOST_HACKS", 0);
                        return;
                    }
                    frm.Width = 506; frm.Height = 152;
                    frm.Show();
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
