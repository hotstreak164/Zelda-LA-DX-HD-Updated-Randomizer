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

        private struct LoopPoints
        {
            public long StartSample;
            public long EndSample;
        }

        private static LoopPoints GetLoopPoints(NVorbis.VorbisReader vorbis, string audioPath)
        {
             // Use -1 as default to play to end.
            long start = 0;
            long end = -1;

            // Try OGG metadata first
            var tags = vorbis.Tags;
            if (tags != null)
            {
                if (long.TryParse(tags.GetTagSingle("LOOPSTART"), out long startSample))
                    start = startSample;

                if (long.TryParse(tags.GetTagSingle("LOOPLENGTH"), out long length))
                    end = start + length;
                else if (long.TryParse(tags.GetTagSingle("LOOPEND"), out long endSample))
                    end = endSample;

                if (start > 0)
                    return new LoopPoints { StartSample = start, EndSample = end };
            }

            // Fall back to .loop file
            var loopFile = Path.ChangeExtension(audioPath, ".loop");
            if (File.Exists(loopFile))
            {
                var lines = File.ReadAllLines(loopFile);
                if (lines.Length >= 1 && double.TryParse(lines[0].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double startSeconds))
                    start = (long)(startSeconds * vorbis.SampleRate);

                if (lines.Length >= 2 && double.TryParse(lines[1].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double endSeconds))
                    end = (long)(endSeconds * vorbis.SampleRate);
            }

            return new LoopPoints { StartSample = start, EndSample = end };
        }

        private void StreamLoopOgg(string path, CancellationToken ct)
        {
            try
            {
                using var fs = File.OpenRead(path);
                using var vorbis = new NVorbis.VorbisReader(fs, closeOnDispose: false);

                int sampleRate = vorbis.SampleRate;
                int channels = vorbis.Channels;
                var audioChannels = channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo;

                var loop = GetLoopPoints(vorbis, path);

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
                        // If we have a loop end point, check if we're about to pass it
                        int samplesToRead = floatBuf.Length;
                        if (loop.EndSample >= 0)
                        {
                            long samplesRemaining = loop.EndSample - vorbis.SamplePosition;
                            if (samplesRemaining <= 0)
                            {
                                vorbis.SeekTo(loop.StartSample);
                                break;
                            }
                            samplesToRead = (int)Math.Min(samplesToRead, samplesRemaining * channels);
                        }

                        int read = vorbis.ReadSamples(floatBuf, 0, samplesToRead);
                        if (read == 0)
                        {
                            vorbis.SeekTo(loop.StartSample);
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