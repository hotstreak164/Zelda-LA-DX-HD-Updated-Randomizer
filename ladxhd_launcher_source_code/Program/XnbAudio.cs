using System;
using System.IO;
using LADXHD_Launcher;
using NAudio.Wave;

public static class XnbAudio
{
    public static string SoundSave;
    public static string SoundXSave;
    public static string SoundOpen;
    public static string SoundClose;
    public static string SoundClick;
    public static string SoundSelect;

    public static bool Enabled { get; set; } = true;
    public static bool SuppressSound { get; set; } = false;

    public static WaveStream LoadFromXnb(string xnbPath)
    {
        byte[] data = File.ReadAllBytes(xnbPath);
        using var reader = new BinaryReader(new MemoryStream(data));

        // Skip XNB header (3 magic + platform + version + flags + filesize)
        reader.BaseStream.Seek(10, SeekOrigin.Begin);

        // Skip type reader count (7-bit encoded)
        Read7BitInt(reader);

        // Skip type reader name (length-prefixed string)
        int nameLen = reader.ReadByte();
        reader.BaseStream.Seek(nameLen, SeekOrigin.Current);

        // Skip type reader version
        reader.ReadInt32();

        // Skip shared resource count
        Read7BitInt(reader);

        // Skip type index
        Read7BitInt(reader);

        // Read SoundEffect format
        int formatSize        = reader.ReadInt32();
        short formatTag       = reader.ReadInt16();
        short channels        = reader.ReadInt16();
        int   sampleRate      = reader.ReadInt32();
        int   avgBytesPerSec  = reader.ReadInt32();
        short blockAlign      = reader.ReadInt16();
        short bitsPerSample   = reader.ReadInt16();
        if (formatSize > 16)
            reader.BaseStream.Seek(formatSize - 16, SeekOrigin.Current);

        // Read PCM data
        int dataSize  = reader.ReadInt32();
        byte[] pcmData = reader.ReadBytes(dataSize);

        // Wrap in WAV
        var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
        var ms = new MemoryStream();
        using (var writer = new WaveFileWriter(ms, waveFormat))
        {
            writer.Write(pcmData, 0, pcmData.Length);
            writer.Flush();
        }
        // Create a new stream from the buffer since WaveFileWriter closes the original
        var ms2 = new MemoryStream(ms.ToArray());
        return new WaveFileReader(ms2);
    }

    private static int Read7BitInt(BinaryReader reader)
    {
        int result = 0, shift = 0;
        byte b;
        do
        {
            b = reader.ReadByte();
            result |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return result;
    }

    public static void PlayXnbSound(string xnbPath)
    {
        if (!Enabled || SuppressSound || !File.Exists(xnbPath)) return;
        try
        {
            var stream = LoadFromXnb(xnbPath);
            var output = new WaveOutEvent();
            output.Init(stream);
            output.PlaybackStopped += (s, e) =>
            {
                output.Dispose();
                stream.Dispose();
            };
            output.Volume = 0.5f;
            output.Play();
        }
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine(ex.Message);
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
        }
    }

    public static void Initialize()
    {
        SoundSave   = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-01-01.xnb");
        SoundXSave  = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-35-23.xnb");
        SoundOpen   = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-17-11.xnb");
        SoundClose  = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-18-12.xnb");
        SoundClick  = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-10-0A.xnb");
        SoundSelect = Path.Combine(Config.BaseFolder, "Content", "SoundEffects", "D360-19-13.xnb");
    }
}