using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;


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
        public static IntPtr GHandle;
        public struct AdressAndValue
        {
            public int Adress;
            public int Value;
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
            [Flags]
            public enum AllocationType
            {
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }

            [Flags]
            public enum MemoryProtection
            {
                Execute = 0x10,
                ExecuteRead = 0x20,
                ExecuteReadWrite = 0x40,
                ExecuteWriteCopy = 0x80,
                NoAccess = 0x01,
                ReadOnly = 0x02,
                ReadWrite = 0x04,
                WriteCopy = 0x08,
                GuardModifierflag = 0x100,
                NoCacheModifierflag = 0x200,
                WriteCombineModifierflag = 0x400
            }
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
            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
               IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        }

        public static AdressAndValue Pointer(string ProcessName, object Address, int[] Offsets, Boolean ReadOnly, int WValue)
        {
            int BaseAddy = -1;
            AdressAndValue res = new AdressAndValue();

            Process[] P = Process.GetProcessesByName(ProcessName);
            if (P.Length == 0)
            {
                res.Adress = -1;
                res.Value = -1;
                return res;
            }

            if (Address.GetType() == typeof(String))
            {
                string[] tmp = Convert.ToString(Address).Split('+');
                if (tmp[0].ToLower() == "non")
                {
                    BaseAddy = int.Parse(tmp[1], NumberStyles.HexNumber);
                }
                else
                {
                    foreach (ProcessModule M in P[0].Modules)
                        if (M.ModuleName.ToLower() == tmp[0].ToLower())
                            BaseAddy = M.BaseAddress.ToInt32() + int.Parse(tmp[1], NumberStyles.HexNumber);
                }
            }
            else
            {
                res.Adress = BaseAddy;
                int o = 0;
                byte[] buff2 = new byte[64];
                Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(BaseAddy), buff2, 64, ref o);
                GHandle = P[0].Handle;
                BaseAddy = BitConverter.ToInt32(buff2, 0);
                res.Value = BaseAddy;
                return res;
            }

            byte[] buff = new byte[64];
            int Final_Address = -1;
            for (int i = 0; i < Offsets.Length; i++)
            {
                int o = 0;
                Final_Address = BaseAddy + Offsets[i];
                Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(BaseAddy + Offsets[i]), buff, 64, ref o);
                BaseAddy = BitConverter.ToInt32(buff, 0);
            }

            if (Final_Address != -1 && !ReadOnly)
            {
                int bytesWritten = 0;
                byte[] buffer = BitConverter.GetBytes(WValue);

                Win32.WriteProcessMemory(P[0].Handle, Final_Address, buffer, buffer.Length, ref bytesWritten);
            }

            res.Adress = Final_Address;
            res.Value = BaseAddy;
            Console.WriteLine(Final_Address);
            Console.WriteLine(BaseAddy);
            return res;
        }

        private static System.Timers.Timer aTimer;
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
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            Game.OnWndProc += Game_OnGameWndProc;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (frm.textBox1.Text == "MAP")
            {
                /*string [] lines = frm.textBox2.Text.Split('\n');
                string[] lines2 = frm.textBox2.Text.Split('\n');
                string[] lines3 = frm.textBox2.Text.Split('\n');
                frm.textBox3.Clear(); frm.textBox4.Clear();
                for (int i = 0; i < lines.Length - 1; i++)
                {*/
                Console.Write("non+" + frm.textBox2.Text);
                frm.textBox3.Text = Pointer("dota2", "non+" + frm.textBox2.Text, new int[] { 0x71C }, true, 0).Adress.ToString("F4");
                frm.textBox4.Text = Pointer("dota2", "non+" + frm.textBox2.Text, new int[] { 0x720 }, true, 0).Adress.ToString("F4");
                //}
            }
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
                this.textBox2 = new System.Windows.Forms.TextBox();
                this.textBox3 = new System.Windows.Forms.TextBox();
                this.textBox4 = new System.Windows.Forms.TextBox();
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
                this.panel1.Controls.Add(this.textBox4);
                this.panel1.Controls.Add(this.textBox3);
                this.panel1.Controls.Add(this.textBox2);
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
                // textBox2
                // 
                this.textBox2.Location = new System.Drawing.Point(3, 60);
                this.textBox2.Multiline = true;
                this.textBox2.Name = "textBox2";
                this.textBox2.Size = new System.Drawing.Size(101, 75);
                this.textBox2.TabIndex = 19;
                // 
                // textBox3
                // 
                this.textBox3.Location = new System.Drawing.Point(110, 60);
                this.textBox3.Multiline = true;
                this.textBox3.Name = "textBox3";
                this.textBox3.Size = new System.Drawing.Size(87, 75);
                this.textBox3.TabIndex = 20;
                // 
                // textBox4
                // 
                this.textBox4.Location = new System.Drawing.Point(203, 60);
                this.textBox4.Multiline = true;
                this.textBox4.Name = "textBox4";
                this.textBox4.Size = new System.Drawing.Size(100, 75);
                this.textBox4.TabIndex = 21;
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
            public System.Windows.Forms.TextBox textBox1;
            private System.Windows.Forms.Button button4;
            public System.Windows.Forms.TextBox textBox4;
            public System.Windows.Forms.TextBox textBox3;
            public System.Windows.Forms.TextBox textBox2;

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
                label2.Text = Pointer("dota2", "server.dll+1C6F980", new int[] { 0, 0x2c8, 0x18 }, false, Convert.ToInt32(textBox1.Text)).Value.ToString("X");
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

        public void MakeCave()
        {
            if (GHandle != IntPtr.Zero)
            {
                var valueon = new byte[] { 0xC7, 0x80, 0xD0, 0x05, 0x00, 0x00, 0x40, 0x42, 0x0F, 0x00 }; // Байты команд на которые поменяются оригинальные команды
                var valueoff = new byte[] { 0x89, 0x7B, 0x18, 0x48, 0x8B, 0x5C, 0x24, 0x30 }; // Байты оригинальной команды
                var caveAddress = (Int32)(Win32.VirtualAllocEx(GHandle, IntPtr.Zero, (IntPtr)(valueon.Length + 5), Win32.AllocationType.Commit, Win32.MemoryProtection.ExecuteReadWrite));
                var addressFrom = FindGoldWSignature();
                if (addressFrom == -1) return;
                var addressTo = addressFrom - caveAddress - valueon.Length;
                var buffer = new byte[valueoff.Length];
                buffer[0] = 0xE9;
                var f = BitConverter.GetBytes(caveAddress - addressFrom - 5);
                Array.Copy(f, 0, buffer, 1, f.Length);
                for (var i = buffer.Length - 1; i < valueoff.Length; i++)
                {
                    buffer[i] = 0x90;
                }
                var dummy = 0;
                var caveBuffer = new byte[valueon.Length + 5];
                Array.Copy(valueon, caveBuffer, valueon.Length);
                caveBuffer[valueon.Length] = 0xE9;
                var retAddress = BitConverter.GetBytes(addressTo);
                Array.Copy(retAddress, 0, caveBuffer, valueon.Length + 1, retAddress.Length);
                Win32.WriteProcessMemory(GHandle, (long)caveAddress, caveBuffer, (int)caveBuffer.Length, ref dummy);
                var dummy1 = 0;
                Win32.WriteProcessMemory(GHandle, (long)addressFrom, buffer, (int)buffer.Length, ref dummy1);
                //VirtualFreeEx(handle, (IntPtr)0x09E90000, 0, FreeType.Release); //Освобождает выделенную память, подумать над тем, как сделать освобождение памяти после того как отключать дллку
            }
        }
        
        public int FindGoldWSignature()
        {
            var moduleName = "server.dll";
            var startAddress = 0;
            var sizeToAllocate = 0;
            var pattern = new byte[] { 0x89, 0x7B, 0x18, 0x48, 0x8B, 0x5C, 0x24, 0x30 };

            if (GHandle != IntPtr.Zero)
            {
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Handle == GHandle)
                    {
                        foreach (ProcessModule m in p.Modules)
                        {
                            if (m.ModuleName == moduleName)
                            {
                                startAddress = (int)m.BaseAddress;
                                sizeToAllocate = m.ModuleMemorySize;
                                break;
                            }
                        }
                    }
                }
                var handle = GHandle;
                if (handle != IntPtr.Zero)
                {
                    byte[] buffer = new byte[sizeToAllocate];
                    int bytesread = 0;
                    Win32.ReadProcessMemory(handle, (IntPtr)startAddress, buffer, (int)sizeToAllocate, ref bytesread);
                    var counter = 0;
                    if (pattern.Length <= buffer.Length)
                    {
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i] == pattern[0] | pattern[0] == 0x00)
                            {
                                for (var j = 0; j < pattern.Length; j++)
                                {
                                    if (buffer[i + j] == pattern[j] | pattern[j] == 0x00)
                                    {
                                        counter++;
                                        if (counter == pattern.Length)
                                        {
                                            var result = i + startAddress;
                                            return (int)result;
                                        }
                                    }
                                    else
                                    {
                                        counter = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
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
