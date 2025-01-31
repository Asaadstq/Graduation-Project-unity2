using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Karaoke
{
    public class MicrophoneScript : MonoBehaviour
    {
        [Header("Visualizer Settings")]
        public float minHeight = 15.0f;
        public float maxHeight = 425.0f;
        public float updateSensitivity = 10.0f;
        public Color visualizerColor = Color.gray;
        [Range(64, 8192)]
        public int visualizerSamples = 64;

        [Header("Audio Source Settings")]
        public bool useMicrophone = true;
        public int microphoneIndex = 0;
        public bool loop = true;
        public AudioSource audioSource;

        [Header("Silence Detection Settings")]
        public float silenceThreshold = 0.02f; // Adjust for silence sensitivity
        public float silenceDuration = 2.0f; // Time in seconds before silence is triggered
        private Coroutine silenceCoroutine;
        private bool isMonitoringSilence = false;

        // The UI elements that will visualize the audio
        public GameObject[] visualizerObjects;

        private string selectedMic;
        private bool isRecording = false;

        void Start()
        {
            audioSource.loop = loop;

            if (useMicrophone)
            {
                if (Microphone.devices.Length > 0)
                {
                    if (microphoneIndex >= Microphone.devices.Length)
                    {
                        Debug.LogWarning("Microphone index out of range; using first available microphone.");
                        microphoneIndex = 0;
                    }

                    selectedMic = Microphone.devices[microphoneIndex];
                    StartRecording();
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

            float[] spectrumData = audioSource.GetSpectrumData(visualizerSamples, 0, FFTWindow.Rectangular);

            for (int i = 0; i < visualizerObjects.Length; i++)
            {
                RectTransform barRect = visualizerObjects[i].GetComponent<RectTransform>();
                Vector2 newSize = barRect.rect.size;

                float spectrumValue = spectrumData[i] * (maxHeight - minHeight) * 50.0f;
                float targetHeight = minHeight + spectrumValue;

                newSize.y = Mathf.Clamp(
                    Mathf.Lerp(newSize.y, targetHeight, updateSensitivity * Time.deltaTime),
                    minHeight,
                    maxHeight
                );

                barRect.sizeDelta = newSize;
                visualizerObjects[i].GetComponent<Image>().color = visualizerColor;
            }
        }

        // 📌 Starts recording from the microphone
        public void StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("Already recording!");
                return;
            }

            if (Microphone.IsRecording(selectedMic))
            {
                Debug.LogWarning("Microphone is already recording.");
                return;
            }

            Debug.Log("Starting microphone recording...");
            audioSource.clip = Microphone.Start(selectedMic, loop, 10, 44100);

            while (Microphone.GetPosition(selectedMic) <= 0) { }

            audioSource.Play();
            isRecording = true;
        }

        // 📌 Stops recording and returns the recorded audio data
        public AudioClip StopRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("Not currently recording.");
                return null;
            }

            Debug.Log("Stopping microphone recording...");
            Microphone.End(selectedMic);
            isRecording = false;
            return audioSource.clip;
        }

        // 📌 Monitors the microphone for silence and triggers an action if detected
        public void StartMonitoringSilence(Action onSilenceDetected)
        {
            if (isMonitoringSilence)
            {
                Debug.LogWarning("Already monitoring for silence.");
                return;
            }

            silenceCoroutine = StartCoroutine(MonitorSilence(onSilenceDetected));
        }

        // 📌 Stops monitoring the microphone for silence
        public void StopMonitoringSilence()
        {
            if (silenceCoroutine != null)
            {
                StopCoroutine(silenceCoroutine);
                silenceCoroutine = null;
            }
            isMonitoringSilence = false;
        }

        // 📌 Coroutine to detect silence
        private IEnumerator MonitorSilence(Action onSilenceDetected)
        {
            isMonitoringSilence = true;
            float silenceTimer = 0f;
            float[] sampleData = new float[visualizerSamples];

            while (isMonitoringSilence)
            {
                audioSource.GetOutputData(sampleData, 0);
                float averageVolume = 0f;

                foreach (float sample in sampleData)
                {
                    averageVolume += Mathf.Abs(sample);
                }

                averageVolume /= visualizerSamples;

                if (averageVolume < silenceThreshold)
                {
                    silenceTimer += Time.deltaTime;
                    if (silenceTimer >= silenceDuration)
                    {
                        Debug.Log("Silence detected!");
                        onSilenceDetected?.Invoke();
                        StopMonitoringSilence();
                    }
                }
                else
                {
                    silenceTimer = 0f;
                }

                yield return null;
            }
        }
    }
}
