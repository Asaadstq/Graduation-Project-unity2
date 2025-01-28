using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Api.BaseMethods
{
public class Unity
{
    private static readonly CookieContainer _cookieContainer = new CookieContainer();
    private static readonly HttpClientHandler _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
    private static readonly HttpClient _httpClient = new HttpClient(_handler);

    public Unity()
    {
        // Optionally configure the HttpClient; set up headers, base address, etc.
        
        _httpClient.BaseAddress = new Uri(Constants.Constants._baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public static async Task<GenericApiResponse<UserProfile>> GetPatientInfo()
    {
        var url = $"{Constants.Constants._baseUrl}/unity/getpatientinfo";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                UserProfile patientInfoResponse = JsonConvert.DeserializeObject<UserProfile>(jsonResponse);


                return GenericApiResponse<UserProfile>.Success(patientInfoResponse, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<UserProfile>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<UserProfile>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<GenericApiResponse<UserProfile>> GetPatientDoctors()
    {
        var url = $"{Constants.Constants._baseUrl}/unity/getpatientdoctors";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                UserProfile patientDoctorsResponse = JsonConvert.DeserializeObject<UserProfile>(jsonResponse);


                return GenericApiResponse<UserProfile>.Success(patientDoctorsResponse, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<UserProfile>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<UserProfile>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }


    public static async Task<GenericApiResponse<PatientExercise>> GetPatientExercises()
    {
        var url = $"{Constants.Constants._baseUrl}/unity/getpatientexercises";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                PatientExercise patientExerciseResponse = JsonConvert.DeserializeObject<PatientExercise>(jsonResponse);


                return GenericApiResponse<PatientExercise>.Success(patientExerciseResponse, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<PatientExercise>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<PatientExercise>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }


        public static async Task<GenericApiResponse<Exercise>> GetExerciseInformation(string exerciseId)
    {
        var url = $"{Constants.Constants._baseUrl}/unity/getexerciseinformation/{exerciseId}";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Exercise exerciseInformationResponse = JsonConvert.DeserializeObject<Exercise>(jsonResponse);


                return GenericApiResponse<Exercise>.Success(exerciseInformationResponse, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<Exercise>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<Exercise>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }

        public static async Task<GenericApiResponse<Exercise>> GetGeneralExercises()
    {
        var url = $"{Constants.Constants._baseUrl}/unity/getgeneralexercises";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Exercise getGeneralExerciseResponse = JsonConvert.DeserializeObject<Exercise>(jsonResponse);


                return GenericApiResponse<Exercise>.Success(getGeneralExerciseResponse, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<Exercise>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<Exercise>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }

        public static async Task<GenericApiResponse<GenericMessageResponse>> SaveExerciseInformation(string exerciseId,string score,string stutteramount,string timetaken,string resetamount)
    {
        var url = $"{Constants.Constants._baseUrl}/unity/saveexerciseinformation/{exerciseId}";

        // Ensure cookies are attached
        Uri baseUri = new Uri(Constants.Constants._baseUrl);
        _handler.CookieContainer.Add(baseUri, new Cookie("authorization", Constants.Constants.AUTHKEY));
        _handler.CookieContainer.Add(baseUri, new Cookie("userid", Constants.Constants.USERID));

        try
        {
            var saveExerciseInformationRequest = new SaveExerciseInformationRequest{score=score,stutteramount=stutteramount,timetaken=timetaken,resetamount=resetamount};

            var jsonRequest = JsonConvert.SerializeObject(saveExerciseInformationRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url,content);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericMessageResponse saveExerciseInformation = JsonConvert.DeserializeObject<GenericMessageResponse>(jsonResponse);


                return GenericApiResponse<GenericMessageResponse>.Success(saveExerciseInformation, (int)response.StatusCode);
            }
            else
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericErrorResponse genericErrorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                return GenericApiResponse<GenericMessageResponse>.Failure(genericErrorResponse.error, (int)response.StatusCode);
            }
        }
        catch (Exception)
        {
            return GenericApiResponse<GenericMessageResponse>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
        }
    }


}
}