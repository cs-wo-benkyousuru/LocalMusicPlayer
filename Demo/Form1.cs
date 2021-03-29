using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using PlayerCore;
using HighTimer;
using System.IO;

namespace Demo
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            audioPlayer.Init(this.Handle);
            openFileDialog1.InitialDirectory = Application.StartupPath;
            accurateTimer.Interval = 1;
            accurateTimer.Elapsed += AccurateTimer_Elapsed;
        }

        private void AccurateTimer_Elapsed(object sender, TimerEventArgs e)
        {
            var NextTimeLrcPair = lrcDict[LrcPos];
            long NextLrcTime = NextTimeLrcPair.Key;
            string NextLrc = NextTimeLrcPair.Value;
            if(PlayedTime == NextLrcTime)
            {
                metroLabel4.Invoke(new Action(() => { metroLabel4.Text = NextLrc; }));
                ++LrcPos;
            }
            if (PlayedTime % 1000 == 0)
            {
                metroTrackBar1.Invoke(new Action(() => { ++metroTrackBar1.Value; }));
                metroLabel1.Invoke(new Action(() => { metroLabel1.Text = TimeSpan.FromSeconds(PlayedTime /1000.0).ToString(); }));
            }
            ++PlayedTime;
        }

        HighAccurateTimer accurateTimer = new HighAccurateTimer();
        AudioPlayer audioPlayer = new AudioPlayer();
        private long PlayedTime = 0;
        Dictionary<int, KeyValuePair<long, string>> lrcDict = new Dictionary<int, KeyValuePair<long, string>>();
        int LrcPos = 0;

        private void metroButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            //只有选择文件成功才能播放
            if(audioPlayer.SetPlayItem(openFileDialog1.FileName))
            {
                metroTextBox1.Text = Path.GetFileName(openFileDialog1.FileName);
                lrcDict = LrcPhaser.GetLrc(Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + ".lrc");
                var l = audioPlayer.Length();
                metroTrackBar1.Maximum = (int)(l);
                metroLabel2.Text = TimeSpan.FromSeconds(l).ToString().Substring(0, 8);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void metroTrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            for(int i = 0; i < lrcDict.Count; ++i)
            {
                if(lrcDict[i].Key > metroTrackBar1.Value * 1000)
                {
                    metroLabel4.Text = lrcDict[i - 1].Value;
                    LrcPos = i;
                    break;
                }
            }
            audioPlayer.SetPosition(metroTrackBar1.Value * 1000);
            PlayedTime = metroTrackBar1.Value * 1000;
            metroLabel1.Text = TimeSpan.FromSeconds(metroTrackBar1.Value).ToString();
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            button3.BackgroundImage = Properties.Resources.play;
            metroTrackBar1.Value = 0;
            accurateTimer.Enabled = false;
            PlayedTime = 0;
            metroLabel1.Text = "00:00:00";
            metroLabel4.Text = " ";
            LrcPos = 0;
            audioPlayer.Stop();
        }
        private void metroButton3_Click(object sender, EventArgs e)
        {
            if(audioPlayer.playStatus == AudioPlayer.PlayStatus.Pause || audioPlayer.playStatus == AudioPlayer.PlayStatus.Stop)
            {
                button3.BackgroundImage = Properties.Resources.pause;
                audioPlayer.Play();
                accurateTimer.Enabled = true;
            }
            else
            {
                button3.BackgroundImage = Properties.Resources.play;
                audioPlayer.Pause();
                accurateTimer.Enabled = false;
            }
        }
    }
}
