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
public class Validation
{
    private static readonly CookieContainer _cookieContainer = new CookieContainer();
    private static readonly HttpClientHandler _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
    private static readonly HttpClient _httpClient = new HttpClient(_handler);

    public Validation()
    {
        // Optionally configure the HttpClient; set up headers, base address, etc.
        
        _httpClient.BaseAddress = new Uri(Constants.Constants._baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public static async Task<GenericApiResponse<GenericMessageResponse>> Login(string email, string password)
    {
        var url = $"{Constants.Constants._baseUrl}/login";

        var LoginRequest = new
        {
            email = email,
            password = password,
        };

        var jsonRequest = JsonConvert.SerializeObject(LoginRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericMessageResponse loginResponse = JsonConvert.DeserializeObject<GenericMessageResponse>(jsonResponse);

                // Retrieve cookies
                Uri baseUri = new Uri(Constants.Constants._baseUrl);
                CookieCollection cookies = _cookieContainer.GetCookies(baseUri);

                string authCookie = cookies["authorization"]?.Value;
                string userIdCookie = cookies["userid"]?.Value;

                Constants.Constants.AUTHKEY=authCookie;
                Constants.Constants.USERID=userIdCookie;
                Constants.PlayerPrefsManager.SaveString("authorization",authCookie);
                Constants.PlayerPrefsManager.SaveString("userid",userIdCookie);

                return GenericApiResponse<GenericMessageResponse>.Success(loginResponse, (int)response.StatusCode);
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

    public static async Task<GenericApiResponse<GenericMessageResponse>> Signup(string email, string password, string name, string age, string type="Patient")
    {
        var url = $"{Constants.Constants._baseUrl}/signup";

        var signUpRequest = new SignUpRequest{email=email,password=password,name=name,age=age,type=type};

        var jsonRequest = JsonConvert.SerializeObject(signUpRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                GenericMessageResponse loginResponse = JsonConvert.DeserializeObject<GenericMessageResponse>(jsonResponse);

                // Retrieve cookies
                Uri baseUri = new Uri(Constants.Constants._baseUrl);
                CookieCollection cookies = _cookieContainer.GetCookies(baseUri);

                string authCookie = cookies["authorization"]?.Value;
                string userIdCookie = cookies["userid"]?.Value;

                Constants.Constants.AUTHKEY=authCookie;
                Constants.Constants.USERID=userIdCookie;
                Constants.PlayerPrefsManager.SaveString("authorization",authCookie);
                Constants.PlayerPrefsManager.SaveString("userid",userIdCookie);

                
                return GenericApiResponse<GenericMessageResponse>.Success(loginResponse, (int)response.StatusCode);
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