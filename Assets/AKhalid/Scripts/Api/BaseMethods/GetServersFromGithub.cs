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
    public class GetServersFromGithub
    {
        private static readonly CookieContainer _cookieContainer = new CookieContainer();
        private static readonly HttpClientHandler _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        private static readonly HttpClient _httpClient = new HttpClient(_handler);


        static GetServersFromGithub()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public static async Task<GenericApiResponse<GetServersResponse>> GetServersFromGithubAsync()
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(Constants.Constants._github_servers_json_link);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize response
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    GetServersResponse getServers = JsonConvert.DeserializeObject<GetServersResponse>(jsonResponse);


                    return GenericApiResponse<GetServersResponse>.Success(getServers, (int)response.StatusCode);
                }
                else
                {
                return GenericApiResponse<GetServersResponse>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
                return GenericApiResponse<GetServersResponse>.Failure("Error communicating with server", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
