using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;

namespace WF_DB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String result = "";

            var webRequest = WebRequest.Create(@"http://www.dotabuff.com/players/6214367");
            ((HttpWebRequest)webRequest).UserAgent = ".NET Framework Example Client";
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new System.IO.StreamReader(content))
            {
                var strContent = reader.ReadToEnd();
                result = strContent;
            }
            int startpos = result.IndexOf("<div class=\"r-table r-only-mobile-5 heroes-overview\">");
            if (startpos > -1)
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
                    String HeroName = result.Substring(NextPos+13, result.IndexOf("\"",NextPos) - NextPos-13);
                    textBox1.Text = textBox1.Text + HeroName;
                    //NextPos = ;
                    int Matchespos = result.IndexOf("Matches Played</div><div class=\"r-body\">");
                    String Matches = result.Substring(Matchespos + 40, result.IndexOf("<", Matchespos + 40) - Matchespos-40);
                    textBox1.Text = textBox1.Text + ((HeroName.Length > 7) ? ("\t") : ("\t\t")) + Matches;

                    int WinRatepos = result.IndexOf("Win Rate</div><div class=\"r-body\">");
                    String WinRate = result.Substring(WinRatepos + 34, result.IndexOf("<", WinRatepos + 34) - WinRatepos - 34);
                    textBox1.Text = textBox1.Text + "\t\t" + WinRate;

                    int KDApos = result.IndexOf("KDA Ratio</div><div class=\"r-body\">");
                    String KDA = result.Substring(KDApos + 35, result.IndexOf("<", KDApos + 35) - KDApos - 35);
                    textBox1.Text = textBox1.Text + "\t\t" + KDA + "\r\n";

                    NextPos = result.IndexOf("matches?hero=", NextPos+630);
                    
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            webBrowser1.Visible = false;
            textBox1.Visible = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            webBrowser1.Visible = true;
            textBox1.Visible = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            webBrowser1.Visible = false;
            textBox1.Visible = false;
        }
    }
}
