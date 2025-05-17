using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace reCAPTCHAv2.Services
{
    public interface IRecaptchaService
    {
        Task<bool> ValidateRecaptchaAsync(string recaptchaResponse);
        Task<(bool success, string errorMessage)> ValidateRecaptchaWithDetailsAsync(string recaptchaResponse);
        bool TestMode { get; set; }
        bool TestModeResult { get; set; }
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecaptchaService>? _logger;
        private readonly string _secretKey;

        // Test mode properties
        public bool TestMode { get; set; } = false;
        public bool TestModeResult { get; set; } = true;

        public RecaptchaService(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<RecaptchaService>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;

            _secretKey = _configuration["RecaptchaSettings:SecretKey"]
                ?? throw new InvalidOperationException("reCAPTCHA Secret Key is not configured");
        }

        public async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            // Return test result if in test mode
            if (TestMode)
            {
                _logger?.LogInformation("reCAPTCHA validation in test mode, returning {result}", TestModeResult);
                return TestModeResult;
            }

            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                _logger?.LogWarning("reCAPTCHA response was empty");
                return false;
            }

            try
            {
                var values = new Dictionary<string, string>
                {
                    {"secret", _secretKey},
                    {"response", recaptchaResponse}
                };

                var content = new FormUrlEncodedContent(values);
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var recaptchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(responseString, options);

                var result = recaptchaResult?.Success ?? false;

                if (!result)
                    _logger?.LogWarning("reCAPTCHA validation failed: {ErrorCodes}",
                        string.Join(", ", (IEnumerable)recaptchaResult?.ErrorCodes! ?? Array.Empty<string>()));
                else
                    _logger?.LogInformation("reCAPTCHA validation successful");

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP error occurred during reCAPTCHA validation");
                return false;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON parsing error during reCAPTCHA validation");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error occurred during reCAPTCHA validation");
                return false;
            }
        }

        public async Task<(bool success, string errorMessage)> ValidateRecaptchaWithDetailsAsync(string recaptchaResponse)
        {
            // Return test result if in test mode
            if (TestMode)
            {
                return (TestModeResult, TestModeResult ? string.Empty : "Test mode validation failed");
            }

            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                _logger?.LogWarning("reCAPTCHA response was empty");
                return (false, "reCAPTCHA response was not provided");
            }

            try
            {
                var values = new Dictionary<string, string>
                {
                    {"secret", _secretKey},
                    {"response", recaptchaResponse}
                };

                var content = new FormUrlEncodedContent(values);
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("Error connecting to Google server: {StatusCode}", response.StatusCode);
                    return (false, $"Error connecting to Google server: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var recaptchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(responseString, options);

                if (recaptchaResult?.Success != true)
                {
                    string errorDetail = recaptchaResult?.ErrorCodes != null && recaptchaResult.ErrorCodes.Any()
                        ? string.Join(", ", recaptchaResult.ErrorCodes)
                        : "unknown reason";

                    _logger?.LogWarning("reCAPTCHA validation failed: {ErrorDetail}", errorDetail);
                    return (false, $"reCAPTCHA validation failed: {errorDetail}");
                }

                _logger?.LogInformation("reCAPTCHA validation successful");
                return (true, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP error during reCAPTCHA validation");
                return (false, $"Error connecting to reCAPTCHA service: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON parsing error during reCAPTCHA validation");
                return (false, "Invalid response format received from reCAPTCHA service");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during reCAPTCHA validation");
                return (false, $"Error during reCAPTCHA validation: {ex.Message}");
            }
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public string? Challenge_ts { get; set; } // Validation timestamp
            public string? Hostname { get; set; } // Website domain name
            public List<string>? ErrorCodes { get; set; } // Error codes
        }
    }
}