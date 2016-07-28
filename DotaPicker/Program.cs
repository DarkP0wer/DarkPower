using System;
using SharpDX;
using SharpDX.Direct3D9;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using System.Windows.Forms;
using System.Data;
using Newtonsoft.Json;


namespace DotaPicker
{
    class Program
    {
        #region Menu

        public static Point[] xy1 = new Point[1];
        public static Boolean[] IsOnTarget = new Boolean[1];
        public static Point[] xy2 = new Point[1];
        public static string[] str = new string[1];
        public static Font[] _font = new Font[1];
        public static Color[] text_color = new Color[1];
        public static int rect_w, rect_h; static Boolean IsMenuOpen = false;

        public static void Add(string text, Color s, int text_height)
        {
            Array.Resize<Font>(ref _font, _font.Length + 1);
            Array.Resize<Point>(ref xy1, xy1.Length + 1);
            Array.Resize<Point>(ref xy2, xy2.Length + 1);
            Array.Resize<string>(ref str, str.Length + 1);
            Array.Resize<Color>(ref text_color, text_color.Length + 1);
            Array.Resize<Boolean>(ref IsOnTarget, IsOnTarget.Length + 1);

            int n = _font.Length - 1;
            var pos1 = new Vector2(xy1[0].X, xy2[n - 1].Y);
            var pos2 = new Vector2(xy2[0].X, (xy2[0].Y - xy1[0].Y) * n + xy1[0].Y);

            IsOnTarget[n] = false;
            xy1[n].X = xy1[0].X;
            xy2[n].X = xy2[0].X;
            xy1[n].Y = xy2[n - 1].Y;
            xy2[n].Y = (xy2[0].Y - xy1[0].Y) + xy2[n - 1].Y;
            text_color[n] = s;
            str[n] = text;

            _font[n] = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = text_height,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               }
            );

            //Drawing.DrawRect(pos1, pos2, new Color(0xE4, 0xDD, 0xDD, 0x20));
            _font[n].DrawText(null, text, xy1[n].X + 5, (rect_w % 2) + xy1[n].Y, s);

        }

        public static void Menu_Label1(int x1, int y1, string text, Color s, int rect_width, int rect_heigth, int text_height)
        {
            rect_w = rect_width; rect_h = rect_heigth; text_color[0] = s;
            xy1[0].X = x1;
            xy2[0].X = x1 + rect_width;
            xy1[0].Y = y1;
            xy2[0].Y = y1 + rect_heigth;
            str[0] = text;
            IsOnTarget[0] = false; IsMenuOpen = false;
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                return;
            _font[0] = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = text_height,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               }
            );
        }

        #endregion


        static Form1 frm = new Form1();

        static void Main(string[] args)
        {
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnWndProc += Game_OnGameWndProc;
            IsMenuOpen = false;
            Menu_Label1(30, 50, "SHOW WebBrowser", Color.Red, 150, 20, 17);
            for (int i = 0; i < 10; i++) HeroNames[i] = "NULL";      
        }

        private static double oldOpacity = 1.0;

        public partial class Form1 : Form
        {

            private System.ComponentModel.IContainer components = null;
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                    Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    key.SetValue(appName, (UInt32)value, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }

            private void SetBrowserFeatureControl()
            {
                // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

                // FeatureControl settings are per-process
                var fileName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                // make the control is not running inside Visual Studio Designer
                if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                    return;

                SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
                SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
                SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
                SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
            }

            private UInt32 GetBrowserEmulationMode()
            {
                int browserVersion = 7;
                using (var ieKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                    Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree,
                    System.Security.AccessControl.RegistryRights.QueryValues))
                {
                    var version = ieKey.GetValue("svcVersion");
                    if (null == version)
                    {
                        version = ieKey.GetValue("Version");
                        if (null == version)
                            throw new ApplicationException("Microsoft Internet Explorer is required!");
                    }
                    int.TryParse(version.ToString().Split('.')[0], out browserVersion);
                }

                UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. Default value for Internet Explorer 11.
                switch (browserVersion)
                {
                    case 7:
                        mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                        break;
                    case 8:
                        mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                        break;
                    case 9:
                        mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                        break;
                    case 10:
                        mode = 10000; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 mode. Default value for Internet Explorer 10.
                        break;
                    default:
                        // use IE11 mode by default
                        break;
                }

                return mode;
            }

            public Form1()
            {
            SetBrowserFeatureControl();
                InitializeComponent();
            }

            private void Form1_FormClosing(object sender, FormClosingEventArgs e)
            {
                e.Cancel = true;
                hideToolStripMenuItem.PerformClick();
            }

            private void toolStripMenuItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    if (toolStripTextBox1.Text.IndexOf("http") > -1)
                        webBrowser1.Navigate(toolStripTextBox1.Text);
                    else
                        webBrowser1.Navigate("https://www.google.com/#q=" + toolStripTextBox1.Text);
                }
                catch { }
            }

            private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == (Char)Keys.Enter) toolStripMenuItem1.PerformClick();
            }

            private void sToolStripMenuItem_Click(object sender, EventArgs e)
            {
                panel2.BringToFront();
                panel2.Visible = !panel2.Visible;
            }

            private void dotapickercomToolStripMenuItem_Click(object sender, EventArgs e)
            {
                webBrowser1.Navigate("http://dotapicker.com/");
            }

            private void dotabuffcomToolStripMenuItem_Click(object sender, EventArgs e)
            {
                webBrowser1.Navigate("http://www.dotabuff.com/");
            }

            private void truepickercomrucapToolStripMenuItem_Click(object sender, EventArgs e)
            {
                webBrowser1.Navigate("http://truepicker.com/en/cap/");
            }

            private void trackBar1_Scroll(object sender, EventArgs e)
            {
                this.Opacity = ((double)(trackBar1.Value) / 100.0);
            }

            private void label2_Click(object sender, EventArgs e)
            {
                panel2.Visible = false;
            }

            private void label2_MouseEnter(object sender, EventArgs e)
            {
                label2.ForeColor = System.Drawing.Color.Red;
            }

            private void label2_MouseLeave(object sender, EventArgs e)
            {
                label2.ForeColor = System.Drawing.Color.Lime;
            }

            private void joToolStripMenuItem_Click(object sender, EventArgs e)
            {
                webBrowser1.Navigate("http://www.joduska.me");
            }

            private void hideToolStripMenuItem_Click(object sender, EventArgs e)
            {
                oldOpacity = Opacity;
                this.Opacity = 0;
                str[0] = "Show WebBrowser";
            }

            private void googlecomToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    
                }
                catch { }
            }

            private void trackBar2_Scroll(object sender, EventArgs e)
            {
                label3.Text = "Time: " + trackBar2.Value + " sec";
                timer1.Interval = trackBar2.Value * 1000;
            }

            private void checkBox1_CheckedChanged(object sender, EventArgs e)
            {
                label3.Visible = !label3.Visible;
                trackBar2.Visible = !trackBar2.Visible;
                timer1.Enabled = !timer1.Enabled;
            }

            private void darkPowerToolStripMenuItem_Click(object sender, EventArgs e)
            {
                webBrowser1.Navigate("https://github.com/DarkP0wer/DarkPower/");
            }

            private void timer1_Tick(object sender, EventArgs e)
            {
                CheckPlayerHero();
            }

             private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sitesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dotapickercomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dotabuffcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.truepickercomrucapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.googlecomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkPowerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.WindowText;
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.trackBar2);
            this.panel2.Controls.Add(this.checkBox1);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.trackBar1);
            this.panel2.Location = new System.Drawing.Point(3, 30);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(595, 70);
            this.panel2.TabIndex = 5;
            this.panel2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.Color.Green;
            this.label3.Location = new System.Drawing.Point(111, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Time: 3 sec";
            this.label3.Visible = false;
            // 
            // trackBar2
            // 
            this.trackBar2.LargeChange = 1;
            this.trackBar2.Location = new System.Drawing.Point(190, 31);
            this.trackBar2.Minimum = 1;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(308, 45);
            this.trackBar2.TabIndex = 4;
            this.trackBar2.Value = 3;
            this.trackBar2.Visible = false;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBox1.ForeColor = System.Drawing.Color.Green;
            this.checkBox1.Location = new System.Drawing.Point(6, 35);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(95, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Auto update";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.Color.Green;
            this.label2.Location = new System.Drawing.Point(577, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "X";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            this.label2.MouseEnter += new System.EventHandler(this.label2_MouseEnter);
            this.label2.MouseLeave += new System.EventHandler(this.label2_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.Green;
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Opacity:";
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 1;
            this.trackBar1.Location = new System.Drawing.Point(55, 3);
            this.trackBar1.Maximum = 99;
            this.trackBar1.Minimum = 25;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(443, 45);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.Value = 99;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Highlight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1,
            this.toolStripMenuItem1,
            this.sToolStripMenuItem,
            this.sitesToolStripMenuItem,
            this.hideToolStripMenuItem,
            this.darkPowerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(851, 27);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(500, 23);
            this.toolStripTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBox1_KeyPress);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(34, 23);
            this.toolStripMenuItem1.Text = "Go";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // sToolStripMenuItem
            // 
            this.sToolStripMenuItem.Name = "sToolStripMenuItem";
            this.sToolStripMenuItem.Size = new System.Drawing.Size(61, 23);
            this.sToolStripMenuItem.Text = "Settings";
            this.sToolStripMenuItem.Click += new System.EventHandler(this.sToolStripMenuItem_Click);
            // 
            // sitesToolStripMenuItem
            // 
            this.sitesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dotapickercomToolStripMenuItem,
            this.dotabuffcomToolStripMenuItem,
            this.truepickercomrucapToolStripMenuItem,
            this.joToolStripMenuItem,
            this.googlecomToolStripMenuItem});
            this.sitesToolStripMenuItem.Name = "sitesToolStripMenuItem";
            this.sitesToolStripMenuItem.Size = new System.Drawing.Size(43, 23);
            this.sitesToolStripMenuItem.Text = "Sites";
            // 
            // dotapickercomToolStripMenuItem
            // 
            this.dotapickercomToolStripMenuItem.Name = "dotapickercomToolStripMenuItem";
            this.dotapickercomToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.dotapickercomToolStripMenuItem.Text = "dotapicker.com";
            this.dotapickercomToolStripMenuItem.Click += new System.EventHandler(this.dotapickercomToolStripMenuItem_Click);
            // 
            // dotabuffcomToolStripMenuItem
            // 
            this.dotabuffcomToolStripMenuItem.Name = "dotabuffcomToolStripMenuItem";
            this.dotabuffcomToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.dotabuffcomToolStripMenuItem.Text = "dotabuff.com";
            this.dotabuffcomToolStripMenuItem.Click += new System.EventHandler(this.dotabuffcomToolStripMenuItem_Click);
            // 
            // truepickercomrucapToolStripMenuItem
            // 
            this.truepickercomrucapToolStripMenuItem.Name = "truepickercomrucapToolStripMenuItem";
            this.truepickercomrucapToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.truepickercomrucapToolStripMenuItem.Text = "truepicker.com";
            this.truepickercomrucapToolStripMenuItem.Click += new System.EventHandler(this.truepickercomrucapToolStripMenuItem_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(0, 30);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(851, 407);
            this.webBrowser1.TabIndex = 7;
            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel1.Controls.Add(this.webBrowser1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(851, 437);
            //
            // joToolStripMenuItem
            // 
            this.joToolStripMenuItem.Name = "joToolStripMenuItem";
            this.joToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.joToolStripMenuItem.Text = "joduska.me";
            this.joToolStripMenuItem.Click += new System.EventHandler(this.joToolStripMenuItem_Click);
            // 
            // googlecomToolStripMenuItem
            // 
            this.googlecomToolStripMenuItem.Name = "googlecomToolStripMenuItem";
            this.googlecomToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.googlecomToolStripMenuItem.Text = "Google.com";
            this.googlecomToolStripMenuItem.Click += new System.EventHandler(this.googlecomToolStripMenuItem_Click);
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.Size = new System.Drawing.Size(44, 23);
            this.hideToolStripMenuItem.Text = "Hide";
            this.hideToolStripMenuItem.Click += new System.EventHandler(this.hideToolStripMenuItem_Click);
            // 
            // darkPowerToolStripMenuItem
            // 
            this.darkPowerToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.darkPowerToolStripMenuItem.Name = "darkPowerToolStripMenuItem";
            this.darkPowerToolStripMenuItem.Size = new System.Drawing.Size(76, 23);
            this.darkPowerToolStripMenuItem.Text = "DarkPower";
            this.darkPowerToolStripMenuItem.Click += new System.EventHandler(this.darkPowerToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 437);
            this.Controls.Add(this.panel1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Web Browser";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        public void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) 
        { this.toolStripTextBox1.Text =  ((WebBrowser)sender).Url.ToString(); } 

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripMenuItem sToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sitesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dotapickercomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dotabuffcomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem truepickercomrucapToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem joToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem googlecomToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem darkPowerToolStripMenuItem;
        public System.Windows.Forms.WebBrowser webBrowser1;
        }
        
        public static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                return;

            for (int i = 1; i < _font.Length; i++)
            {
                if (_font[i] == null) continue;
                xy1[i].X = xy1[0].X;
                xy2[i].X = xy2[0].X;
                xy1[i].Y = xy2[i - 1].Y;
                xy2[i].Y = (xy2[0].Y - xy1[0].Y) + xy2[i - 1].Y;
                _font[i].DrawText(null, str[i], xy1[i].X, xy1[i].Y, (IsOnTarget[i]) ? Color.HotPink : text_color[i]);
            }

            if (_font[0] != null) _font[0].DrawText(null, str[0], xy1[0].X, xy1[0].Y, (IsOnTarget[0]) ? Color.HotPink : text_color[0]);

        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            for (int i = 0; i < _font.Length; i++)
            {
                _font[i].OnResetDevice();
            }
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            for (int i = 0; i < _font.Length; i++)
            {
                _font[i].OnLostDevice();
            }
        }

        public class ClipCursor
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);
        }

        private static readonly uint WM_LBUTTONUP = 0x0202;
        private static readonly uint WM_MOUSEMOVE = 0x0200;
        private const int WM_KEYUP = 0x0101;
        private const int WM_KEYDOWN = 0x0105;
        private static Boolean IsMenuMoved = false;

        private static string[] HeroNames = new string[10];
        private static string[] Heroes = { "NULL", "Anti-Mage", "Axe" ,"Bane","Bloodseeker","Crystal_Maiden","Drow_Ranger","Earthshaker","Juggernaut","Mirana"
        ,"Shadow_Fiend","Morphling","Phantom_Lancer","Puck","Pudge","Razor","Sand_King","Storm_Spirit","Sven","Tiny","Vengeful_Spirit","Windranger","Zeus"
        ,"Kunkka","Lina","Lich","Lion","Shadow_Shaman","Slardar","Tidehunter","Witch_Doctor","Riki","Enigma","Tinker","Sniper","Necrophos","Warlock", "Beastmaster"
        ,"Queen_of_Pain","Venomancer","Faceless_Void","Wraith_King","Death_Prophet","Phantom_Assassin","Pugna","Templar_Assassin","Viper","Luna","Dragon_Knight"
        ,"Dazzle","Clockwerk","Leshrac","Natures_Prophet","Lifestealer","Dark_Seer","Clinkz","Omniknight","Enchantress","Huskar","Night_Stalker","Broodmother"
        ,"Bounty_Hunter","Weaver","Jakiro","Batrider","Chen","Spectre","Doom","Ancient_Apparition","Ursa","Spirit_Breaker","Gyrocopter","Alchemist","Invoker"
        ,"Silencer","Outworld_Devourer","Lycan","Brewmaster","Shadow_Demon","Lone_Druid","Chaos_Knight","Meepo","Treant_Protector","Ogre_Magi","Undying","Rubick"
        ,"Disruptor","Nyx_Assassin","Naga_Siren","Keeper_of_the_Light","Io","Visage","Slark","Medusa","Troll_Warlord","Centaur_Warrunner","Magnus","Timbersaw"
        ,"Bristleback","Tusk","Skywrath_Mage","Abaddon","Elder_Titan","Legion_Commander","Ember_Spirit","Earth_Spirit","Abyssal_Underlord","Terrorblade","Phoenix"
        ,"Techies","Oracle","Winter_Wyvern","Arc Warden"};

        private static string json = "{\"heroes\": [{\"name\": \"antimage\",\"id\": 1,\"localized_name\": \"Anti-Mage\"},{\"name\": \"axe\",\"id\": 2,\"localized_name\": \"Axe\"},{\"name\": \"bane\",\"id\": 3,\"localized_name\": \"Bane\"},{\"name\": \"bloodseeker\",\"id\": 4,\"localized_name\": \"Bloodseeker\"},{\"name\": \"crystal_maiden\",\"id\": 5,\"localized_name\": \"Crystal_Maiden\"},{\"name\": \"drow_ranger\",\"id\": 6,\"localized_name\": \"Drow_Ranger\"},{\"name\": \"earthshaker\",\"id\": 7,\"localized_name\": \"Earthshaker\"},{\"name\": \"juggernaut\",\"id\": 8,\"localized_name\": \"Juggernaut\"},{\"name\": \"mirana\",\"id\": 9,\"localized_name\": \"Mirana\"},{\"name\": \"nevermore\",\"id\": 11,\"localized_name\": \"Shadow_Fiend\"},{\"name\": \"morphling\",\"id\": 10,\"localized_name\": \"Morphling\"},{\"name\": \"phantom_lancer\",\"id\": 12,\"localized_name\": \"Phantom_Lancer\"},{\"name\": \"puck\",\"id\": 13,\"localized_name\": \"Puck\"},{\"name\": \"pudge\",\"id\": 14,\"localized_name\": \"Pudge\"},{\"name\": \"razor\",\"id\": 15,\"localized_name\": \"Razor\"},{\"name\": \"sand_king\",\"id\": 16,\"localized_name\": \"Sand_King\"},{\"name\": \"storm_spirit\",\"id\": 17,\"localized_name\": \"Storm_Spirit\"},{\"name\": \"sven\",\"id\": 18,\"localized_name\": \"Sven\"},{\"name\": \"tiny\",\"id\": 19,\"localized_name\": \"Tiny\"},{\"name\": \"vengefulspirit\",\"id\": 20,\"localized_name\": \"Vengeful_Spirit\"},{\"name\": \"windrunner\",\"id\": 21,\"localized_name\": \"Windranger\"},{\"name\": \"zuus\",\"id\": 22,\"localized_name\": \"Zeus\"},{\"name\": \"kunkka\",\"id\": 23,\"localized_name\": \"Kunkka\"},{\"name\": \"lina\",\"id\": 25,\"localized_name\": \"Lina\"},{\"name\": \"lich\",\"id\": 31,\"localized_name\": \"Lich\"},{\"name\": \"lion\",\"id\": 26,\"localized_name\": \"Lion\"},{\"name\": \"shadow_shaman\",\"id\": 27,\"localized_name\": \"Shadow_Shaman\"},{\"name\": \"slardar\",\"id\": 28,\"localized_name\": \"Slardar\"},{\"name\": \"tidehunter\",\"id\": 29,\"localized_name\": \"Tidehunter\"},{\"name\": \"witch_doctor\",\"id\": 30,\"localized_name\": \"Witch_Doctor\"},{\"name\": \"riki\",\"id\": 32,\"localized_name\": \"Riki\"},{\"name\": \"enigma\",\"id\": 33,\"localized_name\": \"Enigma\"},{\"name\": \"tinker\",\"id\": 34,\"localized_name\": \"Tinker\"},{\"name\": \"sniper\",\"id\": 35,\"localized_name\": \"Sniper\"},{\"name\": \"necrolyte\",\"id\": 36,\"localized_name\": \"Necrophos\"},{\"name\": \"warlock\",\"id\": 37,\"localized_name\": \"Warlock\"},{\"name\": \"beastmaster\",\"id\": 38,\"localized_name\": \"Beastmaster\"},{\"name\": \"queenofpain\",\"id\": 39,\"localized_name\": \"Queen_of_Pain\"},{\"name\": \"venomancer\",\"id\": 40,\"localized_name\": \"Venomancer\"},{\"name\": \"faceless_void\",\"id\": 41,\"localized_name\": \"Faceless_Void\"},{\"name\": \"skeleton_king\",\"id\": 42,\"localized_name\": \"Wraith_King\"},{\"name\": \"death_prophet\",\"id\": 43,\"localized_name\": \"Death_Prophet\"},{\"name\": \"phantom_assassin\",\"id\": 44,\"localized_name\": \"Phantom_Assassin\"},{\"name\": \"pugna\",\"id\": 45,\"localized_name\": \"Pugna\"},{\"name\": \"templar_assassin\",\"id\": 46,\"localized_name\": \"Templar_Assassin\"},{\"name\": \"viper\",\"id\": 47,\"localized_name\": \"Viper\"},{\"name\": \"luna\",\"id\": 48,\"localized_name\": \"Luna\"},{\"name\": \"dragon_knight\",\"id\": 49,\"localized_name\": \"Dragon_Knight\"},{\"name\": \"dazzle\",\"id\": 50,\"localized_name\": \"Dazzle\"},{\"name\": \"rattletrap\",\"id\": 51,\"localized_name\": \"Clockwerk\"},{\"name\": \"leshrac\",\"id\": 52,\"localized_name\": \"Leshrac\"},{\"name\": \"furion\",\"id\": 53,\"localized_name\": \"Natures_Prophet\"},{\"name\": \"life_stealer\",\"id\": 54,\"localized_name\": \"Lifestealer\"},{\"name\": \"dark_seer\",\"id\": 55,\"localized_name\": \"Dark_Seer\"},{\"name\": \"clinkz\",\"id\": 56,\"localized_name\": \"Clinkz\"},{\"name\": \"omniknight\",\"id\": 57,\"localized_name\": \"Omniknight\"},{\"name\": \"enchantress\",\"id\": 58,\"localized_name\": \"Enchantress\"},{\"name\": \"huskar\",\"id\": 59,\"localized_name\": \"Huskar\"},{\"name\": \"night_stalker\",\"id\": 60,\"localized_name\": \"Night_Stalker\"},{\"name\": \"broodmother\",\"id\": 61,\"localized_name\": \"Broodmother\"},{\"name\": \"bounty_hunter\",\"id\": 62,\"localized_name\": \"Bounty_Hunter\"},{\"name\": \"weaver\",\"id\": 63,\"localized_name\": \"Weaver\"},{\"name\": \"jakiro\",\"id\": 64,\"localized_name\": \"Jakiro\"},{\"name\": \"batrider\",\"id\": 65,\"localized_name\": \"Batrider\"},{\"name\": \"chen\",\"id\": 66,\"localized_name\": \"Chen\"},{\"name\": \"spectre\",\"id\": 67,\"localized_name\": \"Spectre\"},{\"name\": \"doom_bringer\",\"id\": 69,\"localized_name\": \"Doom\"},{\"name\": \"ancient_apparition\",\"id\": 68,\"localized_name\": \"Ancient_Apparition\"},{\"name\": \"ursa\",\"id\": 70,\"localized_name\": \"Ursa\"},{\"name\": \"spirit_breaker\",\"id\": 71,\"localized_name\": \"Spirit_Breaker\"},{\"name\": \"gyrocopter\",\"id\": 72,\"localized_name\": \"Gyrocopter\"},{\"name\": \"alchemist\",\"id\": 73,\"localized_name\": \"Alchemist\"},{\"name\": \"invoker\",\"id\": 74,\"localized_name\": \"Invoker\"},{\"name\": \"silencer\",\"id\": 75,\"localized_name\": \"Silencer\"},{\"name\": \"obsidian_destroyer\",\"id\": 76,\"localized_name\": \"Outworld_Devourer\"},{\"name\": \"lycan\",\"id\": 77,\"localized_name\": \"Lycan\"},{\"name\": \"brewmaster\",\"id\": 78,\"localized_name\": \"Brewmaster\"},{\"name\": \"shadow_demon\",\"id\": 79,\"localized_name\": \"Shadow_Demon\"},{\"name\": \"lone_druid\",\"id\": 80,\"localized_name\": \"Lone_Druid\"},{\"name\": \"chaos_knight\",\"id\": 81,\"localized_name\": \"Chaos_Knight\"},{\"name\": \"meepo\",\"id\": 82,\"localized_name\": \"Meepo\"},{\"name\": \"treant\",\"id\": 83,\"localized_name\": \"Treant_Protector\"},{\"name\": \"ogre_magi\",\"id\": 84,\"localized_name\": \"Ogre_Magi\"},{\"name\": \"undying\",\"id\": 85,\"localized_name\": \"Undying\"},{\"name\": \"rubick\",\"id\": 86,\"localized_name\": \"Rubick\"},{\"name\": \"disruptor\",\"id\": 87,\"localized_name\": \"Disruptor\"},{\"name\": \"nyx_assassin\",\"id\": 88,\"localized_name\": \"Nyx_Assassin\"},{\"name\": \"naga_siren\",\"id\": 89,\"localized_name\": \"Naga_Siren\"},{\"name\": \"keeper_of_the_light\",\"id\": 90,\"localized_name\": \"Keeper_of_the_Light\"},{\"name\": \"wisp\",\"id\": 91,\"localized_name\": \"Io\"},{\"name\": \"visage\",\"id\": 92,\"localized_name\": \"Visage\"},{\"name\": \"slark\",\"id\": 93,\"localized_name\": \"Slark\"},{\"name\": \"medusa\",\"id\": 94,\"localized_name\": \"Medusa\"},{\"name\": \"troll_warlord\",\"id\": 95,\"localized_name\": \"Troll_Warlord\"},{\"name\": \"centaur\",\"id\": 96,\"localized_name\": \"Centaur_Warrunner\"},{\"name\": \"magnataur\",\"id\": 97,\"localized_name\": \"Magnus\"},{\"name\": \"shredder\",\"id\": 98,\"localized_name\": \"Timbersaw\"},{\"name\": \"bristleback\",\"id\": 99,\"localized_name\": \"Bristleback\"},{\"name\": \"tusk\",\"id\": 100,\"localized_name\": \"Tusk\"},{\"name\": \"skywrath_mage\",\"id\": 101,\"localized_name\": \"Skywrath_Mage\"},{\"name\": \"abaddon\",\"id\": 102,\"localized_name\": \"Abaddon\"},{\"name\": \"elder_titan\",\"id\": 103,\"localized_name\": \"Elder_Titan\"},{\"name\": \"legion_commander\",\"id\": 104,\"localized_name\": \"Legion_Commander\"},{\"name\": \"ember_spirit\",\"id\": 106,\"localized_name\": \"Ember_Spirit\"},{\"name\": \"earth_spirit\",\"id\": 107,\"localized_name\": \"Earth_Spirit\"},{\"name\": \"abyssal_underlord\",\"id\": 108,\"localized_name\": \"Abyssal_Underlord\"},{\"name\": \"terrorblade\",\"id\": 109,\"localized_name\": \"Terrorblade\"},{\"name\": \"phoenix\",\"id\": 110,\"localized_name\": \"Phoenix\"},{\"name\": \"techies\",\"id\": 105,\"localized_name\": \"Techies\"},{\"name\": \"oracle\",\"id\": 111,\"localized_name\": \"Oracle\"},{\"name\": \"winter_wyvern\",\"id\": 112,\"localized_name\": \"Winter_Wyvern\"},{\"name\": \"npc_dota_hero_arc_warden\",\"id\": 113,\"localized_name\": \"Arc_Warden\"}]}";

        private class HeroJ
        {
            public string name { get; set; }
            public int id { get; set; }
            public string localized_name { get; set; }
        }
        
        private class RootObject
        {
            public List<HeroJ> heroes { get; set; }
            public int status { get; set; }
            public int count { get; set; }
        }

        public static void CheckPlayerHero()
        {
            Player me = ObjectManager.LocalPlayer;
            Bool needrefresh = false;
            string url = "http://dotapicker.com/herocounter/#/";
            if (me == null) return;
            foreach (var player in ObjectManager.GetEntities<Player>().ToList())
            {
                if (player == null || player.SelectedHeroID <= 0)
                {
                    HeroNames[player.ID] = "NULL";
                    continue;
                }
                RootObject tmp = JsonConvert.DeserializeObject<RootObject>(json);
                string localized_name = tmp.heroes.Where(h => h.id == player.SelectedHeroID).Select(h => h.localized_name).FirstOrDefault();
                string str_hero = ((player.Team != me.Team) ? ("E_") : ("T_")) + localized_name;
                if (HeroNames[player.ID] != str_hero) needrefresh = true;
                HeroNames[player.ID] = str_hero;
                url += HeroNames[player.ID] + "/";
            }
            if (needrefresh) { frm.webBrowser1.Navigate(url); frm.webBrowser1.Refresh(); }
        }

        public static void Game_OnGameWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen) return;

            Point CursorPosition;
            ClipCursor.GetCursorPos(out CursorPosition);

            if (args.Msg == WM_LBUTTONUP)
            {
                for (int i = 0; i < _font.Length; i++)
                {
                    if ((CursorPosition.X >= xy1[i].X && CursorPosition.Y >= xy1[i].Y) && (CursorPosition.X <= xy2[i].X && CursorPosition.Y <= xy2[i].Y))
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    frm.Show();
                                    if (frm.Opacity > 0)
                                        CheckPlayerHero();
                                    else { frm.Opacity = oldOpacity; str[0] = "REFRESH Picks"; }
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unknow item");
                                    break;
                                }
                        }
                    }
                }
            }
            else if (args.Msg == WM_MOUSEMOVE)
            {
                for (int i = 0; i < _font.Length; i++)
                {
                    if (CursorPosition.X >= xy1[i].X && CursorPosition.Y >= xy1[i].Y && CursorPosition.X <= xy2[i].X && CursorPosition.Y <= xy2[i].Y)
                    {
                        IsOnTarget[i] = true;

                    }
                    else IsOnTarget[i] = false;
                }
                if (IsMenuMoved)
                {
                    if ((Control.MouseButtons & MouseButtons.Right) != 0)
                    {
                        xy1[0].X = CursorPosition.X;
                        xy2[0].X = CursorPosition.X + rect_w;
                        xy1[0].Y = CursorPosition.Y;
                        xy2[0].Y = CursorPosition.Y + rect_h;
                    }
                    else IsMenuMoved = false;
                }
                if (!IsMenuMoved && (Control.MouseButtons & MouseButtons.Right) != 0)
                {
                    if ((CursorPosition.X >= xy1[0].X && CursorPosition.Y >= xy1[0].Y) && ((CursorPosition.X <= xy2[0].X - 50) && CursorPosition.Y <= xy2[0].Y))
                    {
                        IsMenuMoved = true;
                    }
                }
            }

        }
    }
}
