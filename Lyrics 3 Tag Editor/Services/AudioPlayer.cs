using NAudio.Wave;
using System;
using NAudio.Wave.SampleProviders;
using Lyrics_3_Tag_Editor.Plugin;
using System.Windows.Threading;

namespace Lyrics_3_Tag_Editor.Services
{
    public class AudioPlayer
    {
        #region Callback

        public Callback.onMediaLoaded onMediaLoaded { get; set; }
        public Callback.onPositionChanged onPositionChanged { get; set; }
        public Callback.onPlay onPlay { get; set; }
        public Callback.onPause onPause { get; set; }
        public Callback.onStop onStop { get; set; }

        public class Callback
        {
            public delegate void onMediaLoaded(FileModel file);
            public delegate void onPlay();
            public delegate void onPause();
            public delegate void onStop();
            public delegate void onPositionChanged(AudioFileReader fileReader, TimeSpan position);
        }

        #endregion

        #region Variables

        private DispatcherTimer timer = null;
        private IWavePlayer waveOut = null;
        private AudioFileReader audioFileReader = null;
        private Action<float> setVolumeDelegate = null;
        private WasapiOutPlugin plugin = null;
        private ISampleProvider sampleProvider = null;
        private SampleChannel sampleChannel = null;

        #endregion

        public AudioPlayer()
        {
            this.plugin = new WasapiOutPlugin();
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1);

            this.timer.Tick += (sender, e) => {
                if (waveOut != null && audioFileReader != null)
                {
                    TimeSpan currentTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.CurrentTime;
                    onPositionChanged?.Invoke(audioFileReader, currentTime);
                }
            };
        }

        public int Load(FileModel file)
        {
            if (plugin == null)
            {
                return 1;
            }

            try
            {
                waveOut = plugin.Create(100);
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    if (audioFileReader != null)
                    {
                        audioFileReader.Position = 0;
                    }
                };
            }
            catch
            {
                return 2;
            }

            try
            {
                sampleProvider = CreateInputStream(file);
            }
            catch
            {
                return 3;
            }

            try
            {
                waveOut.Init(sampleProvider);
            }
            catch
            {
                return 4;
            }

            setVolumeDelegate(Properties.Settings.Default.Volume);
            onMediaLoaded?.Invoke(file);

            return 0;
        }

        public void Play()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Paused || waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    waveOut.Play();
                    onPlay?.Invoke();
                    timer.Start();
                }
            }
        }

        public void Pause()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    timer.Stop();
                    onPause?.Invoke();
                }
            }
        }

        public void Stop()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Stop();
                    timer.Stop();
                    onStop?.Invoke();
                }
            }
        }

        public void Clear()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Stop();
                    timer.Stop();
                }
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                setVolumeDelegate = null;
                audioFileReader = null;
            }

            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }

        private ISampleProvider CreateInputStream(FileModel file)
        {
            this.audioFileReader = new AudioFileReader(file.Temporary.FullName);
            this.sampleChannel = new SampleChannel(audioFileReader, true);
            this.setVolumeDelegate = (vol) => sampleChannel.Volume = vol;
            return new MeteringSampleProvider(sampleChannel);
        }

        public void setPosition(double value)
        {
            if (waveOut != null)
            {
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds * value / 1000.0);
            }
        }

        public void setVolume(float value)
        {
            setVolumeDelegate?.Invoke(value);
        }

        public TimeSpan getPosition()
        {
            if (audioFileReader != null)
            {
                return audioFileReader.CurrentTime;
            }
            else
            {
                return new TimeSpan();
            }
        }

        public PlaybackState getPlaybackState()
        {
            if (waveOut != null)
            {
                return waveOut.PlaybackState;
            }

            return PlaybackState.Stopped;
        }
    }
}
