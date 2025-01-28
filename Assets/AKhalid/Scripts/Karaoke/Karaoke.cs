using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Karaoke
{
    public class Karaoke : MonoBehaviour
    {
        [Header("Setup")]
        public string sentenceToProcess;
        public TMP_Text sentence;
        public bool isRTL = true;

        [Header("Algorithm Adjustments")]
        [Tooltip("How much does a syllable affect the karaoke timing")]
        public float syllableWeight = 0.2f;

        [Tooltip("How much does the word length affect the karaoke timing")]
        public float wordCountWeight = 0.1f;

        [Header("Color Settings")]
        public Color startColor = new Color(0.6f, 0.6f, 0.6f, 1); // Gray
        public Color highlightColor = new Color(1f, 0.8f, 0f, 1); // Gold/Yellow

        private string[] words;
        private List<float> wordDurations;

        void Start()
        {
            ProcessSentence(sentenceToProcess, isRTL);
        }

        private void ProcessSentence(string sentenceText, bool rtl)
        {
            words = sentenceText.Split(' ');

            wordDurations = new List<float>();

            foreach (string word in words)
            {
                float duration = CalculateWordDuration(word);
                wordDurations.Add(duration);
                Debug.Log($"Word: {word}, Duration: {duration} sec");
            }

            StartCoroutine(KaraokeEffect());
        }

        float CalculateWordDuration(string word)
        {
            int syllables = EstimateSyllables(word);
            int length = word.Length;
            return Mathf.Max(0.2f, syllableWeight * syllables + wordCountWeight * length); // Ensure minimum time per word
        }

        private int EstimateSyllables(string word)
        {
            int count = 0;
            bool isVowel = false;

            string longVowels = "اوي";
            string shortVowels = "َُِ"; // فتحه ضمه و كسره

            foreach (char c in word)
            {
                if (longVowels.Contains(c) || shortVowels.Contains(c))
                {
                    if (!isVowel) count++;
                    isVowel = true;
                }
                else
                {
                    isVowel = false;
                }
            }

            return Math.Max(1, count); // Ensure at least 1 syllable per word
        }

        IEnumerator KaraokeEffect()
        {
            sentence.text = sentenceToProcess; // Keep full sentence visible
            string[] wordsInSentence = words; // Preserve original order

            float elapsedTime = 0f;

            for (int i = 0; i < wordsInSentence.Length; i++)
            {
                float duration = wordDurations[i];
                float progress = 0f;

                while (progress < 1f)
                {
                    progress += Time.deltaTime / duration;

                    // Build sentence with updated colors
                    string coloredText = "";
                    for (int j = 0; j < wordsInSentence.Length; j++)
                    {
                        int wordIndex = isRTL ? j : j; 
                        if (isRTL) wordIndex = wordsInSentence.Length - 1 - j; // Reverse indexing only when highlighting

                        Color wordColor = (wordIndex < i) ? highlightColor : // Words already completed stay highlighted
                                          (wordIndex == i) ? Color.Lerp(startColor, highlightColor, progress) : // Current word is transitioning
                                          startColor; // Future words stay default color

                        string hexColor = ColorUtility.ToHtmlStringRGBA(wordColor);
                        coloredText += $"<color=#{hexColor}>{wordsInSentence[j]}</color> ";
                    }

                    sentence.text = coloredText.Trim();
                    yield return null;
                }
            }

            // Ensure all words are fully highlighted at the end
            sentence.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(highlightColor)}>{sentenceToProcess}</color>";
        }
    }

}