using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ProjectZ.InGame.GameObjects;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.Audio
{
    public class AudioManager
    {
        private class PlayingSoundEffect
        {
            public bool LowerMusicVolume;
            public float Volume;
            public double EndTime;
            public SoundEffectInstance Instance;
        }
        private GameManager GM => Game1.GameManager;

        // Sound effects that are currently playing.
        private Dictionary<string, PlayingSoundEffect> CurrentSoundEffects = new Dictionary<string, PlayingSoundEffect>();

        // 0: Map Music, 1: PowerUp Music, 2: Marin Singing
        private const int MusicChannels = 3;
        private int[] _musicArray = new int[MusicChannels];

        // Counters used to stop music.
        private float[] _musicCounter = new float[MusicChannels];

        // Muting the sound requires overwriting effect volume so store user setting.
        private int _curEffectVolume = GameSettings.EffectVolume;
        private bool _lastStateSet;
        private bool _muteInactive;
        private bool _mp3WasPlaying;

        // The MP3 player allows playing custom music.
        internal MusicPlayer _musicPlayer = new MusicPlayer();

        // Quick reference to "ObjLink" in MapManager.
        private ObjLink Link => MapManager.ObjLink;

        public void HandleInactiveWindow(bool IsActive)
        {
            // We don't need this to run every single game tick.
            if (IsActive != _lastStateSet)
            {
                if (!IsActive && GameSettings.MuteInactive)
                {
                    Game1.GbsPlayer.SetVolume(0f);
                    _musicPlayer.SetVolume(0f);
                    _muteInactive = true;
                }
                else
                {
                    var vol = GameSettings.MusicVolume / 100.0f;
                    Game1.GbsPlayer.SetVolume(vol);
                    _musicPlayer.SetVolume(vol);
                    _muteInactive = false;
                }
            }
            _lastStateSet = IsActive;
        }

        public void InitGuardianAcorn()
        {
            if (GM.PieceOfPowerIsActive)
                StopPieceOfPower();

            GM.GuardianAcornIsActive = true;
            GM.GuardianAcornDamageCount = 0;

            if (!GameSettings.MutePowerups)
                StartPowerupMusic(0);
            else
                PlaySoundEffect("D360-23-17");
        }

        public void StopGuardianAcorn()
        {
            StopPowerupMusic();
            GM.GuardianAcornIsActive = false;
        }

        public void InitPieceOfPower()
        {
            if (GM.GuardianAcornIsActive)
                StopGuardianAcorn();

            GM.PieceOfPowerIsActive = true;
            GM.PieceOfPowerDamageCount = 0;

            if (!GameSettings.MutePowerups)
                StartPowerupMusic(0);
            else
                PlaySoundEffect("D360-23-17");
        }

        public void StopPieceOfPower()
        {
            StopPowerupMusic();
            GM.PieceOfPowerIsActive = false;
        }

        public void StartPowerupMusic(int Variation)
        {
            // 0: Delayed with sound effect
            // 1: Music starts instantly.
            int trackId = Variation == 0 ? 38 : 72;
            SetMusic(trackId, 1);

            // @HACK: When music is restarted for any reason: map/area transition, healing
            // from a great fairy, etc. we want the version without the starting sound effect.
            if (Variation == 0)
            {
                Game1.GbsPlayer.CurrentTrack = 72;
                _musicArray[1] = 72;
            }
        }

        private void StopPowerupMusic()
        {
            // When inside a village, revert the hack which stores the piece of power music in slot 1 and
            // the village music inside slot 2. This is mostly for Mabe Village where dogs can attack.
            if (_musicArray[0] == 72 && (_musicArray[1] == 3 || _musicArray[1] == 10))
            {
                _musicArray[0] = _musicArray[1];
                _musicArray[1] = -1;
                return;
            }
            // Any other time we can just set slot 2 to -1.
            SetMusic(-1, 1, true);
        }

        public bool CheckSetMusicConditions(int trackID, int priority)
        {
            // Don't restart the overworld track if the version with the intro was already started. But if it's the part
            // of the game where Marin joins the player and the beach photo is taken, we need to allow song 4 to replace 48.
            if (trackID == 4 && _musicArray[priority] == 48 && GM.SaveManager.GetString("maria_state") != "3")
                return false;

            // Make sure to not restart the music while showing the overworld in the final sequence. 
            if (priority != 2 && _musicArray[2] == 62)
                return false;

            // When entering a village (3: Mabe Village, 10: Animal Village) with (72: Piece of Power Music)
            // backup the piece of power music and force the new song onto the piece of power slot.
            if ((trackID == 3 || trackID == 10) && _musicArray[1] == 72 && priority == 0 && !MapManager.ObjLink.IsTransitioning)
            {
                _musicArray[0] = 72;
                _musicArray[1] = trackID;
            }
            // When leaving the village, restore piece of power music and write new track to it's proper slot.
            else if ((trackID != 3 && trackID != 10) && _musicArray[0] == 72 && priority == 0 && !MapManager.ObjLink.IsTransitioning)
            {
                _musicArray[0] = trackID;
                _musicArray[1] = 72;
            }
            // In any other cases, just handle music normally.
            else
            {
                _musicArray[priority] = trackID;
            }
            // Play the music.
            return true;
        }

        public void SetMusic(int trackID, int priority, bool startPlaying = true)
        {
            // See if we should play music and if there is any nuances to take care of before playing the music.
            if (CheckSetMusicConditions(trackID, priority))
            {
                PlayMusic(startPlaying);
            }
        }

        public bool IsMusicPlayerActive()
        {
            return _musicPlayer.IsPlaying;
        }

        public void SetMusicPlayerStopTime(float seconds)
        {
            _musicPlayer.SetStopTime(seconds);
        }

        public bool IsMusicPlayerStopped()
        {
            return _musicPlayer.WasStopped;
        }

        private string GetModMusicPath(int trackId)
        {
            var path = Path.Combine(Values.PathMusicMods, $"{trackId}.ogg");
            return File.Exists(path) ? path : null;
        }

        public void PlayMusic(bool startPlaying = true)
        {
            for (var i = MusicChannels - 1; i >= 0; i--)
            {
                if (_musicArray[i] >= 0)
                {
                    var songNumber = (byte)_musicArray[i];
                    var songPath = GetModMusicPath(songNumber);

                    if (!string.IsNullOrEmpty(songPath))
                    {
                        Game1.GbsPlayer.Stop();
                        if (startPlaying)
                        {
                            _musicPlayer.SetVolume(_muteInactive ? 0f : GameSettings.MusicVolume / 100.0f);
                            _musicPlayer.Play(songPath, songNumber);
                        }
                        return;
                    }
                    _musicPlayer.Stop();
                    if (Game1.GbsPlayer.CurrentTrack != songNumber)
                        Game1.GbsPlayer.StartTrack(songNumber);
                    if (startPlaying)
                        Game1.GbsPlayer.Play();
                    return;
                }
            }
            _musicPlayer.Stop();
            Game1.GbsPlayer.Stop();
        }

        public void StopMusic(bool reset = false)
        {
            if (reset)
                ResetMusic();
            _musicPlayer.Stop();
            Game1.GbsPlayer.Stop();
        }

        public void StopMusic(int time, int priority)
        {
            _musicCounter[priority] = time;
        }

        public void SetMusicStopTime(float stopTime)
        {
            Game1.GbsPlayer.SoundGenerator.SetStopTime(stopTime);
            SetMusicPlayerStopTime(stopTime);
        }

        public bool GetMusicStopTimeExpired()
        {
            if (IsMusicPlayerActive())
                return _musicPlayer.WasStopped;
            return Game1.GbsPlayer.SoundGenerator.WasStopped && Game1.GbsPlayer.SoundGenerator.FinishedPlaying();
        }

        public void PauseMusic()
        {
            _mp3WasPlaying = _musicPlayer.IsPlaying;
            if (_mp3WasPlaying)
                _musicPlayer.Pause();
            else
                Game1.GbsPlayer.Pause();
        }

        public void ResumeMusic()
        {
            if (_mp3WasPlaying)
                _musicPlayer.Resume();
            else
                Game1.GbsPlayer.Resume();
        }

        public void ResetMusic()
        {
            for (var i = 0; i < MusicChannels; i++)
            {
                _musicArray[i] = -1;
                _musicCounter[i] = 0;
            }
        }

        public float GetMusicVolumeMultiplier()
        {
            if (_musicPlayer.IsPlaying)
                return _musicPlayer.GetVolumeMultiplier();
            return Game1.GbsPlayer.GetVolumeMultiplier();
        }

        public void SetMusicVolume(float volume)
        {
            Game1.GbsPlayer.SetVolume(volume);
            _musicPlayer.SetVolume(volume);
        }

        public void SetMusicVolumeMultiplier(float mult)
        {
            Game1.GbsPlayer.SetVolumeMultiplier(mult);
            _musicPlayer.SetVolumeMultiplier(mult);
        }

        public void SetMusicPlaybackSpeed(float speed)
        {
            Game1.GbsPlayer.SetPlaybackSpeed(speed);
            _musicPlayer.SetPlaybackSpeed(speed);
        }

        public void UpdateMusic()
        {
            for (var i = 0; i < MusicChannels; i++)
            {
                if (_musicCounter[i] == 0)
                    continue;

                _musicCounter[i] -= Game1.DeltaTime;

                // finished playing the music?
                if (_musicCounter[i] <= 0)
                {
                    _musicArray[i] = -1;
                    _musicCounter[i] = 0;
                    PlayMusic();
                }
            }
        }

        public int GetCurrentMusic()
        {
            for (var i = _musicArray.Length - 1; i >= 0; i--)
                if (_musicArray[i] >= 0)
                    return _musicArray[i];
            return -1;
        }

        public void UpdateSoundEffects()
        {
            var lowerVolume = false;

            // Set the volume to 0 if window is inactive otherwise use the volume set by the player.
            _curEffectVolume = _muteInactive ? 0 : GameSettings.EffectVolume;

            // we use ToList to be able to remove entries in the foreach loop
            foreach (var soundEffect in CurrentSoundEffects.ToList())
            {
                if (CurrentSoundEffects[soundEffect.Key].LowerMusicVolume)
                    lowerVolume = true;

                // update the volume of the sound effects to match the current settings
                soundEffect.Value.Instance.Volume = CurrentSoundEffects[soundEffect.Key].Volume * _curEffectVolume / 100 * Values.SoundEffectVolumeMult;
                soundEffect.Value.Instance.IsLooped = false;

                if (soundEffect.Value.EndTime != 0 && soundEffect.Value.EndTime < Game1.TotalGameTime)
                    soundEffect.Value.Instance.Stop();

                // finished playing?
                if (soundEffect.Value.Instance.State == SoundState.Stopped)
                    CurrentSoundEffects.Remove(soundEffect.Key);
            }
            if (lowerVolume)
                SetMusicVolumeMultiplier(0.35f);
            else
                SetMusicVolumeMultiplier(1.0f);
        }

        public void PauseSoundEffects()
        {
            foreach (var soundEffect in CurrentSoundEffects)
                if (soundEffect.Value.Instance.State == SoundState.Playing)
                    soundEffect.Value.Instance.Pause();
        }

        public void ContinueSoundEffects()
        {
            foreach (var soundEffect in CurrentSoundEffects)
                if (soundEffect.Value.Instance.State == SoundState.Paused)
                    soundEffect.Value.Instance.Resume();
        }

        public void PlaySoundEffect(string name, bool restart, Vector2 position, float range = 256)
        {
            var playerDistance = Link.EntityPosition.Position - position;
            var volume = 1 - playerDistance.Length() / range;

            if (volume > 0)
                PlaySoundEffect(name, restart, volume);
        }

        public void PlaySoundEffect(string name, bool restart = true, float volume = 1, float pitch = 0, bool lowerMusicVolume = false, float playtime = 0)
        {
            CurrentSoundEffects.TryGetValue(name, out var entry);

            // if the same sound is playing it will be stopped and replaced with the new instance
            if (restart && entry!= null && entry.Instance != null)
            {
                entry.Instance.Stop();
                CurrentSoundEffects.Remove(name);
            }
            if (!restart && entry != null && entry.Instance != null)
            {
                entry.Volume = volume;
                if (playtime != 0)
                    entry.EndTime = Game1.TotalGameTime + playtime;

                entry.Instance.Volume = volume * _curEffectVolume / 100f * Values.SoundEffectVolumeMult;
                entry.Instance.Pitch = pitch;
                
                return;
            }

            entry = new PlayingSoundEffect() { Volume = volume, LowerMusicVolume = lowerMusicVolume };
            entry.Instance = Resources.SoundEffects[name].CreateInstance();

            // the volume of the sound effects is higher than the music; so scale effect volume a little down
            entry.Instance.Volume = volume * _curEffectVolume / 100f * Values.SoundEffectVolumeMult;
            entry.Instance.Pitch = pitch;

            if (playtime != 0)
            {
                entry.Instance.IsLooped = true;
                entry.EndTime = Game1.TotalGameTime + playtime;
            }

            entry.Instance.Play();

            CurrentSoundEffects.Add(name, entry);
        }

        public void StopSoundEffect(string name)
        {
            if (CurrentSoundEffects.TryGetValue(name, out var entry))
                entry.Instance.Stop();
        }

        public bool IsPlaying(string name)
        {
            if (CurrentSoundEffects.TryGetValue(name, out var entry))
                return entry.Instance.State == SoundState.Playing;

            return false;
        }
    }
}
