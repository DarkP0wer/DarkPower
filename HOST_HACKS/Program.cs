using System;
using Ensage;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using Ensage.Common.Menu;


namespace HOST_HACKS
{
    class Program
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        private static readonly Ensage.Common.Menu.Menu SubMenu = new Ensage.Common.Menu.Menu("Host Hacks", "HOST HACKS", true);

        public static Boolean IsFormClose;
        public static IntPtr GHandle;
        public struct AdressAndValue
        {
            public long Adress;
            public long Value;
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
            public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
            {
                var clr = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(text, arguments);
                Console.ForegroundColor = clr;
            }
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
                            BaseAddy = M.BaseAddress.ToInt64() + int.Parse(tmp[1], NumberStyles.HexNumber);
                }
            }
            else
            {
                res.Adress = BaseAddy;
                int o = 0;
                byte[] buff2 = new byte[64];
                Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(BaseAddy), buff2, 64, ref o);
                GHandle = P[0].Handle;
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

        private static System.Timers.Timer aTimer;
        static void Main(string[] args)
        {
            IsFormClose = false;
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("HHKey", "Form Key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("TPKey", "TP Key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("AutoGO", "AutoGO after TP").SetValue(true));
			SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("RHKey", "Restore Health Key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddToMainMenu();
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            Game.OnWndProc += Game_OnGameWndProc;
        }

        static string OffsetGoldR = "server.dll+1CC3240";
        static string OffsetGoldD = "server.dll+1CC3248";
		static string OffsetPlayer = "server.dll+01C51898";

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (frm.checkBox1.Checked && Game.IsInGame)
            {
                String s; int value;
                try
                {
                    s = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x18 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label1.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0xA8 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label2.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x138 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label3.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x1C8 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label4.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x258 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label5.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x18 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label6.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0xA8 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label7.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x138 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label8.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x1C8 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label9.Text = Convert.ToString(value);
                }
                catch { }
                try
                {
                    s = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x258 }, true, 1).Value.ToString("X");
                    s = s.Substring(8);
                    value = Convert.ToInt32(s, 16);
                    frm.label10.Text = Convert.ToString(value);
                }
                catch { }

                try
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    GHandle = P[0].Handle;
                    int o = 0;
                    byte[] buff1 = new byte[128];
                    s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590}, true, 1).Adress.ToString("X");
                    Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(long.Parse(s, NumberStyles.HexNumber)), buff1, buff1.Length, ref o);
                    frm.label666.Text = BitConverter.ToInt32(buff1, 0).ToString("X");/*BitConverter.ToSingle(buff1, 0)*/;
                }
                catch { }
            }
            /*if (frm.textBox1.Text == "MAP")
            {
                string [] lines = frm.textBox2.Text.Split('\n');
                string[] lines2 = frm.textBox2.Text.Split('\n');
                string[] lines3 = frm.textBox2.Text.Split('\n');
                frm.textBox3.Clear(); frm.textBox4.Clear();
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    int o = 0;
                    byte[] buff1 = new byte[128];
                    Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(long.Parse(frm.textBox2.Text, NumberStyles.HexNumber) + 216), buff1, buff1.Length, ref o);
                    byte[] buff2 = new byte[128];
                    Win32.ReadProcessMemory(P[0].Handle, (IntPtr)(long.Parse(frm.textBox2.Text, NumberStyles.HexNumber) + 220), buff2, buff2.Length, ref o);
                    frm.textBox3.Text = Convert.ToString(BitConverter.ToSingle(buff1, 0));
                    frm.textBox4.Text = Convert.ToString(BitConverter.ToSingle(buff2, 0));
                }
            }*/
        }

        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.buttonH = new System.Windows.Forms.Button();
                this.linkLabel1 = new System.Windows.Forms.LinkLabel();
                this.linkLabel2 = new System.Windows.Forms.LinkLabel();
                this.buttonC = new System.Windows.Forms.Button();
                this.panel1 = new System.Windows.Forms.Panel();
                this.label666 = new System.Windows.Forms.Label();
                this.button666 = new System.Windows.Forms.Button();
                this.textBox666 = new System.Windows.Forms.TextBox();
                this.checkBox1 = new System.Windows.Forms.CheckBox();
                this.label1 = new System.Windows.Forms.Label();
                this.button1 = new System.Windows.Forms.Button();
                this.textBox1 = new System.Windows.Forms.TextBox();
                this.label2 = new System.Windows.Forms.Label();
                this.button2 = new System.Windows.Forms.Button();
                this.textBox2 = new System.Windows.Forms.TextBox();
                this.label3 = new System.Windows.Forms.Label();
                this.button3 = new System.Windows.Forms.Button();
                this.textBox3 = new System.Windows.Forms.TextBox();
                this.label4 = new System.Windows.Forms.Label();
                this.button4 = new System.Windows.Forms.Button();
                this.textBox4 = new System.Windows.Forms.TextBox();
                this.label5 = new System.Windows.Forms.Label();
                this.button5 = new System.Windows.Forms.Button();
                this.textBox5 = new System.Windows.Forms.TextBox();
                this.label6 = new System.Windows.Forms.Label();
                this.button6 = new System.Windows.Forms.Button();
                this.textBox6 = new System.Windows.Forms.TextBox();
                this.label7 = new System.Windows.Forms.Label();
                this.button7 = new System.Windows.Forms.Button();
                this.textBox7 = new System.Windows.Forms.TextBox();
                this.label8 = new System.Windows.Forms.Label();
                this.button8 = new System.Windows.Forms.Button();
                this.textBox8 = new System.Windows.Forms.TextBox();
                this.label9 = new System.Windows.Forms.Label();
                this.button9 = new System.Windows.Forms.Button();
                this.textBox9 = new System.Windows.Forms.TextBox();
                this.label10 = new System.Windows.Forms.Label();
                this.button10 = new System.Windows.Forms.Button();
                this.textBox10 = new System.Windows.Forms.TextBox();
                this.panel1.SuspendLayout();
                this.SuspendLayout();
                // 
                // buttonH
                // 
                this.buttonH.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.buttonH.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.buttonH.Location = new System.Drawing.Point(2, 2);
                this.buttonH.Name = "buttonH";
                this.buttonH.Size = new System.Drawing.Size(80, 21);
                this.buttonH.TabIndex = 4;
                this.buttonH.Text = "HH HIDE";
                this.buttonH.UseVisualStyleBackColor = true;
                this.buttonH.Click += new System.EventHandler(this.buttonH_Click);
                // 
                // linkLabel1
                // 
                this.linkLabel1.AutoSize = true;
                this.linkLabel1.LinkColor = System.Drawing.Color.White;
                this.linkLabel1.Location = new System.Drawing.Point(88, 6);
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
                this.linkLabel2.Location = new System.Drawing.Point(167, 6);
                this.linkLabel2.Name = "linkLabel2";
                this.linkLabel2.Size = new System.Drawing.Size(96, 13);
                this.linkLabel2.TabIndex = 7;
                this.linkLabel2.TabStop = true;
                this.linkLabel2.Text = "GitHub DarkP0wer";
                this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
                // 
                // buttonC
                // 
                this.buttonC.BackColor = System.Drawing.SystemColors.ControlLightLight;
                this.buttonC.Location = new System.Drawing.Point(548, 2);
                this.buttonC.Name = "buttonC";
                this.buttonC.Size = new System.Drawing.Size(58, 24);
                this.buttonC.TabIndex = 8;
                this.buttonC.Text = "Close";
                this.buttonC.UseVisualStyleBackColor = true;
                this.buttonC.Click += new System.EventHandler(this.buttonC_Click);
                // 
                // panel1
                // 
                this.panel1.BackColor = System.Drawing.Color.DeepSkyBlue;
                this.panel1.Controls.Add(this.label10);
                this.panel1.Controls.Add(this.button10);
                this.panel1.Controls.Add(this.textBox10);
                this.panel1.Controls.Add(this.label9);
                this.panel1.Controls.Add(this.button9);
                this.panel1.Controls.Add(this.textBox9);
                this.panel1.Controls.Add(this.label8);
                this.panel1.Controls.Add(this.button8);
                this.panel1.Controls.Add(this.textBox8);
                this.panel1.Controls.Add(this.label7);
                this.panel1.Controls.Add(this.button7);
                this.panel1.Controls.Add(this.textBox7);
                this.panel1.Controls.Add(this.label6);
                this.panel1.Controls.Add(this.button6);
                this.panel1.Controls.Add(this.textBox6);
                this.panel1.Controls.Add(this.label5);
                this.panel1.Controls.Add(this.button5);
                this.panel1.Controls.Add(this.textBox5);
                this.panel1.Controls.Add(this.label4);
                this.panel1.Controls.Add(this.button4);
                this.panel1.Controls.Add(this.textBox4);
                this.panel1.Controls.Add(this.label3);
                this.panel1.Controls.Add(this.button3);
                this.panel1.Controls.Add(this.textBox3);
                this.panel1.Controls.Add(this.label2);
                this.panel1.Controls.Add(this.button2);
                this.panel1.Controls.Add(this.textBox2);
                this.panel1.Controls.Add(this.label666);
                this.panel1.Controls.Add(this.button666);
                this.panel1.Controls.Add(this.textBox666);
                this.panel1.Controls.Add(this.checkBox1);
                this.panel1.Controls.Add(this.label1);
                this.panel1.Controls.Add(this.button1);
                this.panel1.Controls.Add(this.textBox1);
                this.panel1.Location = new System.Drawing.Point(12, 29);
                this.panel1.Name = "panel1";
                this.panel1.Size = new System.Drawing.Size(594, 199);
                this.panel1.TabIndex = 14;
                this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                // 
                // label666
                // 
                this.label666.AutoSize = true;
                this.label666.Location = new System.Drawing.Point(185, 178);
                this.label666.Name = "label666";
                this.label666.Size = new System.Drawing.Size(47, 13);
                this.label666.TabIndex = 28;
                this.label666.Text = "Player Adress";
                // 
                // button666
                // 
                this.button666.Location = new System.Drawing.Point(110, 174);
                this.button666.Name = "button666";
                this.button666.Size = new System.Drawing.Size(69, 20);
                this.button666.TabIndex = 27;
                this.button666.Text = "SetHpRegenaration";
                this.button666.UseVisualStyleBackColor = true;
                this.button666.Click += new System.EventHandler(this.button666_Click);
                // 
                // textBox666
                // 
                this.textBox666.Location = new System.Drawing.Point(3, 174);
                this.textBox666.Name = "textBox666";
                this.textBox666.Size = new System.Drawing.Size(101, 20);
                this.textBox666.TabIndex = 26;
                // 
                // checkBox1
                // 
                this.checkBox1.AutoSize = true;
                this.checkBox1.Checked = true;
                this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
                this.checkBox1.Location = new System.Drawing.Point(3, 3);
                this.checkBox1.Name = "checkBox1";
                this.checkBox1.Size = new System.Drawing.Size(92, 17);
                this.checkBox1.TabIndex = 22;
                this.checkBox1.Text = "Check Values";
                this.checkBox1.UseVisualStyleBackColor = true;
                // 
                // label1
                // 
                this.label1.AutoSize = true;
                this.label1.Location = new System.Drawing.Point(185, 25);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(46, 13);
                this.label1.TabIndex = 17;
                this.label1.Text = "Gold R1";
                // 
                // button1
                // 
                this.button1.Location = new System.Drawing.Point(110, 21);
                this.button1.Name = "button1";
                this.button1.Size = new System.Drawing.Size(69, 20);
                this.button1.TabIndex = 16;
                this.button1.Text = "SetGold";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Click += new System.EventHandler(this.button1_Click);
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
                this.label2.Location = new System.Drawing.Point(185, 51);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(46, 13);
                this.label2.TabIndex = 31;
                this.label2.Text = "Gold R2";
                // 
                // button2
                // 
                this.button2.Location = new System.Drawing.Point(110, 47);
                this.button2.Name = "button2";
                this.button2.Size = new System.Drawing.Size(69, 20);
                this.button2.TabIndex = 30;
                this.button2.Text = "SetGold";
                this.button2.UseVisualStyleBackColor = true;
                this.button2.Click += new System.EventHandler(this.button2_Click);
                // 
                // textBox2
                // 
                this.textBox2.Location = new System.Drawing.Point(3, 47);
                this.textBox2.Name = "textBox2";
                this.textBox2.Size = new System.Drawing.Size(101, 20);
                this.textBox2.TabIndex = 29;
                this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label3
                // 
                this.label3.AutoSize = true;
                this.label3.Location = new System.Drawing.Point(185, 77);
                this.label3.Name = "label3";
                this.label3.Size = new System.Drawing.Size(46, 13);
                this.label3.TabIndex = 34;
                this.label3.Text = "Gold R3";
                // 
                // button3
                // 
                this.button3.Location = new System.Drawing.Point(110, 73);
                this.button3.Name = "button3";
                this.button3.Size = new System.Drawing.Size(69, 20);
                this.button3.TabIndex = 33;
                this.button3.Text = "SetGold";
                this.button3.UseVisualStyleBackColor = true;
                this.button3.Click += new System.EventHandler(this.button3_Click);
                // 
                // textBox3
                // 
                this.textBox3.Location = new System.Drawing.Point(3, 73);
                this.textBox3.Name = "textBox3";
                this.textBox3.Size = new System.Drawing.Size(101, 20);
                this.textBox3.TabIndex = 32;
                this.textBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label4
                // 
                this.label4.AutoSize = true;
                this.label4.Location = new System.Drawing.Point(185, 103);
                this.label4.Name = "label4";
                this.label4.Size = new System.Drawing.Size(46, 13);
                this.label4.TabIndex = 37;
                this.label4.Text = "Gold R4";
                // 
                // button4
                // 
                this.button4.Location = new System.Drawing.Point(110, 99);
                this.button4.Name = "button4";
                this.button4.Size = new System.Drawing.Size(69, 20);
                this.button4.TabIndex = 36;
                this.button4.Text = "SetGold";
                this.button4.UseVisualStyleBackColor = true;
                this.button4.Click += new System.EventHandler(this.button4_Click);
                // 
                // textBox4
                // 
                this.textBox4.Location = new System.Drawing.Point(3, 99);
                this.textBox4.Name = "textBox4";
                this.textBox4.Size = new System.Drawing.Size(101, 20);
                this.textBox4.TabIndex = 35;
                this.textBox4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label5
                // 
                this.label5.AutoSize = true;
                this.label5.Location = new System.Drawing.Point(185, 129);
                this.label5.Name = "label5";
                this.label5.Size = new System.Drawing.Size(46, 13);
                this.label5.TabIndex = 40;
                this.label5.Text = "Gold R5";
                // 
                // button5
                // 
                this.button5.Location = new System.Drawing.Point(110, 125);
                this.button5.Name = "button5";
                this.button5.Size = new System.Drawing.Size(69, 20);
                this.button5.TabIndex = 39;
                this.button5.Text = "SetGold";
                this.button5.UseVisualStyleBackColor = true;
                this.button5.Click += new System.EventHandler(this.button5_Click);
                // 
                // textBox5
                // 
                this.textBox5.Location = new System.Drawing.Point(3, 125);
                this.textBox5.Name = "textBox5";
                this.textBox5.Size = new System.Drawing.Size(101, 20);
                this.textBox5.TabIndex = 38;
                this.textBox5.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label6
                // 
                this.label6.AutoSize = true;
                this.label6.Location = new System.Drawing.Point(471, 25);
                this.label6.Name = "label6";
                this.label6.Size = new System.Drawing.Size(46, 13);
                this.label6.TabIndex = 43;
                this.label6.Text = "Gold D1";
                // 
                // button6
                // 
                this.button6.Location = new System.Drawing.Point(396, 21);
                this.button6.Name = "button6";
                this.button6.Size = new System.Drawing.Size(69, 20);
                this.button6.TabIndex = 42;
                this.button6.Text = "SetGold";
                this.button6.UseVisualStyleBackColor = true;
                this.button6.Click += new System.EventHandler(this.button6_Click);
                // 
                // textBox6
                // 
                this.textBox6.Location = new System.Drawing.Point(289, 21);
                this.textBox6.Name = "textBox6";
                this.textBox6.Size = new System.Drawing.Size(101, 20);
                this.textBox6.TabIndex = 41;
                this.textBox6.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label7
                // 
                this.label7.AutoSize = true;
                this.label7.Location = new System.Drawing.Point(471, 51);
                this.label7.Name = "label7";
                this.label7.Size = new System.Drawing.Size(46, 13);
                this.label7.TabIndex = 46;
                this.label7.Text = "Gold D2";
                // 
                // button7
                // 
                this.button7.Location = new System.Drawing.Point(396, 47);
                this.button7.Name = "button7";
                this.button7.Size = new System.Drawing.Size(69, 20);
                this.button7.TabIndex = 45;
                this.button7.Text = "SetGold";
                this.button7.UseVisualStyleBackColor = true;
                this.button7.Click += new System.EventHandler(this.button7_Click);
                // 
                // textBox7
                // 
                this.textBox7.Location = new System.Drawing.Point(289, 47);
                this.textBox7.Name = "textBox7";
                this.textBox7.Size = new System.Drawing.Size(101, 20);
                this.textBox7.TabIndex = 44;
                this.textBox7.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label8
                // 
                this.label8.AutoSize = true;
                this.label8.Location = new System.Drawing.Point(471, 74);
                this.label8.Name = "label8";
                this.label8.Size = new System.Drawing.Size(46, 13);
                this.label8.TabIndex = 49;
                this.label8.Text = "Gold D3";
                // 
                // button8
                // 
                this.button8.Location = new System.Drawing.Point(396, 70);
                this.button8.Name = "button8";
                this.button8.Size = new System.Drawing.Size(69, 20);
                this.button8.TabIndex = 48;
                this.button8.Text = "SetGold";
                this.button8.UseVisualStyleBackColor = true;
                this.button8.Click += new System.EventHandler(this.button8_Click);
                // 
                // textBox8
                // 
                this.textBox8.Location = new System.Drawing.Point(289, 70);
                this.textBox8.Name = "textBox8";
                this.textBox8.Size = new System.Drawing.Size(101, 20);
                this.textBox8.TabIndex = 47;
                this.textBox8.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label9
                // 
                this.label9.AutoSize = true;
                this.label9.Location = new System.Drawing.Point(471, 100);
                this.label9.Name = "label9";
                this.label9.Size = new System.Drawing.Size(46, 13);
                this.label9.TabIndex = 52;
                this.label9.Text = "Gold D4";
                // 
                // button9
                // 
                this.button9.Location = new System.Drawing.Point(396, 96);
                this.button9.Name = "button9";
                this.button9.Size = new System.Drawing.Size(69, 20);
                this.button9.TabIndex = 51;
                this.button9.Text = "SetGold";
                this.button9.UseVisualStyleBackColor = true;
                this.button9.Click += new System.EventHandler(this.button9_Click);
                // 
                // textBox9
                // 
                this.textBox9.Location = new System.Drawing.Point(289, 96);
                this.textBox9.Name = "textBox9";
                this.textBox9.Size = new System.Drawing.Size(101, 20);
                this.textBox9.TabIndex = 50;
                this.textBox9.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // label10
                // 
                this.label10.AutoSize = true;
                this.label10.Location = new System.Drawing.Point(471, 126);
                this.label10.Name = "label10";
                this.label10.Size = new System.Drawing.Size(46, 13);
                this.label10.TabIndex = 55;
                this.label10.Text = "Gold D5";
                // 
                // button10
                // 
                this.button10.Location = new System.Drawing.Point(396, 122);
                this.button10.Name = "button10";
                this.button10.Size = new System.Drawing.Size(69, 20);
                this.button10.TabIndex = 54;
                this.button10.Text = "SetGold";
                this.button10.UseVisualStyleBackColor = true;
                this.button10.Click += new System.EventHandler(this.button10_Click);
                // 
                // textBox10
                // 
                this.textBox10.Location = new System.Drawing.Point(289, 122);
                this.textBox10.Name = "textBox10";
                this.textBox10.Size = new System.Drawing.Size(101, 20);
                this.textBox10.TabIndex = 53;
                this.textBox10.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
                // 
                // Form1
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.BackColor = System.Drawing.SystemColors.Highlight;
                this.ClientSize = new System.Drawing.Size(613, 235);
                this.ControlBox = false;
                this.Controls.Add(this.panel1);
                this.Controls.Add(this.linkLabel2);
                this.Controls.Add(this.linkLabel1);
                this.Controls.Add(this.buttonH);
                this.Controls.Add(this.buttonC);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Name = "Form1";
                this.Opacity = 0.85D;
                this.ShowIcon = false;
                this.Text = "Host_Hacks";
                this.TopMost = true;
                this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
                this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                this.panel1.ResumeLayout(false);
                this.panel1.PerformLayout();
                this.ResumeLayout(false);
                this.PerformLayout();

            }

            private System.Windows.Forms.Button buttonH;
            private System.Windows.Forms.LinkLabel linkLabel1;
            private System.Windows.Forms.LinkLabel linkLabel2;
            private System.Windows.Forms.Button buttonC;
            private System.Windows.Forms.Panel panel1;
            public System.Windows.Forms.Label label1;
            private System.Windows.Forms.Button button1;
            public System.Windows.Forms.TextBox textBox1;
            public System.Windows.Forms.Label label666;
            private System.Windows.Forms.Button button666;
            public System.Windows.Forms.TextBox textBox666;
            public System.Windows.Forms.CheckBox checkBox1;
            public System.Windows.Forms.Label label10;
            private System.Windows.Forms.Button button10;
            public System.Windows.Forms.TextBox textBox10;
            public System.Windows.Forms.Label label9;
            private System.Windows.Forms.Button button9;
            public System.Windows.Forms.TextBox textBox9;
            public System.Windows.Forms.Label label8;
            private System.Windows.Forms.Button button8;
            public System.Windows.Forms.TextBox textBox8;
            public System.Windows.Forms.Label label7;
            private System.Windows.Forms.Button button7;
            public System.Windows.Forms.TextBox textBox7;
            public System.Windows.Forms.Label label6;
            private System.Windows.Forms.Button button6;
            public System.Windows.Forms.TextBox textBox6;
            public System.Windows.Forms.Label label5;
            private System.Windows.Forms.Button button5;
            public System.Windows.Forms.TextBox textBox5;
            public System.Windows.Forms.Label label4;
            private System.Windows.Forms.Button button4;
            public System.Windows.Forms.TextBox textBox4;
            public System.Windows.Forms.Label label3;
            private System.Windows.Forms.Button button3;
            public System.Windows.Forms.TextBox textBox3;
            public System.Windows.Forms.Label label2;
            private System.Windows.Forms.Button button2;
            public System.Windows.Forms.TextBox textBox2;

            private void buttonH_Click(object sender, EventArgs e)
            {
                if (Width != 613)
                {
                    Width = 613; Height = 235;
                }
                else
                {
                    Width = 1; Height = 1;
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 10);
                    Win32.ShowWindow(Win32.FindWindow(null, "Dota 2"), 5);
                    Win32.SetForegroundWindow(Win32.FindWindow(null, "Dota 2"));
                }
            }

            private void buttonC_Click(object sender, EventArgs e)
            {
                IsFormClose = true;
                Close();
            }

            public static Boolean isNOPHpReg;
            public static long HpRegAddress;

            private void button666_Click(object sender, EventArgs e)
            {
                if (frm.textBox666.Text.Length > 0)
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    GHandle = P[0].Handle;
                    //Clipboard.SetText(frm.label666.Text);
                    var valueon = new byte[] { 0x50, 0x48, 0xA1, 0xB8, 0x08, 0xCE, 0xD6, 0xFE, 0x07, 0x00, 0x00, 0x48, 0x8B, 0x00, 0x48, 0x8B, 0x40, 0x48, 0x48, 0x05, 0x90, 0x05, 0x00, 0x00, 0x48, 0x3B, 0x18, 0x74, 0x0C, 0x90, 0x90, 0x90, 0x90, 0xF3, 0x0F, 0x11, 0xBB, 0xEC, 0x07, 0x00, 0x00, 0x58 };
                    var valueoff = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0xF4, 0x07, 0x00, 0x00 }; // Байты оригинальной команды
                    var patern = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0xF4, 0x07, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8B, 0xF4, 0x07, 0x00, 0x00 };
                    long offsetmodule = 0x01C508B8;
                    //MakeCave(valueon, valueoff, patern, offsetmodule);
                    if (isNOPHpReg)
                    {
                        UNNOP(HpRegAddress, valueoff);
                        isNOPHpReg = false;
                        frm.button666.Text = "SetHpReg";
                    }
                    else
                    {
                        HpRegAddress = MakeNOP(valueon, valueoff, patern, offsetmodule);
                        isNOPHpReg = true;
                        frm.button666.Text = "UnSetHpReg";
                        try
                        {
                            int bytesWritten; byte[] buffer; String s;
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Convert.ToSingle(textBox666.Text));
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x7F4 }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }
                    }
                }
             }

            private void button1_Click(object sender, EventArgs e)
            {
                if (frm.textBox1.Text.Length > 0)
                    label1.Text = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x18 }, false, Convert.ToInt32(textBox1.Text)).Value.ToString("X");
            }

            private void button2_Click(object sender, EventArgs e)
            {
                if (frm.textBox2.Text.Length > 0)
                    label2.Text = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0xA8 }, false, Convert.ToInt32(textBox2.Text)).Value.ToString("X");
            }

            private void button3_Click(object sender, EventArgs e)
            {
                if (frm.textBox3.Text.Length > 0)
                    label3.Text = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x138 }, false, Convert.ToInt32(textBox3.Text)).Value.ToString("X");
            }

            private void button4_Click(object sender, EventArgs e)
            {
                if (frm.textBox4.Text.Length > 0)
                    label4.Text = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x1C8 }, false, Convert.ToInt32(textBox4.Text)).Value.ToString("X");
            }

            private void button5_Click(object sender, EventArgs e)
            {
                if (frm.textBox5.Text.Length > 0)
                    label5.Text = Pointer("dota2", OffsetGoldR, new int[] { 0, 0x2c8, 0x258 }, false, Convert.ToInt32(textBox5.Text)).Value.ToString("X");
            }

            private void button6_Click(object sender, EventArgs e)
            {
                if (frm.textBox6.Text.Length > 0)
                    label6.Text = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x18 }, false, Convert.ToInt32(frm.textBox6.Text)).Value.ToString("N");
            }

            private void button7_Click(object sender, EventArgs e)
            {
                if (frm.textBox7.Text.Length > 0)
                    label7.Text = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0xA8 }, false, Convert.ToInt32(frm.textBox7.Text)).Value.ToString("N");
            }

            private void button8_Click(object sender, EventArgs e)
            {
                if (frm.textBox8.Text.Length > 0)
                    label8.Text = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x138 }, false, Convert.ToInt32(frm.textBox8.Text)).Value.ToString("N");
            }

            private void button9_Click(object sender, EventArgs e)
            {
                if (frm.textBox9.Text.Length > 0)
                    label9.Text = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x1C8 }, false, Convert.ToInt32(frm.textBox9.Text)).Value.ToString("N");
            }

            private void button10_Click(object sender, EventArgs e)
            {
                if (frm.textBox10.Text.Length > 0)
                    label10.Text = Pointer("dota2", OffsetGoldD, new int[] { 0, 0x2c8, 0x258 }, false, Convert.ToInt32(frm.textBox10.Text)).Value.ToString("N");
            }

            private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) // && (e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                /*only allow one decimal point
                if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }*/
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

        private static void playSound(string path)
        {
            System.Media.SoundPlayer player =
                new System.Media.SoundPlayer();
            String fullpath = Environment.CurrentDirectory;
            fullpath = fullpath.Remove(fullpath.Length - 10);
            fullpath += @"\dota\materials\sounds\" + path;
            player.SoundLocation = fullpath;
            player.Load();
            player.Play();
        }

        public static void UNNOP(long address, byte[] valueoff)
        {
            if (GHandle != IntPtr.Zero)
            {
                var dummy1 = 0;
                Win32.WriteProcessMemory(GHandle, (long)address, valueoff, (int)valueoff.Length, ref dummy1);
            }
        }

        public static long MakeNOP(byte[] valueon, byte[] valueoff, byte[] patern, long offsetmodule)
        {
            if (GHandle != IntPtr.Zero)
            {
                Sign signature;
                signature = FindWSignature(patern);
                long addressFrom = signature.AdressSignature;
                if (addressFrom != -1)
                {
                    Console.WriteLine(addressFrom.ToString("X"));
                    //playSound("FIND.wav");
                }
                else
                {
                    Console.WriteLine("HOST_HACKS: instruction NOT FINDED");
                    //playSound("NOFIND.wav");
                    return -1;
                }
                var buffer = new byte[valueoff.Length];
                for (var i = 0; i < valueoff.Length; i++)
                {
                    buffer[i] = 0x90;
                }
                var dummy1 = 0;
                Win32.WriteProcessMemory(GHandle, (long)addressFrom, buffer, (int)buffer.Length, ref dummy1);
                return addressFrom;
            }
            else return -1;
        }

        public static void MakeCave(byte[] valueon, byte[] valueoff, byte[] patern, long offsetmodule)
        {
            if (GHandle != IntPtr.Zero)
            {
                long caveAddress = (long)(Win32.VirtualAllocEx(GHandle, IntPtr.Zero, (IntPtr)(valueon.Length + 12), Win32.AllocationType.Commit, Win32.MemoryProtection.ExecuteReadWrite));
                Sign signature;
                signature = FindWSignature(patern);
                offsetmodule = offsetmodule + signature.ModuleAdress;
                var offs = BitConverter.GetBytes(offsetmodule);
                Array.Copy(offs, 0, valueon, 2, offs.Length);
                long addressFrom = signature.AdressSignature;
                if (addressFrom != -1)
                {
                    Console.WriteLine(addressFrom.ToString("X"));
                    //playSound("FIND.wav");
                }
                else
                {
                    Console.WriteLine("NOT FINDED");
                    //playSound("NOFIND.wav");
                    return;
                }
                long addressTo = addressFrom + valueoff.Length;
                var buffer = new byte[valueoff.Length];
                buffer[0] = 0x50;
                buffer[1] = 0x48; buffer[2] = 0xB8;
                var f = BitConverter.GetBytes(caveAddress);
                Array.Copy(f, 0, buffer, 3, f.Length);
                buffer[11] = 0xFF; buffer[12] = 0xE0;
                for (var i = 13; i < valueoff.Length; i++)
                {
                    buffer[i] = 0x90;
                }
                buffer[valueoff.Length - 1] = 0x58;
                var dummy = 0;
                var caveBuffer = new byte[valueon.Length + 12];
                Array.Copy(valueon, caveBuffer, valueon.Length);
                caveBuffer[valueon.Length] = 0x48; caveBuffer[valueon.Length + 1] = 0xB8;
                var retAddress = BitConverter.GetBytes(addressTo - 1);
                Array.Copy(retAddress, 0, caveBuffer, valueon.Length + 2, retAddress.Length);
                caveBuffer[caveBuffer.Length - 2] = 0xFF; caveBuffer[caveBuffer.Length - 1] = 0xE0;
                Win32.WriteProcessMemory(GHandle, (long)caveAddress, caveBuffer, (int)caveBuffer.Length, ref dummy);
                var dummy1 = 0;
                Win32.WriteProcessMemory(GHandle, (long)addressFrom, buffer, (int)buffer.Length, ref dummy1);
                //Win32.VirtualFreeEx((IntPtr)GHandle, (IntPtr)caveAddress, (UIntPtr)0, (uint)0x8000);
                Console.ReadKey();
            }
        }

        public struct Sign
        {
            public long AdressSignature;
            public long ModuleAdress;
        }

        static public Sign FindWSignature(byte[] pattern)
        {
            Sign GG_WP;
            GG_WP.ModuleAdress = -1;
            GG_WP.AdressSignature = -1;
            var moduleName = "server.dll";
            long startAddress = 0;
            long sizeToAllocate = 0;
            if (GHandle != IntPtr.Zero)
            {
                Process[] P = Process.GetProcessesByName("dota2");
                GHandle = P[0].Handle;
                foreach (ProcessModule m in P[0].Modules)
                {
                    if (m.ModuleName == moduleName)
                    {
                        startAddress = (long)m.BaseAddress;
                        GG_WP.ModuleAdress = startAddress;
                        sizeToAllocate = m.ModuleMemorySize;
                        break;
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
                                            GG_WP.AdressSignature = (long)result;
                                            return GG_WP;
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
            GG_WP.AdressSignature = -1;
            return GG_WP;
        }
        
        static Form1 frm = new Form1();
        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen || Game.IsWatchingGame) return;
            try
            {
                if (args.Msg == 0x0101)
                {
                    if (args.WParam == SubMenu.Item("HHKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                        if (IsFormClose)
                        {
                            Win32.PrintEncolored("HOST_HACKS: You close form! Reload script for openning!", ConsoleColor.Red);
                            return;
                        }
                        frm.Width = 613; frm.Height = 235;
                        frm.Show();
                    }
                    else if (args.WParam == SubMenu.Item("TPKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                        var player = ObjectMgr.LocalHero;
                        if (!Game.IsInGame || player == null) return;
                        Process[] P = Process.GetProcessesByName("dota2");
                        if (P.Length == 0) return;
                        int bytesWritten; byte[] buffer; String s;
                        try
                        {
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Game.MousePosition.X);
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0xD8 }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }

                        try
                        {
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Game.MousePosition.Y);
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0xDC }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);

                        }
                        catch { }
                        if (SubMenu.Item("AutoGO").GetValue<bool>())
                            player.Move(new SharpDX.Vector3(Game.MousePosition.X + 4, Game.MousePosition.Y, Game.MousePosition.Z));
                    }
					else if (args.WParam == SubMenu.Item("RHKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                        var player = ObjectMgr.LocalHero;
                        if (!Game.IsInGame || player == null) return;
                        Process[] P = Process.GetProcessesByName("dota2");
                        if (P.Length == 0) return;
                        int bytesWritten; byte[] buffer; String s;
                        try
                        {
                            frm.label666.Text = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x128 }, false, 99999).Value.ToString("X");
						}
                        catch { }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    Win32.PrintEncolored("Error: " + e.Source, ConsoleColor.Red);
                throw;
            }
        }

    }
}
