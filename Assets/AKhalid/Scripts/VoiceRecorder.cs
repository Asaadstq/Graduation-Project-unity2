using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class VoiceRecorder : MonoBehaviour
{
    [Header("OpenAI API Settings")]
    [Tooltip("Your OpenAI Secret Key (keep it safe!)")]
    public string openAIApiKey = "sk-proj-CTEStQO_ukP2U1_zqRB6ZuLQQuN12hvn4L0hmhgh7Vrvv9MKBtTwxVV22gZta-DaHaxPlrVJ3oT3BlbkFJxcXRi0nxehl1cI5Lux5Hx7dj6anFMI_-6Jj4y3HRJ9CVrf20G0fqQXcjCwH34E0F_mwjdvkPAA";

    [Header("Recording Settings")]
    [Tooltip("Maximum length (in seconds) of the audio recording.")]
    public int maxRecordingLength = 10;   // e.g., 10 seconds


    [Header("UI")]
    public Button startRecording;
    public Button stopRecording;
    public TMP_InputField transcriptionText;
    public Image StartStopColor;



    private AudioClip recordedClip;
    private string microphoneDevice;
    private bool isRecording;


    void Start(){
        Debug.Log(Microphone.devices[2]);

        startRecording.onClick.AddListener(()=>StartRecording());
        stopRecording.onClick.AddListener(()=>StopRecordingAndTranscribe());

    }
    /// <summary>
    /// Call this method (e.g., from a UI button) to start recording.
    /// </summary>
    public void StartRecording()
    {
        // Make sure user has at least one microphone device
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found!");
            return;
        }

        microphoneDevice = Microphone.devices[2]; 
        isRecording = true;

        StartStopColor.color=Color.green;

        // Start recording
        recordedClip = Microphone.Start(microphoneDevice, 
                                        loop: false, 
                                        lengthSec: maxRecordingLength, 
                                        frequency: 44100);
        Debug.Log("Recording started...");
    }

    /// <summary>
    /// Call this method (e.g., from a UI button) to stop recording and send to OpenAI.
    /// </summary>
    public void StopRecordingAndTranscribe()
    {
        if (!isRecording)
            return;

        StartStopColor.color=Color.red;

        isRecording = false;
        Microphone.End(microphoneDevice);
        Debug.Log("Recording stopped. Sending audio to OpenAI...");

        // Convert AudioClip to WAV byte array
        byte[] wavData = ConvertAudioClipToWav(recordedClip);

        // Send WAV to OpenAI for transcription
        StartCoroutine(SendWavToOpenAI(wavData));
    }

    /// <summary>
    /// Convert an AudioClip to a WAV byte array in memory.
    /// </summary>
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null, cannot convert to WAV.");
            return null;
        }

        // Get the raw data from the AudioClip
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // Convert floats to 16-bit PCM data
        short[] intData = new short[samples.Length];
        const int RESCALE_FACTOR = 32767; // to convert float[-1.0..1.0] to short
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * RESCALE_FACTOR);
        }

        // Prepare WAV header + data
        // For simplicity, this example only handles PCM 16-bit, single or stereo
        byte[] wavBytes = WriteWavFile(intData, clip.channels, clip.frequency);

        return wavBytes;
    }

    /// <summary>
    /// Writes the necessary WAV header and the PCM data.
    /// </summary>
    private byte[] WriteWavFile(short[] intData, int channels, int sampleRate)
    {
        // Reference: 
        //   RIFF/WAVE header format: 
        //   https://web.archive.org/web/20160304001642/http://www-mmsp.ece.mcgill.ca/documents/audioformats/wave/wave.html

        MemoryStream memoryStream = new MemoryStream();

        // Calculate byte count
        int byteCount = intData.Length * sizeof(short);

        // RIFF header
        // Chunk ID
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        // Chunk Size (36 + SubChunk2Size)
        memoryStream.Write(BitConverter.GetBytes(36 + byteCount), 0, 4);
        // Format
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);

        // fmt sub-chunk
        // Subchunk1 ID
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        // Subchunk1 size (16 for PCM)
        memoryStream.Write(BitConverter.GetBytes(16), 0, 4);
        // Audio format (1 = PCM)
        memoryStream.Write(BitConverter.GetBytes((short)1), 0, 2);
        // Num channels
        memoryStream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        // Sample rate
        memoryStream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        // Byte rate (sampleRate * channels * bitsPerSample/8)
        memoryStream.Write(BitConverter.GetBytes(sampleRate * channels * 16 / 8), 0, 4);
        // Block align (channels * bitsPerSample/8)
        memoryStream.Write(BitConverter.GetBytes((short)(channels * 16 / 8)), 0, 2);
        // Bits per sample
        memoryStream.Write(BitConverter.GetBytes((short)16), 0, 2);

        // data sub-chunk
        // Subchunk2 ID
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        // Subchunk2 size (numSamples * numChannels * bitsPerSample/8)
        memoryStream.Write(BitConverter.GetBytes(byteCount), 0, 4);

        // PCM data
        for (int i = 0; i < intData.Length; i++)
        {
            memoryStream.Write(BitConverter.GetBytes(intData[i]), 0, 2);
        }

        return memoryStream.ToArray();
    }

 private IEnumerator SendWavToOpenAI(byte[] wavData)
    {
        if (wavData == null)
        {
            Debug.LogError("No WAV data to send.");
            yield break;
        }

        string url = "https://api.openai.com/v1/audio/transcriptions";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"OpenAI request failed: {request.error}\n{request.downloadHandler.text}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"OpenAI Response: {jsonResponse}");

                try
                {
                    // Deserialize the JSON response
                    OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
                    transcriptionText.text=ArabicSupport.Fix(response.text);
                    // Display the transcribed text
                    Debug.Log($"Transcription: {response.text}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON Parsing Error: {ex.Message}");
                }
            }
        }
    }

        [Serializable]
    private class OpenAIResponse
    {
        public string text;  // The transcribed text
    }

}
