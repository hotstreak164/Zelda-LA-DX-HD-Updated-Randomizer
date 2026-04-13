using System;
using System.IO;
using System.Threading;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ProjectZ.Core.InGame.Things
{
    public class MusicPlayer
    {
        private DynamicSoundEffectInstance _instance;
        private CancellationTokenSource _cts;
        private Thread _streamThread;

        private int _currentTrack = -1;
        private string _currentPath;
        private float _volume = 1f;
        private float _volumeMultiplier = 1f;
        private readonly object _lock = new object();

        public bool IsPlaying => _instance?.State == SoundState.Playing;
        public int CurrentTrack => _currentTrack;

        public void Play(string path, int trackId)
        {
            if (_currentPath == path && IsPlaying) return;

            Stop();

            _currentPath = path;
            _currentTrack = trackId;
            _cts = new CancellationTokenSource();
            _streamThread = new Thread(() => StreamLoop(path, _cts.Token))
            {
                IsBackground = true,
                Name = "MusicStream"
            };
            _streamThread.Start();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _streamThread?.Join(500);
            _streamThread = null;

            lock (_lock)
            {
                _instance?.Stop();
                _instance?.Dispose();
                _instance = null;
            }
            _currentPath = null;
            _currentTrack = -1;
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
            ApplyVolume();
        }

        public void SetVolumeMultiplier(float mult)
        {
            _volumeMultiplier = mult;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            lock (_lock)
            {
                if (_instance != null)
                    _instance.Volume = MathHelper.Clamp(_volume * _volumeMultiplier, 0f, 1f);
            }
        }

        private void StreamLoop(string path, CancellationToken ct)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".ogg")
                StreamLoopOgg(path, ct);
        }

        private static double GetLoopPoint(string audioPath)
        {
            var loopFile = Path.ChangeExtension(audioPath, ".loop");
            var loopText = File.ReadAllText(loopFile).Trim();

            if (File.Exists(loopFile) && double.TryParse(loopText, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                return seconds;
            return 0.0;
        }

        private void StreamLoopOgg(string path, CancellationToken ct)
        {
            try
            {
                var loopStartSeconds = GetLoopPoint(path);

                using var fs = File.OpenRead(path);
                using var vorbis = new NVorbis.VorbisReader(fs, closeOnDispose: false);

                int sampleRate = vorbis.SampleRate;
                int channels = vorbis.Channels;
                var audioChannels = channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo;
                long loopStartSample = (long)(loopStartSeconds * sampleRate);

                lock (_lock)
                {
                    _instance?.Dispose();
                    _instance = new DynamicSoundEffectInstance(sampleRate, audioChannels);
                    _instance.Volume = MathHelper.Clamp(_volume * _volumeMultiplier, 0f, 1f);
                    _instance.Play();
                }

                const int samplesPerChunk = 4096;
                var floatBuf = new float[samplesPerChunk * channels];
                var byteBuf = new byte[floatBuf.Length * 2];

                while (!ct.IsCancellationRequested)
                {
                    while (!ct.IsCancellationRequested && _instance.PendingBufferCount < 3)
                    {
                        int read = vorbis.ReadSamples(floatBuf, 0, floatBuf.Length);
                        if (read == 0)
                        {
                            vorbis.SeekTo(loopStartSample);
                            break;
                        }
                        for (int i = 0; i < read; i++)
                        {
                            short s = (short)MathHelper.Clamp(floatBuf[i] * 32767f, short.MinValue, short.MaxValue);
                            byteBuf[i * 2]     = (byte)(s & 0xFF);
                            byteBuf[i * 2 + 1] = (byte)(s >> 8);
                        }
                        lock (_lock)
                            _instance?.SubmitBuffer(byteBuf, 0, read * 2);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MusicPlayer OGG stream error: {ex.Message}");
            }
        }
    }
}