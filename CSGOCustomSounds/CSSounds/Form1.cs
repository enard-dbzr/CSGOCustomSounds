using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSSounds
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void Checker()
        {
            double mainVolume = 1, curVolume = 1;
            string providerSteamID, playerSteamID,
                phase, prevPhase = "",
                team = "";
            int kills, prevKills = 9999,
                deaths, prevDeaths = 9999,
                mvps, prevMvps = 9999;
            bool alredyWon = false;

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            WMPLib.WindowsMediaPlayer WMP = new WMPLib.WindowsMediaPlayer();
            WMP.settings.volume = 9;
            Log("Ожидание подключений...");

            while (true)
            {
                Invoke(new Action(() => mainVolume = trackBar_volume.Value / 100.0));

                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string responseString = "Good";

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                string str;
                using (var reader = new StreamReader(request.InputStream))
                    str = reader.ReadToEnd();
                output.Write(buffer, 0, buffer.Length);
                JObject json = JObject.Parse(str);

                providerSteamID = json["provider"]["steamid"].ToString();
                playerSteamID = json["player"]["steamid"].ToString();
                team = json["player"]["team"].ToString();
                phase = json["round"]["phase"].ToString();
                kills = Convert.ToInt32(json["player"]["match_stats"]["kills"].ToString());
                deaths = Convert.ToInt32(json["player"]["match_stats"]["deaths"].ToString());
                mvps = Convert.ToInt32(json["player"]["match_stats"]["mvps"].ToString());


                if(phase == "freezetime" && prevPhase != phase)
                {
                    WMP.URL = "startround.mp3";
                    Invoke(new Action(() => curVolume = trackBar_feezetime.Value));
                    WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                    WMP.controls.play();
                }
                if (kills > prevKills && providerSteamID == playerSteamID)
                {
                    WMP.URL = "kill.mp3";
                    Invoke(new Action(() => curVolume = trackBar_kill.Value));
                    WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                    WMP.controls.play();
                }
                if (deaths > prevDeaths && providerSteamID == playerSteamID)
                {
                    WMP.URL = "die.mp3";
                    Invoke(new Action(() => curVolume = trackBar_death.Value));
                    WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                    WMP.controls.play();
                }
                if(phase == "over" && !alredyWon)
                {
                    alredyWon = true;
                    if (json["round"]["win_team"].ToString() == team)
                    {
                        WMP.URL = "wonround.mp3";
                        Invoke(new Action(() => curVolume = trackBar_mvp.Value));
                        WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                        WMP.controls.play();
                    }
                    else
                    {
                        WMP.URL = "lostround.mp3";
                        Invoke(new Action(() => curVolume = trackBar_mvp.Value));
                        WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                        WMP.controls.play();
                    }  
                }
                else if(phase != "over")
                {
                    alredyWon = false;
                }
                if (mvps > prevMvps && providerSteamID == playerSteamID)
                {
                    WMP.URL = "mvp.mp3";
                    Invoke(new Action(() => curVolume = trackBar_mvp.Value));
                    WMP.settings.volume = Convert.ToInt32(curVolume * mainVolume);
                    WMP.controls.play();
                }

                Invoke(new Action(() => richTextBox1.AppendText(str + ",\n")));
                Invoke(new Action(() => richTextBox1.ScrollToCaret()));

                prevPhase = phase;
                if(providerSteamID == playerSteamID)
                {
                    prevKills = kills;
                    prevDeaths = deaths;
                    prevMvps = mvps;
                }
                output.Close();
            }

        }  
        void Log(string str)
        {
            toolStripStatusLabel1.Text = str;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread th = new Thread(Checker);
            th.Start();
        }
    }
}
