using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.Things;

namespace GBSPlayer
{
    public class GbsPlayer
    {
        public GameBoyCPU Cpu;
        public Cartridge Cartridge;
        public GeneralMemory Memory;
        public Sound SoundGenerator;

        public byte CurrentTrack;
        private volatile int _pendingTrack = -1;

        public bool GbsLoaded;

        private float _volume = 1;
        private float _volumeMultiplier = 1.0f;

        private readonly object _updateLock = new object();
        private volatile bool _exitThread;

        private Thread _updateThread;

        public GbsPlayer()
        {
            SoundGenerator = new Sound();
            Cartridge = new Cartridge();
            Memory = new GeneralMemory(Cartridge, SoundGenerator);
            Cpu = new GameBoyCPU(Memory, Cartridge, SoundGenerator);
        }

        public void OnExit()
        {
            _exitThread = true;

            try
            {
                if (_updateThread != null && _updateThread.IsAlive)
                    _updateThread.Join(1000);
            }
            catch { }
        }

        public void LoadFile(string path)
        {
            path = GameFS.ToAssetPath(path);

            Cartridge.ROM = GameFS.ReadAllBytes(path);

            Cartridge.Init();
            Cpu.Init();

            GbsLoaded = true;
        }

        public void ChangeTrack(int offset)
        {
            var newTrack = CurrentTrack + offset;

            while (newTrack < 0)
                newTrack += Cartridge.TrackCount;
            newTrack %= Cartridge.TrackCount;

            StartTrack((byte)newTrack);
        }

        public void RequestTrack(byte trackNr)
        {
            _pendingTrack = trackNr;
        }

        public void StartTrack(byte trackNr)
        {
            // directly init the new song if update is not called at this time
            lock (_updateLock)
            {
                if (GbsLoaded && Cpu.IsRunning && trackNr == CurrentTrack)
                    return;

                CurrentTrack = trackNr;

                SoundGenerator.Stop();
                GbsInit(trackNr);
                SoundGenerator.SetStopTime(0);
            }
        }

        private void StartTrack_NoLock(byte trackNr)
        {
            CurrentTrack = trackNr;

            SoundGenerator.Stop();
            GbsInit(trackNr);
            SoundGenerator.SetStopTime(0);
        }

        private void GbsInit(byte trackNumber)
        {
            Cartridge.Init();
            Cpu.SkipBootROM();
            Cpu.Init();
            Cpu.SetPlaybackSpeed(1);

            // tack number
            Cpu.reg_A = trackNumber;

            Cpu.reg_PC = Cartridge.InitAddress;
            Cpu.reg_SP = Cartridge.StackPointer;

            // push the idleAddress on the stack
            Memory[--Cpu.reg_SP] = (byte)(Cpu.IdleAddress >> 0x8);
            Memory[--Cpu.reg_SP] = (byte)(Cpu.IdleAddress & 0xFF);

            Console.WriteLine("finished gbs init");
        }

        public void Pump()
        {
            SoundGenerator?.Pump();
        }

        public void Play()
        {
            Cpu.IsRunning = true;
        }

        public void Pause()
        {
            SoundGenerator.Pause();
            Cpu.IsRunning = false;
        }

        public void Resume()
        {
            SoundGenerator.Resume();
            Cpu.IsRunning = true;
        }

        public void Stop()
        {
            // stop music playback
            SoundGenerator.Stop();
            Cpu.IsRunning = false;
        }

        public float GetVolume()
        {
            return _volume;
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
            SoundGenerator.SetVolume(_volume * _volumeMultiplier);
        }

        public float GetVolumeMultiplier()
        {
            return _volumeMultiplier;
        }

        public void SetVolumeMultiplier(float multiplier)
        {
            _volumeMultiplier = multiplier;
            SoundGenerator.SetVolume(_volume * _volumeMultiplier);
        }

        public void Update(float deltaTime)
        {
            if (!Cpu.IsRunning) return;
            lock (_updateLock)
                Cpu.Update();
        }

        public void StartThread()
        {
            // Don’t start twice
            if (_updateThread != null && _updateThread.IsAlive)
                return;

            _exitThread = false;

            _updateThread = new Thread(UpdateThread)
            {
                IsBackground = true,
                Name = "GBSPlayer.UpdateThread"
            };

            _updateThread.Start();
        }

        public void UpdateThread()
        {
            while (!_exitThread)
            {
                lock (_updateLock)
                {
                    var req = _pendingTrack;
                    if (req >= 0)
                    {
                        _pendingTrack = -1;

                        var song = (byte)req;
                        if (CurrentTrack != song)
                            StartTrack_NoLock(song);
                    }
                    Cpu.Update();
                }
                Thread.Sleep(5);
            }
        }
    }
}
