using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Karaoke{
public class Karaoke2 : MonoBehaviour
{   
    [Header("Karaoke")]
    public Main mainscript;
    public MicrophoneScript microphoneScript;
    public GameObject parentCanvas;
    public GameObject startExitCanvas;
    public GameObject karaokeCanvas;
    public TextMeshProUGUI textDisplay; 
    public TextMeshProUGUI countdownDisplay;
    public TextMeshProUGUI exerciseNumber;
    public float wordsPerMinute = 200f; 
    public Color highlightColor = Color.red;
    public Color defaultColor = Color.black;
    private string[] words;
    private float wordTime;
    private int currentSentenceIndex = 0;
    private Coroutine activeKaraokeCoroutine; // Track current karaoke coroutine

    [Header("Material Settings")]
    public Renderer exerciseLight;
    private Material urpMaterial; 
    public string hdrProperty = "_EmissionColor"; 
    public Color correctColor = Color.green; 
    public Color wrongColor = Color.red; 
    public Color idleColor = Color.white; 
    public float targetIntensity = 3f; 

    [Header("Audio Settings")]
    public AudioSource audioSource; 
    public AudioClip correctSoundEffect;
    public AudioClip wrongSoundEffect;
    public AudioClip countdownBeep; 
    public float delayBeforeReset = 2f; 

    [Header("Exercise Data")]
    public List<string> sentences = new List<string>();
    public string exerciseId;
    public string exerciseType;
    public bool isGeneralExercise; 

    void Start()
    {
        urpMaterial = exerciseLight.material;
    }

    public void SetupExercise(List<string> newSentences, string newExerciseId,bool isGeneralExercise)
    {
        startExitCanvas.SetActive(true);
        karaokeCanvas.SetActive(false);
        parentCanvas.SetActive(true);

        //Stop the teachers animator
        Animator teacherAnim= mainscript.TeacherAtBoard.GetComponent<Animator>();
        teacherAnim.Play("TeacherExerciseLoop",0,0);
        teacherAnim.speed = 0; // Pause animation

        sentences = newSentences;
        exerciseId = newExerciseId;
        this.isGeneralExercise=isGeneralExercise;
        
        currentSentenceIndex = 0;

        for (int i = 0; i < sentences.Count; i++)

        {
            sentences[i]=ArabicSupport.Fix(sentences[i]);
            
        }
        exerciseNumber.text="1/" + sentences.Count.ToString();
    }

    public void StartExercise()
    {   

        startExitCanvas.SetActive(false);
        karaokeCanvas.SetActive(true);

        if (sentences.Count == 0) return;
        StopAllCoroutines(); // Ensure everything resets
        StartCoroutine(StartCountdown()); 
    }

    IEnumerator StartCountdown()
    {
        countdownDisplay.text = "";
        textDisplay.text = sentences[currentSentenceIndex];         
        for (int i = 3; i > 0; i--)
        {
            countdownDisplay.text = i.ToString();
            if (audioSource != null && countdownBeep != null)
            {
                audioSource.PlayOneShot(countdownBeep);
            }
            yield return new WaitForSeconds(1f);
        }
        countdownDisplay.text = "";
        StartSentence();
    }

    void StartSentence()
    {
        //Start the teachers animator
        Animator teacherAnim= mainscript.TeacherAtBoard.GetComponent<Animator>();
        teacherAnim.Play("TeacherExerciseLoop",0,0);
        teacherAnim.speed = 1;

        StopAllCoroutines(); // Stop previous sentence animations if needed
        words = textDisplay.text.Split(' '); 
        wordTime = CalculateWordTime(words.Length);

        activeKaraokeCoroutine = StartCoroutine(AnimateText()); // Store coroutine reference
    }

    float CalculateWordTime(int wordCount)
    {
        return 60f / wordsPerMinute; 
    }

    IEnumerator AnimateText()
    {
        microphoneScript.StartRecording();
        textDisplay.text = ""; 

        for (int i = 0; i < words.Length; i++)
        {
            textDisplay.text = GetColoredSentence(i);
            yield return new WaitForSeconds(wordTime);
        }
        AudioClip audioClip = microphoneScript.StopRecording();

        NextSentence(); // Proceed if completed
    }


    public void ValidateSentence(){

        
    }
        
    
    string GetColoredSentence(int currentWordIndex)
    {
        string formattedText = "";
        int highlightIndex = words.Length - 1 - currentWordIndex; 

        for (int i = 0; i < words.Length; i++)
        {
            if (i == highlightIndex)
                formattedText += $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{words[i]}</color> ";
            else
                formattedText += $"<color=#{ColorUtility.ToHtmlStringRGB(defaultColor)}>{words[i]}</color> ";
        }

        return formattedText.Trim();
    }

    public void CorrectAnswer()
    {
        if (urpMaterial == null) return;

        Color adjustedColor = correctColor * Mathf.Pow(2, targetIntensity); 
        urpMaterial.SetColor(hdrProperty, adjustedColor);

        if (audioSource != null && correctSoundEffect != null)
        {
            audioSource.PlayOneShot(correctSoundEffect);
        }

        StartCoroutine(ResetEffectAfterDelay());
        NextSentence();
    }

    public void WrongAnswer()
    {
        if (urpMaterial == null) return;

        Color adjustedColor = wrongColor * Mathf.Pow(2, targetIntensity); 
        urpMaterial.SetColor(hdrProperty, adjustedColor);

        if (audioSource != null && wrongSoundEffect != null)
        {
            audioSource.PlayOneShot(wrongSoundEffect);
        }

        StartCoroutine(ResetEffectAfterDelay());
        ResetSentence();
    }

    IEnumerator ResetEffectAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeReset);
        urpMaterial.SetColor(hdrProperty, idleColor * 1);
    }

    void NextSentence()
    {
        currentSentenceIndex++;

        if(currentSentenceIndex+1<sentences.Count)
        exerciseNumber.text=(currentSentenceIndex+1) +"/" + sentences.Count.ToString();

        if (currentSentenceIndex < sentences.Count)
        {
            StopAllCoroutines(); // Stop any running coroutines before restarting
            StartCoroutine(StartCountdown()); 
        }
        else
        {
            mainscript.LeaveExercise();
            Debug.Log("Exercise Complete!");
        }
    }

    void ResetSentence()
    {
        StopAllCoroutines(); // Stop any running sentence animation immediately
        StartCoroutine(StartCountdown()); 
    }
}
}