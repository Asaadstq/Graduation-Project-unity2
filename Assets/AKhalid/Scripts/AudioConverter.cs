using System;
using System.IO;
using UnityEngine;

public static class AudioConverter
{
    public static byte[] AudioClipToWav(AudioClip clip)
    {
        Debug.Log("Starting AudioClipToWav conversion...");
        
        if (clip == null)
        {
            Debug.LogError("AudioClip is null!");
            return null;
        }

        using (MemoryStream stream = new MemoryStream()) // Ensures stream is disposed
        {
            int sampleCount = clip.samples * clip.channels;
            int sampleRate = 16000;
            int channels = clip.channels;

            Debug.Log($"Sample Count: {sampleCount}, Sample Rate: {sampleRate}, Channels: {channels}");

            try
            {
                // Write WAV Header
                WriteWavHeader(stream, sampleCount, sampleRate, channels);
                Debug.Log("WAV header written successfully.");

                // Write PCM Data
                float[] samples = new float[sampleCount];
                clip.GetData(samples, 0);
                Debug.Log($"Retrieved {samples.Length} samples from AudioClip.");

                short[] intData = new short[sampleCount];

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * short.MaxValue);
                }

                byte[] byteData = new byte[intData.Length * 2];
                Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
                Debug.Log($"Converted PCM data to byte array. Byte array size: {byteData.Length}");

                Debug.Log("Attempting to write PCM data to stream...");
                stream.Write(byteData, 0, byteData.Length);
                Debug.Log("PCM data successfully written to stream.");

                // ✅ Convert stream to byte array before disposing
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during AudioClipToWav conversion: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        } // ✅ MemoryStream is automatically closed here
    }

    private static void WriteWavHeader(Stream stream, int sampleCount, int sampleRate, int channels)
    {
        Debug.Log("Writing WAV header...");
        try
        {
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) // ✅ Keeps stream open
            {
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + sampleCount * 2);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);

                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(sampleCount * 2);
            }
            Debug.Log("WAV header written successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing WAV header: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
