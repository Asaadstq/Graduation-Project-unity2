using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Models;
using DG.Tweening;
using PrefabSpecificScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    public class Main : MonoBehaviour
    {

        [Header("General Exercises")]
        public AudioClip pageFlipAudio;
        public AudioSource audiosource;
        

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
        public Button PatientExercises_NextPage;
        public Button PatientExercises_PreviousPage;
        public Slider PatientExercises_Slider;
        public TMP_Text PatientExercises_SliderCurrentPageText;
        public GameObject PatientExercises_ExerciseLeftPageDetails;
        public GameObject PatientExercises_ExerciseRightPageDetails;
        public GameObject PatientExercises_Loading;
        public TMP_Text PatientExercises_ApiResponse;
        public Sprite PatientExercises_PageCoverSprite;

        [Header("General Exercises")]
        public Button GeneralExercises_NextPage;
        public Button GeneralExercises_PreviousPage;
        public Slider GeneralExercises_Slider;
        public TMP_Text GeneralExercises_SliderCurrentPageText;
        public GameObject GeneralExercises_ExerciseLeftPageDetails;
        public GameObject GeneralExercises_ExerciseRightPageDetails;
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
            //SetupPatientExercises();
            //SetupGeneralExercises();

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
            
            PatientExercises_Book.currentPage = 6 - pagenumber;
            PatientExercises_Book.UpdateSprites();
            //PatientExercises_SliderCurrentPageText.text=  (int.Parse(PatientExercises_MaxPageCount) - PatientExercises_Book.currentPage ).ToString() + "/" + PatientExercises_MaxPageCount;

        }
        //if page number is odd
        else
        {

            PatientExercises_Book.currentPage = 6 - pagenumber + 1;
            PatientExercises_Book.UpdateSprites();
            //PatientExercises_SliderCurrentPageText.text=  (int.Parse(PatientExercises_MaxPageCount) - PatientExercises_Book.currentPage ).ToString() + "/" + PatientExercises_MaxPageCount;

        }


    }

    public void GoToNextOrPreviousPagePersonalExercise(bool adding)
    {
        
        if(adding)
        PatientExercises_Slider.value+=2;
        else
        PatientExercises_Slider.value-=2;

        if(PatientExercises_Slider.value==0 || PatientExercises_Slider.value==6 ){
            PatientExercises_ExerciseLeftPageDetails.SetActive(false);
            PatientExercises_ExerciseRightPageDetails.SetActive(false);
        }
        else{
            PatientExercises_ExerciseLeftPageDetails.SetActive(true);
            PatientExercises_ExerciseRightPageDetails.SetActive(true);
        }


        PlayPageFlipAudio();
    }

        public void ChangeGeneralExercisePage(int pagenumber)
    { 


        //If page is even
        if (pagenumber%2==0)
        {
            
            GeneralExercises_Book.currentPage = 6 - pagenumber;
            GeneralExercises_Book.UpdateSprites();
            //GeneralExercises_SliderCurrentPageText.text=  (int.Parse(GeneralExercises_MaxPageCount) - GeneralExercises_Book.currentPage ).ToString() + "/" + GeneralExercises_MaxPageCount;

        }
        //if page number is odd
        else
        {

            GeneralExercises_Book.currentPage = 6 - pagenumber + 1;
            GeneralExercises_Book.UpdateSprites();
            //GeneralExercises_SliderCurrentPageText.text=  (int.Parse(GeneralExercises_MaxPageCount) - GeneralExercises_Book.currentPage ).ToString() + "/" +GeneralExercises_MaxPageCount;

        }
    }


    public void GoToNextOrPreviousPageGeneralExercise(bool adding)
    {
        
        if(adding)
        GeneralExercises_Slider.value+=2;
        else
        GeneralExercises_Slider.value-=2;

        PlayPageFlipAudio();

        if(GeneralExercises_Slider.value==0 || GeneralExercises_Slider.value==6 ){
           GeneralExercises_ExerciseLeftPageDetails.SetActive(false);
            GeneralExercises_ExerciseRightPageDetails.SetActive(false);
        }
        else{
            GeneralExercises_ExerciseLeftPageDetails.SetActive(true);
            GeneralExercises_ExerciseRightPageDetails.SetActive(true);
        }
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

        public async void SetupPatientExercises()
        {

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


                //Begin by adding the cover images

                //Front Cover
                PatientExercises_Book.bookPages.Add(PatientExercises_PageCoverSprite);
                //Back Cover
                PatientExercises_Book.bookPages.Add(PatientExercises_PageCoverSprite);

                //Determine number of pages to add. If it's odd then add one extra page to ensure it's even
                int numberOfPagesToAdd = patientExercises.Count%2==0?patientExercises.Count:patientExercises.Count+1;

                //Add pages
                for (int i = 0; i < numberOfPagesToAdd; i++)
                {
                    PatientExercises_Book.bookPages.Add(Book_PageSprite);
                    
                }

                PatientExercises_MaxPageCount=numberOfPagesToAdd.ToString();

                //Setup Slider
                PatientExercises_SliderCurrentPageText.text=PatientExercises_Book.currentPage.ToString() + "/" + numberOfPagesToAdd.ToString();;
                
                PatientExercises_Slider.maxValue=numberOfPagesToAdd+2;
                PatientExercises_Slider.minValue=0;


                PatientExercises_Loading.SetActive(false);
        }

        public async void SetupGeneralExercises()
        {

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


                //Begin by adding the cover images

                //Front Cover
                GeneralExercises_Book.bookPages.Add(GeneralExercises_PageCoverSprite);
                //Back Cover
                GeneralExercises_Book.bookPages.Add(GeneralExercises_PageCoverSprite);

                //Determine number of pages to add. If it's odd then add one extra page to ensure it's even
                int numberOfPagesToAdd = generalExercises.Count%2==0?generalExercises.Count:generalExercises.Count+1;

                //Add pages
                for (int i = 0; i < numberOfPagesToAdd; i++)
                {
                    GeneralExercises_Book.bookPages.Add(Book_PageSprite);
                    
                }

                //Setup Slider
                GeneralExercises_MaxPageCount=numberOfPagesToAdd.ToString();
                GeneralExercises_SliderCurrentPageText.text=GeneralExercises_Book.currentPage.ToString() + "/" + numberOfPagesToAdd.ToString();
                GeneralExercises_Slider.maxValue=numberOfPagesToAdd+2;
                GeneralExercises_Slider.minValue=0;


                GeneralExercises_Loading.SetActive(false);
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



