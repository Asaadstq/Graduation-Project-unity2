using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class Main : MonoBehaviour
    {
        [Header("Login UI")]
        public CanvasGroup Login_UI;
        public TMP_InputField Login_Email;
        public TMP_InputField Login_Password;
        public Button Login_LoginButton;
        public GameObject Login_Loading
        public TMP_Text Login_ApiResponse;

        [Header("Signup UI")]
        public CanvasGroup Signup_UI;
        public TMP_InputField Signup_Name;
        public TMP_InputField Signup_Email;
        public TMP_InputField Signup_Password;
        public TMP_InputField Signup_Age;
        public Button Signup_SignupBUtton;
        public GameObject Signup_Loading
        public TMP_Text Signup_ApiResponse;

        [Header("Profile UI")]
        public CanvasGroup Profile_UI;
        public TMP_Text Profile_Name;
        public Button Profile_LogoutButton;
        public TMP_Text Profile_Email;
        public TMP_Text Profile_Age;
        public GameObject Profile_Loading
        public TMP_Text Profile_ApiResponse;
        public GameObject Profile_DoctorNamePrefab;
        public GameObject Profile_DoctorNamePrefabParent;

        [Header("Book")]
        public Book PatientExercises_Book;
        public AutoFlip PatientExercises_AutoFlipScript;
        public Book GeneralExercises_Book;
        public AutoFlip GeneralExercises_AutoFlipScript;
        public Sprite Book_PageSprite;


        [Header("Patient Exercises")]
        public CanvasGroup PatientExercises_UI;
        public Button PatientExercises_NextPage;
        public Button PatientExercises_PreviousPage;
        public Slider PatientExercises_Slider;
        public TMP_Text PatientExercises_SliderCurrentPageText;
        public TMP_Text PatientExercises_SliderMaxPageText;
        public GameObject PatientExercises_ExerciseLeftPageDetails;
        public GameObject PatientExercises_ExerciseRightPageDetails;
        public GameObject PatientExercises_Loading;
        public TMP_Text PatientExercises_ApiResponse;
        public CanvasGroup PatientExercises_ApiResponseCanvas;
        public Button PatientExercises_ApiResponseCanvasCloseButton;
        public Sprite PatientExercises_PageCoverSprite;

        [Header("General Exercises")]
        public CanvasGroup GeneralExercises_UI;
        public Button GeneralExercises_NextPage;
        public Button GeneralExercises_PreviousPage;
        public Slider GeneralExercises_Slider;
        public TMP_Text GeneralExercises_SliderCurrentPageText;
        public TMP_Text GeneralExercises_SliderMaxPageText;
        public GameObject GeneralExercises_ExerciseLeftPageDetails;
        public GameObject GeneralExercises_ExerciseRightPageDetails;
        public GameObject GeneralExercises_Loading;
        public TMP_Text GeneralExercises_ApiResponse;
        public CanvasGroup GeneralExercises_ApiResponseCanvas;
        public Button GeneralExercises_ApiResponseCanvasCloseButton;
        public Sprite GeneralExercises_PageCoverSprite;


        private List<PatientExercises> patientExercises=new List<PatientExercises>();
        private List<Exercises> patientExercisesDetails=new List<Exercises>();
        private List<Exercises> generalExercises=new List<Exercises>();

        async Task Start()
        {
            await Setup();

        }

        public async void Setup()
        {

            //Setup Sliders

            //Login UI
            Login_LoginButton.onClick.AddListener(Login);

            //Begin by hiding everything and displaying the login and canvas
            ShowHideUI(Login_UI,true);
            ShowHideUI(Signup_UI,false);
            ShowHideUI(Profile_UI,false);
            ShowHideUI(Exercises_UI,false);

            //Get auth token from player prefs  
            Constants.Constants.AUTHKEY = Constants.PlayerPrefsManager.GetString("authorization")
            Constants.Constants.USERID = Constants.PlayerPrefsManager.GetString("userid")

            //Start the LoginUI loading
            Login_Loading.setactive(true);

            //Call the getpatientinfo endpoint to validate the auth token, if it fails then force the user to login
            var getPatientInfoResponse = await Api.BaseMethods.Unity.GetPatientInfo();
            if(!getPatientInfoResponse.IsSuccess)
            {
                Login_Loading.setactive(false);
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

        public async void Login()
        {
            //Start the LoginUI loading
            Login_Loading.setactive(true);

            var loginResponse = await Api.BaseMethods.Validation.Login();
            if(!loginResponse.IsSuccess)
            {
                Login_Loading.setactive(false);
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
            Login_Loading.setactive(true);

            var signupResponse = await Api.BaseMethods.Validation.Signup();
            if(!signupResponse.IsSuccess)
            {
                Signup_Loading.setactive(false);
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
            Profile_Loading.setactive(true);

            //Get user profile
            var getPatientInfoResponse = await Api.BaseMethods.Unity.GetPatientInfo();
            if(!getPatientInfoResponse.IsSuccess)
            {
                    //Response code forbidden means invalid auth token so return to login
                    if (getPatientInfoResponse.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    Profile_Loading.setactive(false);
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
                    Profile_Loading.setactive(false);
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
                       
                       doctorPrefabScript.email = userprofile.email;
                       doctorPrefabScript.name = userprofile.name;
                       doctorPrefabScript.age = userprofile.age;

                    }

                }

            }

            //Stop loading profile
            Profile_Loading.setactive(false);
        }

        public async void SetupPatientExercises()
        {

                PatientExercises_Loading.setactive(true);

                var getPatientExercises = await Api.BaseMethods.Unity.GetPatientExercises();
                if(!getPatientExercises.IsSuccess)
                {
                    //Response code forbidden means invalid auth token so return to login
                    if (getPatientExercises.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    Profile_Loading.setactive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {

                    //If its any other error just display it and exit
                    PatientExercises_ApiResponse.text=getPatientExercises.error;
                    ShowHideUI(PatientExercises_ApiResponseCanvas,true);
                    PatientExercises_Loading.setactive(false);
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
                        Profile_Loading.setactive(false);
                        LogoutAndGoBacktoLoginScreen();
                        return;


                        }else
                        {

                        //If its any other error just display it and exit
                        PatientExercises_ApiResponse.text=getExerciseInformation.error;
                        ShowHideUI(PatientExercises_ApiResponseCanvas,true);
                        PatientExercises_Loading.setactive(false);
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

                //Setup Slider
                PatientExercises_SliderMaxPageText.text=numberOfPagesToAdd.tostring();
                PatientExercises_SliderCurrentPageText.text=currentPage.tostring();
                
                PatientExercises_Slider.maxValue=numberOfPagesToAdd+2;
                PatientExercises_Slider.minValue=0;


                PatientExercises_Loading.setactive(false);
        }

        public async void SetupGeneralExercises()
        {

                GeneralExercises_Loading.setactive(true);

                var getGeneralExercises = await Api.BaseMethods.Unity.GetGeneralExercises();
                if(!getPatientExercises.IsSuccess)
                {
                    //Response code forbidden means invalid auth token so return to login
                    if (getGeneralExercises.HttpCode == (int)HttpStatusCode.Forbidden)
                    {
                    GeneralExercises_Loading.setactive(false);
                    LogoutAndGoBacktoLoginScreen();
                    return;


                    }else
                    {

                    //If its any other error just display it and exit
                    GeneralExercises_ApiResponse.text=getGeneralExercises.error;
                    ShowHideUI(GeneralExercises_ApiResponseCanvas,true);
                    GeneralExercises_Loading.setactive(false);
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
                GeneralExercises_SliderMaxPageText.text=numberOfPagesToAdd.tostring();
                GeneralExercises_SliderCurrentPageText.text=currentPage.tostring();
                
                GeneralExercises_Slider.maxValue=numberOfPagesToAdd+2;
                GeneralExercises_Slider.minValue=0;


                GeneralExercises_Loading.setactive(false);
        }


        void LogoutAndGoBacktoLoginScreen(){

            //Delete Player Prefs
            Constants.PlayerPrefsManager.DeleteKey("authorization")
            Constants.PlayerPrefsManager.DeleteKey("userid")

            //Reload Scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            
        }




        private void ShowHideUI(CanvasGroup canvasGroup, bool isActive)
    {
        foreach (CanvasGroup element in elements)
        {
            element.DOFade(isActive ? 1f : 0f, 0.5f); 
            element.interactable = isActive;
            element.blocksRaycasts = isActive;
        }
    }


    }



