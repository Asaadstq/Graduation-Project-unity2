using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.Models;
using DG.Tweening;
using Karaoke;
using PrefabSpecificScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    public class Main : MonoBehaviour
    {

        [Header("General")]
        public Camera mainCamera;
        public GameObject TeacherAtDesk;
        public GameObject TeacherAtBoard;
        public Karaoke2 karaokeScript;
        public AudioClip pageFlipAudio;
        public AudioSource audiosource;
        public ResetXROriginOnStart resetXrOriginScript;
        

        [Header("Fade Camera")]
        public Renderer  blackCameraFadeCube;
        public float fadeDuration = 1.0f;
        private Material material;

        [Header("Login UI")]
        public CanvasGroup Login_UI;
        public TMP_InputField Login_Email;
        public TMP_InputField Login_Password;
        public Button Login_LoginButton;
        public Button Login_GoToSignupPage;
        public GameObject Login_Loading;
        public TMP_Text Login_ApiResponse;

        [Header("Signup UI")]
        public CanvasGroup Signup_UI;
        public TMP_InputField Signup_Name;
        public TMP_InputField Signup_Email;
        public TMP_InputField Signup_Password;
        public TMP_InputField Signup_Age;
        public Button Signup_SignupBUtton;
        public Button Signup_GoToLoginPage;
        public GameObject Signup_Loading;
        public TMP_Text Signup_ApiResponse;

        [Header("Profile UI")]
        public CanvasGroup Profile_UI;
        public TMP_InputField Profile_Name;
        public Button Profile_LogoutButton;
        public TMP_InputField Profile_Email;
        public TMP_InputField Profile_Age;
        public GameObject Profile_Loading;
        public TMP_Text Profile_ApiResponse;
        public GameObject Profile_DoctorNamePrefab;
        public GameObject Profile_DoctorNamePrefabParent;

        [Header("Book")]
        public Book PatientExercises_Book;
        public string PatientExercises_MaxPageCount;
        public AutoFlip PatientExercises_AutoFlipScript;
        public Book GeneralExercises_Book;
        public string GeneralExercises_MaxPageCount;
        public AutoFlip GeneralExercises_AutoFlipScript;
        public Sprite Book_PageSprite;


        [Header("Patient Exercises")]
        public CanvasGroup PatientExercises_CanvasGroup;
        public Button PatientExercises_NextPage;
        public Button PatientExercises_PreviousPage;
        public Slider PatientExercises_Slider;
        public TMP_Text PatientExercises_SliderCurrentPageText;
        public ExercisePagePrefab PatientExercises_ExerciseLeftPageDetails;
        public ExercisePagePrefab PatientExercises_ExerciseRightPageDetails;
        public GameObject PatientExercises_Loading;
        public TMP_Text PatientExercises_ApiResponse;
        public Sprite PatientExercises_PageCoverSprite;

        [Header("General Exercises")]
        public CanvasGroup GeneralExercises_CanvasGroup;
        public Button GeneralExercises_NextPage;
        public Button GeneralExercises_PreviousPage;
        public Slider GeneralExercises_Slider;
        public TMP_Text GeneralExercises_SliderCurrentPageText;
        public ExercisePagePrefab GeneralExercises_ExerciseLeftPageDetails;
        public ExercisePagePrefab GeneralExercises_ExerciseRightPageDetails;
        public GameObject GeneralExercises_Loading;
        public TMP_Text GeneralExercises_ApiResponse;
        public Sprite GeneralExercises_PageCoverSprite;



        private List<PatientExercise> patientExercises=new List<PatientExercise>();
        private List<Exercise> patientExercisesDetails=new List<Exercise>();
        private List<Exercise> generalExercises=new List<Exercise>();

        async Task Start()
        {
            await Setup();
        }

        public async Task Setup()
        {
            material=blackCameraFadeCube.material;
            //Setup Sliders
            PatientExercises_Slider.onValueChanged.AddListener((value) => ChangePersonalExercisePage((int)value));
            GeneralExercises_Slider.onValueChanged.AddListener((value) => ChangeGeneralExercisePage((int)value));

            //Login Signup UI
            Login_LoginButton.onClick.AddListener(Login);
            Signup_SignupBUtton.onClick.AddListener(Signup);

            Login_GoToSignupPage.onClick.AddListener(() => GoToSignUpPage());
            Signup_GoToLoginPage.onClick.AddListener(() => GoToLoginPage());

            //Logout
            Profile_LogoutButton.onClick.AddListener(LogoutAndGoBacktoLoginScreen);
            //Begin by hiding everything and displaying the login and canvas
            ShowHideUI(Login_UI,true);
            ShowHideUI(Signup_UI,false);
            ShowHideUI(Profile_UI,false);
            GeneralExercises_CanvasGroup.interactable=false;
            PatientExercises_CanvasGroup.interactable=false;

            //Get auth token from player prefs  
            Constants.Constants.AUTHKEY = Constants.PlayerPrefsManager.GetString("authorization");
            Constants.Constants.USERID = Constants.PlayerPrefsManager.GetString("userid");

            //Start the LoginUI loading
            Login_Loading.SetActive(true);

            //Call the getpatientinfo endpoint to validate the auth token, if it fails then force the user to login
            var getPatientInfoResponse = await Api.BaseMethods.Unity.GetPatientInfo();
            if(!getPatientInfoResponse.IsSuccess)
            {
                Login_Loading.SetActive(false);
                return;

            }

            //Hide login and signup UI
            ShowHideUI(Login_UI,false);
            ShowHideUI(Signup_UI,false);

            //Setup profile and exercises
            SetupProfile();
            SetupPatientExercises();
            SetupGeneralExercises();

        }


    public void GoToSignUpPage()
    {
        ShowHideUI(Login_UI,false);
        ShowHideUI(Signup_UI,true);
        
    }
    public void GoToLoginPage()
    {
        ShowHideUI(Login_UI,true);
        ShowHideUI(Signup_UI,false);
        
    }

    public void PlayPageFlipAudio()
    {

        audiosource.PlayOneShot(pageFlipAudio,0.1f);

    }

    public void ChangePersonalExercisePage(int pagenumber)
    { 


        //If page is even
        if (pagenumber%2==0)
        {
            
            PatientExercises_Book.currentPage =  pagenumber;
            PatientExercises_Book.UpdateSprites();
            PatientExercises_SliderCurrentPageText.text=   PatientExercises_Book.currentPage .ToString() + "/" + (int.Parse(PatientExercises_MaxPageCount)+2).ToString();

        }
        //if page number is odd
        else
        {

            PatientExercises_Book.currentPage = pagenumber + 1;
            PatientExercises_Book.UpdateSprites();
            PatientExercises_SliderCurrentPageText.text=  PatientExercises_Book.currentPage .ToString() + "/" +(int.Parse(PatientExercises_MaxPageCount)+2).ToString();

        }

        //Show exercise details if not first or last page
        if(PatientExercises_Book.currentPage<2 || PatientExercises_Book.currentPage > int.Parse(PatientExercises_MaxPageCount) ){
            PatientExercises_ExerciseLeftPageDetails.gameObject.SetActive(false);
            PatientExercises_ExerciseRightPageDetails.gameObject.SetActive(false);
            return;
        }
        
        PatientExercises_ExerciseLeftPageDetails.gameObject.SetActive(false);
        PatientExercises_ExerciseRightPageDetails.gameObject.SetActive(false);

        //Fill out exercises for each page and display it
        if(PatientExercises_Book.currentPage -1 < patientExercises.Count && PatientExercises_Book.currentPage -1 < patientExercisesDetails.Count){

        PatientExercises_ExerciseRightPageDetails.exercisename.text=patientExercisesDetails[PatientExercises_Book.currentPage-1].exercisename;

        PatientExercises_ExerciseRightPageDetails.completed.text=patientExercises[PatientExercises_Book.currentPage-1].completed==1?"Yes":"No";
        PatientExercises_ExerciseRightPageDetails.dateassigned.text=patientExercises[PatientExercises_Book.currentPage-1].dateassigned.ToString();
        PatientExercises_ExerciseRightPageDetails.datecompleted.text=patientExercises[PatientExercises_Book.currentPage-1].datecompleted.ToString();
        PatientExercises_ExerciseRightPageDetails.score.text=patientExercises[PatientExercises_Book.currentPage-1].score;
        PatientExercises_ExerciseRightPageDetails.stutteramount.text=patientExercises[PatientExercises_Book.currentPage-1].stutteramount.ToString();
        PatientExercises_ExerciseRightPageDetails.resetamount.text=patientExercises[PatientExercises_Book.currentPage-1].resetamount.ToString();
        
        PatientExercises_ExerciseRightPageDetails.startexercise.onClick.RemoveAllListeners();
        PatientExercises_ExerciseRightPageDetails.startexercise.onClick.AddListener(()=> GoToExercise(patientExercisesDetails[PatientExercises_Book.currentPage-1].sentences.Split(",").ToList<String>(),patientExercisesDetails[PatientExercises_Book.currentPage-1].exerciseid,false));

        PatientExercises_ExerciseRightPageDetails.gameObject.SetActive(true);
        }


        PatientExercises_ExerciseLeftPageDetails.exercisename.text=patientExercisesDetails[PatientExercises_Book.currentPage-2].exercisename;

        PatientExercises_ExerciseLeftPageDetails.completed.text=patientExercises[PatientExercises_Book.currentPage-2].completed==1?"Yes":"No";
        PatientExercises_ExerciseLeftPageDetails.dateassigned.text=patientExercises[PatientExercises_Book.currentPage-2].dateassigned.ToString();
        PatientExercises_ExerciseLeftPageDetails.datecompleted.text=patientExercises[PatientExercises_Book.currentPage-2].completed==1?patientExercises[PatientExercises_Book.currentPage-2].datecompleted.ToString():"";
        PatientExercises_ExerciseLeftPageDetails.score.text=patientExercises[PatientExercises_Book.currentPage-2].score;
        PatientExercises_ExerciseLeftPageDetails.stutteramount.text=patientExercises[PatientExercises_Book.currentPage-2].stutteramount.ToString();
        PatientExercises_ExerciseLeftPageDetails.resetamount.text=patientExercises[PatientExercises_Book.currentPage-2].resetamount.ToString();

        PatientExercises_ExerciseLeftPageDetails.startexercise.onClick.RemoveAllListeners();
        PatientExercises_ExerciseLeftPageDetails.startexercise.onClick.AddListener(()=> GoToExercise(patientExercisesDetails[PatientExercises_Book.currentPage-2].sentences.Split(",").ToList<string>(),patientExercisesDetails[PatientExercises_Book.currentPage-2].exerciseid,false));


        PatientExercises_ExerciseLeftPageDetails.gameObject.SetActive(true);
        


    }

    public void GoToNextOrPreviousPagePersonalExercise(bool adding)
    {
        
        if(adding)
        PatientExercises_AutoFlipScript.FlipRightPage();
        else
        PatientExercises_AutoFlipScript.FlipLeftPage();


        PlayPageFlipAudio();
    }

        public void ChangeGeneralExercisePage(int pagenumber)
    { 


        //If page is even
        if (pagenumber%2==0)
        {
            
            GeneralExercises_Book.currentPage =  pagenumber;
            GeneralExercises_Book.UpdateSprites();
            GeneralExercises_SliderCurrentPageText.text=  GeneralExercises_Book.currentPage.ToString() + "/" +(int.Parse(GeneralExercises_MaxPageCount)+2).ToString();

        }
        //if page number is odd
        else
        {

            GeneralExercises_Book.currentPage =  pagenumber + 1;
            GeneralExercises_Book.UpdateSprites();
            GeneralExercises_SliderCurrentPageText.text=  GeneralExercises_Book.currentPage.ToString() + "/" +(int.Parse(GeneralExercises_MaxPageCount)+2).ToString();

        }

        //Show exercise details if not first or last page
        if(GeneralExercises_Book.currentPage<2 || GeneralExercises_Book.currentPage > int.Parse(GeneralExercises_MaxPageCount) ){
            GeneralExercises_ExerciseLeftPageDetails.gameObject.SetActive(false);
            GeneralExercises_ExerciseRightPageDetails.gameObject.SetActive(false);
            return;
        }

        GeneralExercises_ExerciseLeftPageDetails.gameObject.SetActive(false);
        GeneralExercises_ExerciseRightPageDetails.gameObject.SetActive(false);

        //Fill out exercises for each page and display it
        if(GeneralExercises_Book.currentPage-1< generalExercises.Count){
        GeneralExercises_ExerciseRightPageDetails.exercisename.text=generalExercises[GeneralExercises_Book.currentPage-1].exercisename;
        GeneralExercises_ExerciseRightPageDetails.completed.text=ArabicSupport.Fix(generalExercises[GeneralExercises_Book.currentPage-1].sentences);
        GeneralExercises_ExerciseRightPageDetails.gameObject.SetActive(true);

        GeneralExercises_ExerciseRightPageDetails.startexercise.onClick.RemoveAllListeners();
        GeneralExercises_ExerciseRightPageDetails.startexercise.onClick.AddListener(()=> GoToExercise(generalExercises[GeneralExercises_Book.currentPage-1].sentences.Split(",").ToList<string>(),generalExercises[GeneralExercises_Book.currentPage-1].exerciseid,true));

        }

        GeneralExercises_ExerciseLeftPageDetails.exercisename.text=generalExercises[GeneralExercises_Book.currentPage-2].exercisename;
        GeneralExercises_ExerciseLeftPageDetails.completed.text=ArabicSupport.Fix(generalExercises[GeneralExercises_Book.currentPage-2].sentences);

        GeneralExercises_ExerciseLeftPageDetails.startexercise.onClick.RemoveAllListeners();
        GeneralExercises_ExerciseLeftPageDetails.startexercise.onClick.AddListener(()=> GoToExercise(generalExercises[GeneralExercises_Book.currentPage-2].sentences.Split(",").ToList<string>(),generalExercises[GeneralExercises_Book.currentPage-2].exerciseid,true));

        GeneralExercises_ExerciseLeftPageDetails.gameObject.SetActive(true);


    }


    public void GoToNextOrPreviousPageGeneralExercise(bool adding)
    {
        
        if(adding)
        GeneralExercises_AutoFlipScript.FlipRightPage();
        else
        GeneralExercises_AutoFlipScript.FlipLeftPage();

        PlayPageFlipAudio();

    }

        public async void Login()
        {
            //Start the LoginUI loading
            Login_Loading.SetActive(true);

            var loginResponse = await Api.BaseMethods.Validation.Login(Login_Email.text,Login_Password.text);
            if(!loginResponse.IsSuccess)
            {
                Login_Loading.SetActive(false);
                Login_ApiResponse.text=loginResponse.error;
                return;

            }


            //Hide login and signup UI
            ShowHideUI(Login_UI,false);
            ShowHideUI(Signup_UI,false);

            //Setup profile and exercises
            SetupProfile();
            SetupPatientExercises();
            SetupGeneralExercises();
        }
        public async void Signup()
        {
            //Start the LoginUI loading
            Login_Loading.SetActive(true);

            var signupResponse = await Api.BaseMethods.Validation.Signup(Signup_Email.text,Signup_Password.text,Signup_Name.text,Signup_Age.text);
            if(!signupResponse.IsSuccess)
            {
                Signup_Loading.SetActive(false);
                Signup_ApiResponse.text=signupResponse.error;
                return;

            }
            

            //Hide login and signup UI
            ShowHideUI(Login_UI,false);
            ShowHideUI(Signup_UI,false);

            //Setup profile and exercises
            SetupProfile();
            SetupPatientExercises();
            SetupGeneralExercises();
        }

        public async void SetupProfile(){

            //Display profile UI
            ShowHideUI(Profile_UI,true);
            
            //Start profile loading
            Profile_Loading.SetActive(true);

            //Get user profile
            var getPatientInfoResponse = await Api.BaseMethods.Unity.GetPatientInfo();
            if(!getPatientInfoResponse.IsSuccess)
            {
                    //Response code forbidden means invalid auth token so return to login
                    if (getPatientInfoResponse.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    Profile_Loading.SetActive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {
                    //If its any other error just display it and continue no need to exit
                    Profile_ApiResponse.text=getPatientInfoResponse.error;
                    
                    }

            }else{
                
                //On success save profile info

                //Setup user profile
                Profile_Name.text=getPatientInfoResponse.Data.name;
                Profile_Email.text=getPatientInfoResponse.Data.email;
                Profile_Age.text=getPatientInfoResponse.Data.age;

                //Instansiate a Doctor prefab for each Doctor assigned to this patient

                //Get doctors assigned to this doctor
                var getPatientDoctorsResponse = await Api.BaseMethods.Unity.GetPatientDoctors();
                if(!getPatientDoctorsResponse.IsSuccess)
                {
                    //Response code forbidden means invalid auth token so return to login
                    if (getPatientDoctorsResponse.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    Profile_Loading.SetActive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {
                    //If its any other error just display it and continue no need to exit
                    Profile_ApiResponse.text=getPatientInfoResponse.error;
                    
                    }

                }else
                {
                    //On success save doctor details
                    foreach(var userprofile in getPatientDoctorsResponse.Data){

                       GameObject doctorPrefab = Instantiate(Profile_DoctorNamePrefab,Profile_DoctorNamePrefabParent.transform);
                        //Add information to doctor prefab
                       DoctorNamePrefab doctorPrefabScript = doctorPrefab.GetComponent<DoctorNamePrefab>();
                       
                       doctorPrefabScript.email.text = userprofile.email;
                       doctorPrefabScript.name.text = userprofile.name;
                       doctorPrefabScript.age.text = userprofile.age;

                    }

                }

            }

            //Stop loading profile
            Profile_Loading.SetActive(false);
        }
        public async void ResetPatientExercises(){
            // Clear the existing patient exercises list
            patientExercises.Clear();
            patientExercisesDetails.Clear();

            // Reset the UI elements related to patient exercises
            PatientExercises_Book.bookPages.Clear();
            PatientExercises_Book.currentPage = 0;
            PatientExercises_Book.UpdateSprites();
            PatientExercises_Slider.value = 0;
            PatientExercises_SliderCurrentPageText.text = "0/0";
            PatientExercises_ApiResponse.text = string.Empty;
            PatientExercises_CanvasGroup.interactable=false;

            // Call the SetupPatientExercises method to repopulate the list
            SetupPatientExercises();
        }
        public async void SetupPatientExercises()
        {

                //Begin by adding the cover images

                //Front Cover
                PatientExercises_Book.bookPages.Add(PatientExercises_PageCoverSprite);
                PatientExercises_Book.UpdateSprites();

                PatientExercises_Loading.SetActive(true);

                var getPatientExercises = await Api.BaseMethods.Unity.GetPatientExercises();
                if(!getPatientExercises.IsSuccess)
                {
                    //Response code forbidden means invalid auth token so return to login
                    if (getPatientExercises.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    Profile_Loading.SetActive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {

                    //If its any other error just display it and exit
                    PatientExercises_ApiResponse.text=getPatientExercises.error;
                    PatientExercises_Loading.SetActive(false);
                    return;
                    
                    }

                }
                //Save Patient Exercises
                patientExercises=getPatientExercises.Data;

                //Get each exercise's information and save it

                foreach (var patientExercise in patientExercises)
                {
                    
                    var getExerciseInformation = await Api.BaseMethods.Unity.GetExerciseInformation(patientExercise.exerciseid);
                    if(!getExerciseInformation.IsSuccess)
                    {
                        //Response code forbidden means invalid auth token so return to login
                        if (getExerciseInformation.HttpCode == (int)HttpStatusCode.Forbidden)
                        {
                        Profile_Loading.SetActive(false);
                        LogoutAndGoBacktoLoginScreen();
                        return;


                        }else
                        {

                        //If its any other error just display it and exit
                        PatientExercises_ApiResponse.text=getExerciseInformation.error;
                        PatientExercises_Loading.SetActive(false);
                        return;
                        
                        }

                    }

                    //Add exercise information to list
                    patientExercisesDetails.Add(getExerciseInformation.Data);

                }


                //Determine number of pages to add. If it's odd then add one extra page to ensure it's even
                int numberOfPagesToAdd = patientExercises.Count%2==0?patientExercises.Count:patientExercises.Count+1;

                //Add pages
                for (int i = 0; i < numberOfPagesToAdd; i++)
                {
                    PatientExercises_Book.bookPages.Add(Book_PageSprite);
                    
                }

                PatientExercises_MaxPageCount=numberOfPagesToAdd.ToString();

                //Back Cover
                PatientExercises_Book.bookPages.Add(PatientExercises_PageCoverSprite);

                //Setup Slider
                PatientExercises_SliderCurrentPageText.text=PatientExercises_Book.currentPage.ToString() + "/" + (numberOfPagesToAdd+2).ToString();
                
                PatientExercises_Slider.maxValue=numberOfPagesToAdd+2;
                PatientExercises_Slider.minValue=0;


                PatientExercises_Loading.SetActive(false);
                PatientExercises_CanvasGroup.interactable=true;
                PatientExercises_Book.UpdateSprites();
        }

        public async void ResetGeneralExercises(){
            // Clear the existing patient exercises list
            generalExercises.Clear();

            // Reset the UI elements related to patient exercises
            GeneralExercises_Book.bookPages.Clear();
            GeneralExercises_Book.currentPage = 0;
            GeneralExercises_Book.UpdateSprites();
            GeneralExercises_Slider.value = 0;
            GeneralExercises_SliderCurrentPageText.text = "0/0";
            GeneralExercises_ApiResponse.text = string.Empty;
            GeneralExercises_CanvasGroup.interactable=false;

            // Call the SetupPatientExercises method to repopulate the list
            SetupGeneralExercises();
        }

        public async void SetupGeneralExercises()
        {

                //Begin by adding the cover images

                //Front Cover
                GeneralExercises_Book.bookPages.Add(GeneralExercises_PageCoverSprite);
                GeneralExercises_Book.UpdateSprites();


                GeneralExercises_Loading.SetActive(true);

                var getGeneralExercises = await Api.BaseMethods.Unity.GetGeneralExercises();
                if(!getGeneralExercises.IsSuccess)
                {
                    //Response code forbidden means invalid auth token so return to login
                    if (getGeneralExercises.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    GeneralExercises_Loading.SetActive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {

                    //If its any other error just display it and exit
                    GeneralExercises_ApiResponse.text=getGeneralExercises.error;
                    GeneralExercises_Loading.SetActive(false);
                    return;
                    
                    }

                }
                //Save Patient Exercises
                generalExercises=getGeneralExercises.Data;



                //Determine number of pages to add. If it's odd then add one extra page to ensure it's even
                int numberOfPagesToAdd = generalExercises.Count%2==0?generalExercises.Count:generalExercises.Count+1;

                //Add pages
                for (int i = 0; i < numberOfPagesToAdd; i++)
                {
                    GeneralExercises_Book.bookPages.Add(Book_PageSprite);
                    
                }


                //Back Cover
                GeneralExercises_Book.bookPages.Add(GeneralExercises_PageCoverSprite);

                //Setup Slider
                GeneralExercises_MaxPageCount=numberOfPagesToAdd.ToString();
                GeneralExercises_SliderCurrentPageText.text=GeneralExercises_Book.currentPage.ToString() + "/" + (numberOfPagesToAdd+2).ToString();
                GeneralExercises_Slider.maxValue=numberOfPagesToAdd+2;
                GeneralExercises_Slider.minValue=0;


                GeneralExercises_Loading.SetActive(false);
                GeneralExercises_CanvasGroup.interactable=true;
                GeneralExercises_Book.UpdateSprites();

        }

    public async void GoToExercise(List<String> sentences, string exerciseId, bool isGeneralExercise){

            await FadeInCameraAsync(1f);

            //Move to exercise location
            resetXrOriginScript.GoToExercisePoint();

            //Hide Desk Teacher and Show Board Teacher
            TeacherAtDesk.SetActive(false);
            TeacherAtBoard.SetActive(true);
            
            karaokeScript.SetupExercise(sentences,exerciseId,isGeneralExercise);

            await FadeOutCameraAsync(1f);

            


    }

        public async void LeaveExercise(){

            await FadeInCameraAsync(1f);
            

            //Move to desk location
            resetXrOriginScript.Recenter();

            //Show Desk Teacher and Hide Board Teacher
            TeacherAtDesk.SetActive(true);
            TeacherAtBoard.SetActive(false);

            karaokeScript.parentCanvas.SetActive(false);
            
            await FadeOutCameraAsync(1f);
    }
    public async Task FadeInCameraAsync(float duration)
    {
        if (material == null)
        {
            Debug.LogError("Material not found!");
            return;
        }

        await FadeMaterialAsync(0, 1, duration);
        SetMaterialMode(false); // Set to Opaque after fade-in
    }

    public async Task FadeOutCameraAsync(float duration)
    {
        if (material == null)
        {
            Debug.LogError("Material not found!");
            return;
        }

        await FadeMaterialAsync(1, 0, duration);
        blackCameraFadeCube.gameObject.SetActive(false); // Deactivate after fade-out
    }

    private async Task FadeMaterialAsync(float startAlpha, float endAlpha, float duration)
    {
        SetMaterialMode(true); // Set to Transparent before fading
        blackCameraFadeCube.gameObject.SetActive(true);

        float elapsedTime = 0;
        Color color = material.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            material.color = color;
            await Task.Yield(); // Yield control back to Unity’s main loop
        }

        // Ensure final alpha value
        color.a = endAlpha;
        material.color = color;
    }

    private void SetMaterialMode(bool isTransparent)
    {
        if (isTransparent)
        {
            material.SetFloat("_Surface", 1);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
        }
        else
        {
            material.SetFloat("_Surface", 0);
            material.SetOverrideTag("RenderType", "Opaque");
            material.renderQueue = (int)RenderQueue.Geometry;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
        }
    }

        void LogoutAndGoBacktoLoginScreen(){

            //Delete Player Prefs
            Constants.PlayerPrefsManager.DeleteKey("authorization");
            Constants.PlayerPrefsManager.DeleteKey("userid");

            //Reload Scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            
        }




        private void ShowHideUI(CanvasGroup canvasGroup, bool isActive)
    {

            canvasGroup.DOFade(isActive ? 1f : 0f, 0.5f); 
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;
        
    }


    }



