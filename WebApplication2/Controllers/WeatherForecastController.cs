using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "A", "B", "C"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly HttpClient _client;
        private readonly HttpClient _client2;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,IHttpClientFactory httpClient, IHttpClientFactory httpClient2)
        {
              _client = httpClient.CreateClient("MyHttpClient");
            _client2 = httpClient2.CreateClient("MyHttpClient2");
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<string> GetAsync()
        {
            //************************************************************
            var services = new ServiceCollection();

            // Register the HttpClient with Polly policies
            //services.AddHttpClient("MyHttpClient", client =>
            //{
            //    client.BaseAddress = new Uri("https://localhost:7061/WeatherForecast");
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //});//.AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetCircuitBreakerPolicy()); // Add circuit breaker policy

            // Build the service provider
            // var serviceProvider = services.BuildServiceProvider();

            // Get the HttpClient instance from the service provider
            // var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            //var client = httpClientFactory.CreateClient("MyHttpClient");

            // Example request
            //try {
            var response = await _client.GetAsync("WeatherForecast");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
               // return content;
            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
               // return "Not!!!";
            }
            /*}
            catch (Exception ex) {
                var message = ex.Message; 
                Console.WriteLine(message);
            }*/
            try
            {
                var response2= await _client2.GetAsync("WeatherForecast");

            if (response2.IsSuccessStatusCode)
            {
                var content2 = await response2.Content.ReadAsStringAsync();
                Console.WriteLine(content2);
                return content2;
            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response2.StatusCode}");
                return "Not!!!";
            }
            }
            catch (Exception ex) {

                return "";
            }

            //**************************************************************

        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        // Circuit breaker policy: breaks the circuit for 30 seconds after 5 consecutive errors
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}