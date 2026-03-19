using Azure.Core;
using RecipesAPI.DTOs;
using System.Text;
using System.Text.Json.Serialization;

namespace RecipesAPI.Services
{
    public interface IAuthService
    {
        Task<bool> IsCaptchaValid(string captchaToken);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly string secretKey;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.secretKey = configuration["Captcha:SecretKey"];
        }

        public async Task<bool> IsCaptchaValid(string captchaToken)
        {
            string googleUrl = "https://www.google.com/recaptcha/api/siteverify";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", captchaToken)
            });

            var response = await httpClient.PostAsync(googleUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<RecaptchaVerificationResponse>();

                return jsonResponse != null && jsonResponse.Success;
            }

            return false;
        }

        public class RecaptchaVerificationResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }
}
