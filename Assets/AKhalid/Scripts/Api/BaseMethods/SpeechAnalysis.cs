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
    public class SpeechAnalysis
    {
        private static readonly CookieContainer _cookieContainer = new CookieContainer();
        private static readonly HttpClientHandler _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        private static readonly HttpClient _httpClient = new HttpClient(_handler);

        private static readonly string apiUrl = "https://vz82ssc6-8000.euw.devtunnels.ms/analyze_speech/";

        static SpeechAnalysis()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public static async Task<GenericApiResponse<SpeechAnalysisResult>> ValidateSentence(byte[] wavData, string sentence)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Attach WAV file
                    ByteArrayContent audioContent = new ByteArrayContent(wavData);
                    audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                    formData.Add(audioContent, "file", "recording.wav");

                    // Attach sentence text
                    StringContent textContent = new StringContent(sentence, Encoding.UTF8, "text/plain");
                    formData.Add(textContent, "target_text");

                    // Send POST request
                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        SpeechAnalysisResult result = JsonConvert.DeserializeObject<SpeechAnalysisResult>(jsonResponse);
                        return GenericApiResponse<SpeechAnalysisResult>.Success(result, (int)response.StatusCode);
                    }
                    else
                    {
                        GenericErrorResponse errorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(jsonResponse);
                        return GenericApiResponse<SpeechAnalysisResult>.Failure(errorResponse.error, (int)response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return GenericApiResponse<SpeechAnalysisResult>.Failure($"Error: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
