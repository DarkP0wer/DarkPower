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

	[DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress,
              byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
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
	    SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("RKey", "Restore Health/Mana Key").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("RH", "Resotre Helath").SetValue(true));
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("RM", "Resotre Mana").SetValue(true));
            SubMenu.AddItem(new Ensage.Common.Menu.MenuItem("EXPKey", "Set 500,000 EXP").SetValue(new Ensage.Common.Menu.KeyBind(96, Ensage.Common.Menu.KeyBindType.Press)));
            SubMenu.AddToMainMenu();
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            Game.OnWndProc += Game_OnGameWndProc;
        }

        static string OffsetGoldR = "server.dll+1D27E10";
        static string OffsetGoldD = "server.dll+1D27E18";
	static string OffsetPlayer = "server.dll+01D34650";

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
                buttonH = new System.Windows.Forms.Button();
                linkLabel1 = new System.Windows.Forms.LinkLabel();
                linkLabel2 = new System.Windows.Forms.LinkLabel();
                buttonC = new System.Windows.Forms.Button();
                panel1 = new System.Windows.Forms.Panel();
                label666 = new System.Windows.Forms.Label();
                button666 = new System.Windows.Forms.Button();
                textBox666 = new System.Windows.Forms.TextBox();
                checkBox1 = new System.Windows.Forms.CheckBox();
                label1 = new System.Windows.Forms.Label();
                button1 = new System.Windows.Forms.Button();
                textBox1 = new System.Windows.Forms.TextBox();
                label2 = new System.Windows.Forms.Label();
                button2 = new System.Windows.Forms.Button();
                textBox2 = new System.Windows.Forms.TextBox();
                label3 = new System.Windows.Forms.Label();
                button3 = new System.Windows.Forms.Button();
                textBox3 = new System.Windows.Forms.TextBox();
                label4 = new System.Windows.Forms.Label();
                button4 = new System.Windows.Forms.Button();
                textBox4 = new System.Windows.Forms.TextBox();
                label5 = new System.Windows.Forms.Label();
                button5 = new System.Windows.Forms.Button();
                textBox5 = new System.Windows.Forms.TextBox();
                label6 = new System.Windows.Forms.Label();
                button6 = new System.Windows.Forms.Button();
                textBox6 = new System.Windows.Forms.TextBox();
                label7 = new System.Windows.Forms.Label();
                button7 = new System.Windows.Forms.Button();
                textBox7 = new System.Windows.Forms.TextBox();
                label8 = new System.Windows.Forms.Label();
                button8 = new System.Windows.Forms.Button();
                textBox8 = new System.Windows.Forms.TextBox();
                label9 = new System.Windows.Forms.Label();
                button9 = new System.Windows.Forms.Button();
                textBox9 = new System.Windows.Forms.TextBox();
                label10 = new System.Windows.Forms.Label();
                button10 = new System.Windows.Forms.Button();
                textBox10 = new System.Windows.Forms.TextBox();
                textBoxStr = new System.Windows.Forms.TextBox();
                buttonStr = new System.Windows.Forms.Button();
                buttonAlg = new System.Windows.Forms.Button();
                textBoxAlg = new System.Windows.Forms.TextBox();
                buttonInt = new System.Windows.Forms.Button();
                textBoxInt = new System.Windows.Forms.TextBox();
                panel1.SuspendLayout();
                SuspendLayout();
                // 
                // buttonH
                // 
                buttonH.BackColor = System.Drawing.SystemColors.ControlLightLight;
                buttonH.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                buttonH.Location = new System.Drawing.Point(2, 2);
                buttonH.Name = "buttonH";
                buttonH.Size = new System.Drawing.Size(80, 21);
                buttonH.TabIndex = 4;
                buttonH.Text = "HH HIDE";
                buttonH.UseVisualStyleBackColor = true;
                buttonH.Click += new System.EventHandler(buttonH_Click);
                // 
                // linkLabel1
                // 
                linkLabel1.AutoSize = true;
                linkLabel1.LinkColor = System.Drawing.Color.White;
                linkLabel1.Location = new System.Drawing.Point(88, 6);
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
                linkLabel2.Location = new System.Drawing.Point(167, 6);
                linkLabel2.Name = "linkLabel2";
                linkLabel2.Size = new System.Drawing.Size(96, 13);
                linkLabel2.TabIndex = 7;
                linkLabel2.TabStop = true;
                linkLabel2.Text = "GitHub DarkP0wer";
                linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel2_LinkClicked);
                // 
                // buttonC
                // 
                buttonC.BackColor = System.Drawing.SystemColors.ControlLightLight;
                buttonC.Location = new System.Drawing.Point(548, 2);
                buttonC.Name = "buttonC";
                buttonC.Size = new System.Drawing.Size(58, 24);
                buttonC.TabIndex = 8;
                buttonC.Text = "Close";
                buttonC.UseVisualStyleBackColor = true;
                buttonC.Click += new System.EventHandler(buttonC_Click);
                // 
                // panel1
                // 
                panel1.BackColor = System.Drawing.Color.DeepSkyBlue;
                panel1.Controls.Add(buttonInt);
                panel1.Controls.Add(textBoxInt);
                panel1.Controls.Add(buttonAlg);
                panel1.Controls.Add(textBoxAlg);
                panel1.Controls.Add(buttonStr);
                panel1.Controls.Add(textBoxStr);
                panel1.Controls.Add(label10);
                panel1.Controls.Add(button10);
                panel1.Controls.Add(textBox10);
                panel1.Controls.Add(label9);
                panel1.Controls.Add(button9);
                panel1.Controls.Add(textBox9);
                panel1.Controls.Add(label8);
                panel1.Controls.Add(button8);
                panel1.Controls.Add(textBox8);
                panel1.Controls.Add(label7);
                panel1.Controls.Add(button7);
                panel1.Controls.Add(textBox7);
                panel1.Controls.Add(label6);
                panel1.Controls.Add(button6);
                panel1.Controls.Add(textBox6);
                panel1.Controls.Add(label5);
                panel1.Controls.Add(button5);
                panel1.Controls.Add(textBox5);
                panel1.Controls.Add(label4);
                panel1.Controls.Add(button4);
                panel1.Controls.Add(textBox4);
                panel1.Controls.Add(label3);
                panel1.Controls.Add(button3);
                panel1.Controls.Add(textBox3);
                panel1.Controls.Add(label2);
                panel1.Controls.Add(button2);
                panel1.Controls.Add(textBox2);
                panel1.Controls.Add(label666);
                panel1.Controls.Add(button666);
                panel1.Controls.Add(textBox666);
                panel1.Controls.Add(checkBox1);
                panel1.Controls.Add(label1);
                panel1.Controls.Add(button1);
                panel1.Controls.Add(textBox1);
                panel1.Location = new System.Drawing.Point(12, 29);
                panel1.Name = "panel1";
                panel1.Size = new System.Drawing.Size(594, 199);
                panel1.TabIndex = 14;
                panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
                // 
                // label666
                // 
                label666.AutoSize = true;
                label666.Location = new System.Drawing.Point(185, 178);
                label666.Name = "label666";
                label666.Size = new System.Drawing.Size(47, 13);
                label666.TabIndex = 28;
                label666.Text = "Player Adress";
                // 
                // button666
                // 
                button666.Location = new System.Drawing.Point(110, 174);
                button666.Name = "button666";
                button666.Size = new System.Drawing.Size(69, 20);
                button666.TabIndex = 27;
                button666.Text = "SetHpRegenaration";
                button666.UseVisualStyleBackColor = true;
                button666.Click += new System.EventHandler(button666_Click);
                // 
                // textBox666
                // 
                textBox666.Location = new System.Drawing.Point(3, 174);
                textBox666.Name = "textBox666";
                textBox666.Size = new System.Drawing.Size(101, 20);
                textBox666.TabIndex = 26;
                // 
                // checkBox1
                // 
                checkBox1.AutoSize = true;
                checkBox1.Checked = true;
                checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
                checkBox1.Location = new System.Drawing.Point(3, 3);
                checkBox1.Name = "checkBox1";
                checkBox1.Size = new System.Drawing.Size(92, 17);
                checkBox1.TabIndex = 22;
                checkBox1.Text = "Check Values";
                checkBox1.UseVisualStyleBackColor = true;
                // 
                // label1
                // 
                label1.AutoSize = true;
                label1.Location = new System.Drawing.Point(185, 25);
                label1.Name = "label1";
                label1.Size = new System.Drawing.Size(46, 13);
                label1.TabIndex = 17;
                label1.Text = "Gold R1";
                // 
                // button1
                // 
                button1.Location = new System.Drawing.Point(110, 21);
                button1.Name = "button1";
                button1.Size = new System.Drawing.Size(69, 20);
                button1.TabIndex = 16;
                button1.Text = "SetGold";
                button1.UseVisualStyleBackColor = true;
                button1.Click += new System.EventHandler(button1_Click);
                // 
                // textBox1
                // 
                textBox1.Location = new System.Drawing.Point(3, 21);
                textBox1.Name = "textBox1";
                textBox1.Size = new System.Drawing.Size(101, 20);
                textBox1.TabIndex = 14;
                textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label2
                // 
                label2.AutoSize = true;
                label2.Location = new System.Drawing.Point(185, 51);
                label2.Name = "label2";
                label2.Size = new System.Drawing.Size(46, 13);
                label2.TabIndex = 31;
                label2.Text = "Gold R2";
                // 
                // button2
                // 
                button2.Location = new System.Drawing.Point(110, 47);
                button2.Name = "button2";
                button2.Size = new System.Drawing.Size(69, 20);
                button2.TabIndex = 30;
                button2.Text = "SetGold";
                button2.UseVisualStyleBackColor = true;
                button2.Click += new System.EventHandler(button2_Click);
                // 
                // textBox2
                // 
                textBox2.Location = new System.Drawing.Point(3, 47);
                textBox2.Name = "textBox2";
                textBox2.Size = new System.Drawing.Size(101, 20);
                textBox2.TabIndex = 29;
                textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label3
                // 
                label3.AutoSize = true;
                label3.Location = new System.Drawing.Point(185, 77);
                label3.Name = "label3";
                label3.Size = new System.Drawing.Size(46, 13);
                label3.TabIndex = 34;
                label3.Text = "Gold R3";
                // 
                // button3
                // 
                button3.Location = new System.Drawing.Point(110, 73);
                button3.Name = "button3";
                button3.Size = new System.Drawing.Size(69, 20);
                button3.TabIndex = 33;
                button3.Text = "SetGold";
                button3.UseVisualStyleBackColor = true;
                button3.Click += new System.EventHandler(button3_Click);
                // 
                // textBox3
                // 
                textBox3.Location = new System.Drawing.Point(3, 73);
                textBox3.Name = "textBox3";
                textBox3.Size = new System.Drawing.Size(101, 20);
                textBox3.TabIndex = 32;
                textBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label4
                // 
                label4.AutoSize = true;
                label4.Location = new System.Drawing.Point(185, 103);
                label4.Name = "label4";
                label4.Size = new System.Drawing.Size(46, 13);
                label4.TabIndex = 37;
                label4.Text = "Gold R4";
                // 
                // button4
                // 
                button4.Location = new System.Drawing.Point(110, 99);
                button4.Name = "button4";
                button4.Size = new System.Drawing.Size(69, 20);
                button4.TabIndex = 36;
                button4.Text = "SetGold";
                button4.UseVisualStyleBackColor = true;
                button4.Click += new System.EventHandler(button4_Click);
                // 
                // textBox4
                // 
                textBox4.Location = new System.Drawing.Point(3, 99);
                textBox4.Name = "textBox4";
                textBox4.Size = new System.Drawing.Size(101, 20);
                textBox4.TabIndex = 35;
                textBox4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label5
                // 
                label5.AutoSize = true;
                label5.Location = new System.Drawing.Point(185, 129);
                label5.Name = "label5";
                label5.Size = new System.Drawing.Size(46, 13);
                label5.TabIndex = 40;
                label5.Text = "Gold R5";
                // 
                // button5
                // 
                button5.Location = new System.Drawing.Point(110, 125);
                button5.Name = "button5";
                button5.Size = new System.Drawing.Size(69, 20);
                button5.TabIndex = 39;
                button5.Text = "SetGold";
                button5.UseVisualStyleBackColor = true;
                button5.Click += new System.EventHandler(button5_Click);
                // 
                // textBox5
                // 
                textBox5.Location = new System.Drawing.Point(3, 125);
                textBox5.Name = "textBox5";
                textBox5.Size = new System.Drawing.Size(101, 20);
                textBox5.TabIndex = 38;
                textBox5.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label6
                // 
                label6.AutoSize = true;
                label6.Location = new System.Drawing.Point(471, 25);
                label6.Name = "label6";
                label6.Size = new System.Drawing.Size(46, 13);
                label6.TabIndex = 43;
                label6.Text = "Gold D1";
                // 
                // button6
                // 
                button6.Location = new System.Drawing.Point(396, 21);
                button6.Name = "button6";
                button6.Size = new System.Drawing.Size(69, 20);
                button6.TabIndex = 42;
                button6.Text = "SetGold";
                button6.UseVisualStyleBackColor = true;
                button6.Click += new System.EventHandler(button6_Click);
                // 
                // textBox6
                // 
                textBox6.Location = new System.Drawing.Point(289, 21);
                textBox6.Name = "textBox6";
                textBox6.Size = new System.Drawing.Size(101, 20);
                textBox6.TabIndex = 41;
                textBox6.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label7
                // 
                label7.AutoSize = true;
                label7.Location = new System.Drawing.Point(471, 51);
                label7.Name = "label7";
                label7.Size = new System.Drawing.Size(46, 13);
                label7.TabIndex = 46;
                label7.Text = "Gold D2";
                // 
                // button7
                // 
                button7.Location = new System.Drawing.Point(396, 47);
                button7.Name = "button7";
                button7.Size = new System.Drawing.Size(69, 20);
                button7.TabIndex = 45;
                button7.Text = "SetGold";
                button7.UseVisualStyleBackColor = true;
                button7.Click += new System.EventHandler(button7_Click);
                // 
                // textBox7
                // 
                textBox7.Location = new System.Drawing.Point(289, 47);
                textBox7.Name = "textBox7";
                textBox7.Size = new System.Drawing.Size(101, 20);
                textBox7.TabIndex = 44;
                textBox7.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label8
                // 
                label8.AutoSize = true;
                label8.Location = new System.Drawing.Point(471, 74);
                label8.Name = "label8";
                label8.Size = new System.Drawing.Size(46, 13);
                label8.TabIndex = 49;
                label8.Text = "Gold D3";
                // 
                // button8
                // 
                button8.Location = new System.Drawing.Point(396, 70);
                button8.Name = "button8";
                button8.Size = new System.Drawing.Size(69, 20);
                button8.TabIndex = 48;
                button8.Text = "SetGold";
                button8.UseVisualStyleBackColor = true;
                button8.Click += new System.EventHandler(button8_Click);
                // 
                // textBox8
                // 
                textBox8.Location = new System.Drawing.Point(289, 70);
                textBox8.Name = "textBox8";
                textBox8.Size = new System.Drawing.Size(101, 20);
                textBox8.TabIndex = 47;
                textBox8.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label9
                // 
                label9.AutoSize = true;
                label9.Location = new System.Drawing.Point(471, 100);
                label9.Name = "label9";
                label9.Size = new System.Drawing.Size(46, 13);
                label9.TabIndex = 52;
                label9.Text = "Gold D4";
                // 
                // button9
                // 
                button9.Location = new System.Drawing.Point(396, 96);
                button9.Name = "button9";
                button9.Size = new System.Drawing.Size(69, 20);
                button9.TabIndex = 51;
                button9.Text = "SetGold";
                button9.UseVisualStyleBackColor = true;
                button9.Click += new System.EventHandler(button9_Click);
                // 
                // textBox9
                // 
                textBox9.Location = new System.Drawing.Point(289, 96);
                textBox9.Name = "textBox9";
                textBox9.Size = new System.Drawing.Size(101, 20);
                textBox9.TabIndex = 50;
                textBox9.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // label10
                // 
                label10.AutoSize = true;
                label10.Location = new System.Drawing.Point(471, 126);
                label10.Name = "label10";
                label10.Size = new System.Drawing.Size(46, 13);
                label10.TabIndex = 55;
                label10.Text = "Gold D5";
                // 
                // button10
                // 
                button10.Location = new System.Drawing.Point(396, 122);
                button10.Name = "button10";
                button10.Size = new System.Drawing.Size(69, 20);
                button10.TabIndex = 54;
                button10.Text = "SetGold";
                button10.UseVisualStyleBackColor = true;
                button10.Click += new System.EventHandler(button10_Click);
                // 
                // textBox10
                // 
                textBox10.Location = new System.Drawing.Point(289, 122);
                textBox10.Name = "textBox10";
                textBox10.Size = new System.Drawing.Size(101, 20);
                textBox10.TabIndex = 53;
                textBox10.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                //
                // textBoxStr
                // 
                textBoxStr.Location = new System.Drawing.Point(289, 174);
                textBoxStr.Name = "textBoxStr";
                textBoxStr.Size = new System.Drawing.Size(38, 20);
                textBoxStr.TabIndex = 56;
                textBoxStr.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // buttonStr
                // 
                buttonStr.Location = new System.Drawing.Point(333, 175);
                buttonStr.Name = "buttonStr";
                buttonStr.Size = new System.Drawing.Size(44, 20);
                buttonStr.TabIndex = 57;
                buttonStr.Text = "SetStr";
                buttonStr.UseVisualStyleBackColor = true;
                buttonStr.Click += new System.EventHandler(buttonStr_Click);
                // 
                // buttonAlg
                // 
                buttonAlg.Location = new System.Drawing.Point(427, 175);
                buttonAlg.Name = "buttonAlg";
                buttonAlg.Size = new System.Drawing.Size(49, 21);
                buttonAlg.TabIndex = 59;
                buttonAlg.Text = "SetAlg";
                buttonAlg.UseVisualStyleBackColor = true;
                buttonAlg.Click += new System.EventHandler(buttonAlg_Click);
                // 
                // textBoxAlg
                // 
                textBoxAlg.Location = new System.Drawing.Point(383, 174);
                textBoxAlg.Name = "textBoxAlg";
                textBoxAlg.Size = new System.Drawing.Size(38, 20);
                textBoxAlg.TabIndex = 58;
                textBoxAlg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // buttonInt
                // 
                buttonInt.Location = new System.Drawing.Point(523, 174);
                buttonInt.Name = "buttonInt";
                buttonInt.Size = new System.Drawing.Size(49, 21);
                buttonInt.TabIndex = 61;
                buttonInt.Text = "SetInt";
                buttonInt.UseVisualStyleBackColor = true;
                buttonInt.Click += new System.EventHandler(buttonInt_Click);
                // 
                // textBoxInt
                // 
                textBoxInt.Location = new System.Drawing.Point(479, 175);
                textBoxInt.Name = "textBoxInt";
                textBoxInt.Size = new System.Drawing.Size(38, 20);
                textBoxInt.TabIndex = 60;
                textBoxInt.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
                // 
                // Form1
                // 
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                BackColor = System.Drawing.SystemColors.Highlight;
                ClientSize = new System.Drawing.Size(613, 235);
                ControlBox = false;
                Controls.Add(panel1);
                Controls.Add(linkLabel2);
                Controls.Add(linkLabel1);
                Controls.Add(buttonH);
                Controls.Add(buttonC);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                Name = "Form1";
                Opacity = 0.85D;
                ShowIcon = false;
                Text = "Host_Hacks";
                TopMost = true;
                FormClosed += new System.Windows.Forms.FormClosedEventHandler(Form1_FormClosed);
                MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
                panel1.ResumeLayout(false);
                panel1.PerformLayout();
                ResumeLayout(false);
                PerformLayout();

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
            private System.Windows.Forms.Button buttonStr;
            public System.Windows.Forms.TextBox textBoxStr;
            private System.Windows.Forms.Button buttonAlg;
            public System.Windows.Forms.TextBox textBoxAlg;
            private System.Windows.Forms.Button buttonInt;
            public System.Windows.Forms.TextBox textBoxInt;

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
                    var valueoff = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0x04, 0x08, 0x00, 0x00 }; // Байты оригинальной команды
                    var patern = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0x04, 0x08, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8B, 0x04, 0x08, 0x00, 0x00 };
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
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x804 }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }
                    }
                }
             }

            public static Boolean isNOPStr;
            public static long StrAddress;

            private void buttonStr_Click(object sender, EventArgs e)
            {
                if (frm.textBoxStr.Text.Length > 0)
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    GHandle = P[0].Handle;
                    var valueon = new byte[] { 0x50, 0x48, 0xA1, 0xB8, 0x08, 0xCE, 0xD6, 0x11, 0x08, 0x00, 0x00, 0x48, 0x8B, 0x00, 0x48, 0x8B, 0x40, 0x48, 0x48, 0x05, 0x90, 0x05, 0x00, 0x00, 0x48, 0x3B, 0x18, 0x74, 0x0C, 0x90, 0x90, 0x90, 0x90, 0xF3, 0x0F, 0x11, 0xBB, 0xEC, 0x07, 0x00, 0x00, 0x58 };
                    var valueoff = new byte[] { 0xF3, 0x0F, 0x11, 0xB7, 0x08, 0x11, 0x00, 0x00 }; // Байты оригинальной команды
                    var patern = new byte[] { 0xF3, 0x0F, 0x11, 0xB7, 0x08, 0x11, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8F, 0x08, 0x11, 0x00, 0x00 };
                    long offsetmodule = 0x01C508B8;
                    //MakeCave(valueon, valueoff, patern, offsetmodule);
                    if (isNOPStr)
                    {
                        UNNOP(StrAddress, valueoff);
                        isNOPStr = false;
                        frm.buttonStr.Text = "SetStr";
                    }
                    else
                    {
                        StrAddress = MakeNOP(valueon, valueoff, patern, offsetmodule);
                        isNOPStr = true;
                        frm.buttonStr.Text = "UnSetStr";
                        try
                        {
                            int bytesWritten; byte[] buffer; String s;
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Convert.ToSingle(textBoxStr.Text));
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x1108 }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }
                    }
                }
            }

            public static Boolean isNOPAlg;
            public static long AlgAddress;

            private void buttonAlg_Click(object sender, EventArgs e)
            {
                if (frm.textBoxAlg.Text.Length > 0)
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    GHandle = P[0].Handle;
                    var valueon = new byte[] { 0x50, 0x48, 0xA1, 0xB8, 0x08, 0xCE, 0xD6, 0xFE, 0x07, 0x00, 0x00, 0x48, 0x8B, 0x00, 0x48, 0x8B, 0x40, 0x48, 0x48, 0x05, 0x90, 0x05, 0x00, 0x00, 0x48, 0x3B, 0x18, 0x74, 0x0C, 0x90, 0x90, 0x90, 0x90, 0xF3, 0x0F, 0x11, 0xBB, 0xEC, 0x07, 0x00, 0x00, 0x58 };
                    var valueoff = new byte[] { 0xF3, 0x0F, 0x11, 0xBB, 0x0C, 0x11, 0x00, 0x00 }; // Байты оригинальной команды
                    var patern = new byte[] { 0xF3, 0x0F, 0x11, 0xBB, 0x0C, 0x11, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8B, 0x0C, 0x11, 0x00, 0x00 };
                    long offsetmodule = 0x01C508B8;
                    //MakeCave(valueon, valueoff, patern, offsetmodule);
                    if (isNOPAlg)
                    {
                        UNNOP(AlgAddress, valueoff);
                        isNOPAlg = false;
                        frm.buttonAlg.Text = "SetAlg";
                    }
                    else
                    {
                        AlgAddress = MakeNOP(valueon, valueoff, patern, offsetmodule);
                        isNOPAlg = true;
                        frm.buttonAlg.Text = "UnSetAlg";
                        try
                        {
                            int bytesWritten; byte[] buffer; String s;
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Convert.ToSingle(textBoxAlg.Text));
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x110C }, true, 1).Adress.ToString("X");
                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }
                    }
                }
            }

            public static Boolean isNOPInt;
            public static long IntAddress;

            private void buttonInt_Click(object sender, EventArgs e)
            {
                if (frm.textBoxInt.Text.Length > 0)
                {
                    Process[] P = Process.GetProcessesByName("dota2");
                    if (P.Length == 0) return;
                    GHandle = P[0].Handle;
                    var valueon = new byte[] { 0x50, 0x48, 0xA1, 0xB8, 0x08, 0xCE, 0xD6, 0xFE, 0x07, 0x00, 0x00, 0x48, 0x8B, 0x00, 0x48, 0x8B, 0x40, 0x48, 0x48, 0x05, 0x90, 0x05, 0x00, 0x00, 0x48, 0x3B, 0x18, 0x74, 0x0C, 0x90, 0x90, 0x90, 0x90, 0xF3, 0x0F, 0x11, 0xBB, 0xEC, 0x07, 0x00, 0x00, 0x58 };
                    var valueoff = new byte[] { 0xF3, 0x0F, 0x11, 0xBB, 0x10, 0x11, 0x00, 0x00 }; // Байты оригинальной команды
                    var patern = new byte[] { 0xF3, 0x0F, 0x11, 0xBB, 0x10, 0x11, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8B, 0x10, 0x11, 0x00, 0x00 };
                    long offsetmodule = 0x01C508B8;
                    //MakeCave(valueon, valueoff, patern, offsetmodule);
                    if (isNOPInt)
                    {
                        UNNOP(IntAddress, valueoff);
                        isNOPInt = false;
                        frm.buttonInt.Text = "SetInt";
                    }
                    else
                    {
                        IntAddress = MakeNOP(valueon, valueoff, patern, offsetmodule);
                        isNOPInt = true;
                        frm.buttonInt.Text = "UnSetInt";
                        try
                        {
                            int bytesWritten; byte[] buffer; String s;
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Convert.ToSingle(textBoxInt.Text));
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x1110 }, true, 1).Adress.ToString("X");
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
                    else if (args.WParam == SubMenu.Item("EXPKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                    	var player = ObjectMgr.LocalHero;
                        if (!Game.IsInGame || player == null) return;
                        Process[] P = Process.GetProcessesByName("dota2");
                        if (P.Length == 0) return;
	                try
	                {
	                	frm.label1.Text = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x1188 }, false, 500000).Value.ToString("X");
	                }
	                catch { }
                    }
                    else if (args.WParam == SubMenu.Item("TPKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                        var player = ObjectMgr.LocalHero;
                        if (!Game.IsInGame || player == null) return;
                        int bytesWritten; byte[] buffer; String s;
                        try
                        {
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Game.MousePosition.X);
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0xD8 }, true, 1).Adress.ToString("X");
                            WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        }
                        catch { }

                        try
                        {
                            bytesWritten = 0;
                            buffer = BitConverter.GetBytes(Game.MousePosition.Y);
                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0xDC }, true, 1).Adress.ToString("X");
                            WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);

                        }
                        catch { }
                        if (SubMenu.Item("AutoGO").GetValue<bool>())
                            player.Move(new SharpDX.Vector3(Game.MousePosition.X + 4, Game.MousePosition.Y, Game.MousePosition.Z));
                    }
					else if (args.WParam == SubMenu.Item("RKey").GetValue<Ensage.Common.Menu.KeyBind>().Key)
                    {
                        var player = ObjectMgr.LocalHero;
                        if (!Game.IsInGame || player == null) return;
                        Process[] P = Process.GetProcessesByName("dota2");
                        if (P.Length == 0) return;
                        int bytesWritten; byte[] buffer; String s;
                        if(SubMenu.Item("RH").GetValue<bool>())
                        {
                        	try
                        	{
                        	 frm.label666.Text = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x128 }, false, 99999).Value.ToString("X");
				}
                        	catch { }
                        }
                        
                        if(SubMenu.Item("RM").GetValue<bool>())
                        {
                        	try
                        	{
	                            bytesWritten = 0;
	                            float mana = 99999;
	                            buffer = BitConverter.GetBytes(mana);
	                            s = Pointer("dota2", OffsetPlayer, new int[] { 0, 0x0, 0x48, 0x590, 0x7f0 }, true, 1).Adress.ToString("X");
	                            Win32.WriteProcessMemory(P[0].Handle, long.Parse(s, NumberStyles.HexNumber), buffer, buffer.Length, ref bytesWritten);
                        	}
                        	catch { }
                        }
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
