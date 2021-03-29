using System;
using Un4seen;
using Un4seen.Bass;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace PlayerCore
{
    class LrcPhaser
    {
        public static Dictionary<int,KeyValuePair<long, string>> GetLrc(string FileName)
        {
            Dictionary<int, KeyValuePair<long, string>> dict = new Dictionary<int, KeyValuePair<long, string>>();
            Regex r = new Regex(@"\[[0-9]{2,2}:[0-9]{2,2}\.[0-9]{2,2}\]");
            try
            {
                using (StreamReader file = new StreamReader(FileName,encoding : Encoding.Default))
                {
                    int Counter = 0;
                    while (!file.EndOfStream)
                    {
                        string tmp = file.ReadLine();
                        if (r.IsMatch(tmp))
                        {
                            long time = long.Parse(tmp.Substring(1, 2)) * 60 * 1000
                                + long.Parse(tmp.Substring(4, 2)) * 1000
                                + long.Parse(tmp.Substring(7, 2)) * 10;
                            dict.Add(Counter, new KeyValuePair<long, string>(time, tmp.Substring(10, tmp.Length - 10)));
                            ++Counter;
                        }
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                //返回空的dict
                return dict;
            }
            return dict;
        }
    }
    class AudioPlayer
    {
        private bool Initilized
        {
            get;
            set;
        }
        public string PlayItem { get; private set; }
        private int PlayStream;
        public enum PlayStatus
        {
            Playing = 0,
            Stop = 1,
            Pause = 2
        }
        public PlayStatus playStatus { get; set; } = PlayStatus.Stop;
        public void Init(IntPtr Handle)
        {
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_SPEAKERS, Handle, Guid.Empty))
            {
                throw new Exception("未能初始化播放器。");
            }
        }
        public double Length()
        {
            return Bass.BASS_ChannelBytes2Seconds(PlayStream, Bass.BASS_ChannelGetLength(PlayStream));
        }
        public void Play()
        {
            if (playStatus == PlayStatus.Pause)
                Bass.BASS_ChannelPlay(PlayStream, false);
            else if (playStatus == PlayStatus.Stop)
                Bass.BASS_ChannelPlay(PlayStream, true);
            playStatus = PlayStatus.Playing;
        }
        public void Pause()
        {
            Bass.BASS_ChannelPause(PlayStream);
            this.playStatus = PlayStatus.Pause;
        }
        public void Stop()
        {
            Bass.BASS_ChannelStop(PlayStream);
            this.playStatus = PlayStatus.Stop;
        }
        public void SetPosition(long Milliseconds)
        {
            long postion = Bass.BASS_ChannelSeconds2Bytes(PlayStream, Milliseconds/1000.0);
            Bass.BASS_ChannelSetPosition(PlayStream, postion);
        }
        public bool SetPlayItem(string FilePath)
        {
            //只有在没有播放的时候才能设置路径
            if (playStatus == PlayStatus.Playing) return false;
            this.PlayItem = FilePath;
            PlayStream = Bass.BASS_StreamCreateFile(FilePath, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            this.playStatus = PlayStatus.Stop;
            return true;
        }
    }
}