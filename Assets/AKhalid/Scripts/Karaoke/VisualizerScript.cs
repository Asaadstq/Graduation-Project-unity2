using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Karaoke
{
public class VisualizerScript : MonoBehaviour
{
    [Header("Visualizer Settings")]
    public float minHeight = 15.0f;
    public float maxHeight = 425.0f;
    public float updateSentivity = 10.0f;
    public Color visualizerColor = Color.gray;
    [Range (64, 8192)]
    public int visualizerSamples = 64;   // number of samples to retrieve from the spectrum

    [Header("Audio Source Settings")]
    public bool useMicrophone = true;    // toggle to use microphone or audioClip
    public int microphoneIndex = 0;      // index of the microphone in Microphone.devices
    public bool loop = true;            // loop the audio
    public AudioSource audioSource;

    // The UI elements that will visualize the audio
    public GameObject[] visualizerObjects;

    void Start()
    {
        audioSource.loop = loop;

        if (useMicrophone)
        {
            // Check if there are any microphone devices
            if (Microphone.devices.Length > 0)
            {
                // Make sure our index is in range
                if (microphoneIndex >= Microphone.devices.Length)
                {
                    Debug.LogWarning("Microphone index out of range; using first available microphone.");
                    microphoneIndex = 0;
                }

                // Get the microphone device name
                string selectedMic = Microphone.devices[microphoneIndex];

                // Start recording from the microphone
                // 10 seconds duration, 44100 Hz sample rate (adjust as needed)
                audioSource.clip = Microphone.Start(selectedMic, loop, 10, 44100);

                // Wait until the microphone has started recording
                while (Microphone.GetPosition(selectedMic) <= 0) { }

                // Play the recorded audio through the AudioSource
                audioSource.Play();
            }
            else
            {
                Debug.LogError("No microphone devices found. Check if your microphone is connected/enabled.");
            }
        }
    }

    void FixedUpdate()
    {
        if (audioSource == null) return;

        // Get the current spectrum data
        float[] spectrumData = audioSource.GetSpectrumData(visualizerSamples, 0, FFTWindow.Rectangular);

        // Update each bar in the visualizer
        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            // Get the current size of the bar
            RectTransform barRect = visualizerObjects[i].GetComponent<RectTransform>();
            Vector2 newSize = barRect.rect.size;

            // Calculate new height based on spectrum data
            float spectrumValue = spectrumData[i] * (maxHeight - minHeight) * 50.0f;
            float targetHeight = minHeight + spectrumValue;

            // Smoothly interpolate to the target height
            newSize.y = Mathf.Clamp(
                Mathf.Lerp(newSize.y, targetHeight, updateSentivity * Time.deltaTime),
                minHeight,
                maxHeight
            );

            // Apply the new size and color
            barRect.sizeDelta = newSize;
            visualizerObjects[i].GetComponent<Image>().color = visualizerColor;
        }
    }
}
}