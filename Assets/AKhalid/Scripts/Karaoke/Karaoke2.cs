using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Api.BaseMethods;
using Api.Models;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
namespace Karaoke{
public class Karaoke2 : MonoBehaviour
{   
    [Header("Karaoke")]
    public Main mainscript;
    public MicrophoneScript microphoneScript;
    public GameObject parentCanvas;
    public GameObject startExitCanvas;
    public GameObject loading;
    public GameObject scoreBoardCanvas;
    public TextMeshProUGUI scoreboardText;
    public GameObject karaokeCanvas;
    public GameObject FinishGameEffect; 
    public TextMeshProUGUI apiResponse; 
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
    public AudioClip finishExerciseSoundEffect;
    public AudioClip finishExerciseSoundEffect2;

    public AudioClip countdownBeep; 
    public float delayBeforeReset = 2f; 

    [Header("Exercise Data")]
    public List<string> sentences = new List<string>();
    public List<string> unReversedsentences = new List<string>();
    public int exerciseCount=0;

    public string exerciseId;
    public string exerciseType;
    public bool isGeneralExercise; 

    private int score;
    private int resetAmount;
    private int stutterAmount;

    void Start()
    {
        urpMaterial = exerciseLight.material;
    }

    public void SetupExercise(List<string> newSentences, string newExerciseId,bool isGeneralExercise)
    {
        FinishGameEffect.SetActive(false);
        apiResponse.text="";
        score=0;
        resetAmount=0;
        stutterAmount=0;

        exerciseCount=newSentences.Count;


        startExitCanvas.SetActive(true);
        karaokeCanvas.SetActive(false);
        parentCanvas.SetActive(true);
        scoreBoardCanvas.SetActive(false);

        sentences.Clear();
        unReversedsentences.Clear();
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
            unReversedsentences.Add(sentences[i]);
            sentences[i]=ArabicSupport.Fix(sentences[i]);
            
        }
        exerciseNumber.text="1/" + sentences.Count.ToString();
    }

    public void StartExercise()
    {   

        startExitCanvas.SetActive(false);
        karaokeCanvas.SetActive(true);
        scoreBoardCanvas.SetActive(false);
        score=0;

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
        microphoneScript.StartMonitoringSilence(WrongAnswer);

        textDisplay.text = ""; 

        for (int i = 0; i < words.Length; i++)
        {
            textDisplay.text = GetColoredSentence(i);
            yield return new WaitForSeconds(wordTime);
        }

        microphoneScript.StopMonitoringSilence();

        yield return new WaitForSeconds(0.5f);

        AudioClip audioClip = microphoneScript.StopRecording();

        

        ValidateSentence(audioClip,unReversedsentences[currentSentenceIndex]); // Proceed if completed
    }

public async Task ValidateSentence(AudioClip clip, string sentence)
{

    Debug.Log("Validating Sentence...");
    Debug.Log($"SENTENCE TO BE SENT TO API: {sentence}");

    byte[] wavData = AudioConverter.AudioClipToWav(clip);

    int maxRetries = 3; // Maximum number of retries
    int attempt = 0;

    while (attempt < maxRetries)
    {
        attempt++;
        Debug.Log($"API Call Attempt: {attempt}");

        try
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(20)); // Set timeout of 8 seconds
                
                Task<GenericApiResponse<SpeechAnalysisResult>> apiCallTask = SpeechAnalysis.ValidateSentence(wavData, sentence);
                Task completedTask = await Task.WhenAny(apiCallTask, Task.Delay(20000, cts.Token)); // Wait for response or timeout
                StopAllCoroutines();
                if (completedTask == apiCallTask) // API call completed successfully
                {
                    GenericApiResponse<SpeechAnalysisResult> response = await apiCallTask;

                    if (response.Data.success)
                    {
                        Debug.Log($"Transcription: {ArabicSupport.Fix(response.Data.metrics.transcription.text)}");
                        Debug.Log($"Fluency Score: {response.Data.metrics.fluency_score}");
                        Debug.Log($"repetitions: {response.Data.metrics.repetitions.Count}");

                    }
                    else
                    {
                        Debug.LogError("Error: ERROROROROROROROR");
                    }

                    Debug.Log($"Text Comparison similarity ratio: {response.Data.metrics.text_comparison.similarity_ratio}");
                    Debug.Log($"Text Comparison success: {response.Data.metrics.text_comparison.success}");

                    if (response.Data.metrics.text_comparison.success)
                    {


                        score+=response.Data.metrics.fluency_score;
                        stutterAmount+=response.Data.metrics.repetitions.Count;
                        stutterAmount+=response.Data.metrics.prolongations.Count;
                        CorrectAnswer();
                        Debug.LogError($"SUCCESS SCORE BEFORE: OLD SCORE {score}  ADD {response.Data.metrics.fluency_score}");


                        Debug.LogError($"SUCCESS NEW SCORE {score}");

                    }
                    else
                    {
                        WrongAnswer();
                    }

                    return; // Exit loop on success
                }
                else
                {
                    Debug.LogWarning("API Call Timeout: Retrying...");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"API Call Failed: {ex.Message}");
        }

        await Task.Delay(500); // Wait 1 second before retrying
    }

    Debug.LogError("Max retries reached: API request failed.");
    mainscript.LeaveExercise();
    StopAllCoroutines();
}

    
    string GetColoredSentence(int currentWordIndex)
    {
        string formattedText = "";
        int highlightIndex = words.Length - 1 - currentWordIndex; 

        for (int i = 0; i < words.Length; i++)
        {
            if (i == highlightIndex)
                formattedText += $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(highlightColor)}>{words[i]}</color> ";
            else
                formattedText += $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(defaultColor)}>{words[i]}</color> ";
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

        if(currentSentenceIndex+1<=sentences.Count)
        exerciseNumber.text=(currentSentenceIndex+1) +"/" + sentences.Count.ToString();

        if (currentSentenceIndex < sentences.Count)
        {
            StopAllCoroutines(); // Stop any running coroutines before restarting
            StartCoroutine(StartCountdown()); 
        }
        else
        {
            ShowFinalMenuAsync();
            StopAllCoroutines();
            Debug.Log("Exercise Complete!");
        }
    }


    public async Task ShowFinalMenuAsync()
    {
        

        parentCanvas.SetActive(true);
        scoreBoardCanvas.SetActive(true);
        karaokeCanvas.SetActive(false);
        startExitCanvas.SetActive(false);

        int scorenormalized=score/exerciseCount;

        if(!isGeneralExercise)
        {
        
        loading.SetActive(true);
        var getGeneralExercises = await Api.BaseMethods.Unity.SaveExerciseInformation(exerciseId.ToString(), scorenormalized.ToString(), stutterAmount.ToString(), "0",resetAmount.ToString());
        if(!getGeneralExercises.IsSuccess)
        {
            apiResponse.text=getGeneralExercises.error;
        }
        mainscript.ResetPatientExercises();
        }
        loading.SetActive(false);
        FinishGameEffect.SetActive(true);
        audioSource.PlayOneShot(finishExerciseSoundEffect);
        audioSource.PlayOneShot(finishExerciseSoundEffect2);

        scoreboardText.text=scorenormalized.ToString();


    }
    void ResetSentence()
    {
        microphoneScript.StopMonitoringSilence();
        StopAllCoroutines(); // Stop any running sentence animation immediately
        StartCoroutine(StartCountdown()); 
        resetAmount+=1;
    }
}
}