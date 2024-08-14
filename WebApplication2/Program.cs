using Polly.Extensions.Http;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http.Resilience;
using Polly.Fallback;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



 
builder.Services.AddHttpClient("MyHttpClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7061");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddResilienceHandler("CustomPipeline", builder =>
{
    builder.AddRetry(new HttpRetryStrategyOptions
    {

        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),

    }).AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(40),
        FailureRatio = 0.5, // 50% failure ratio means 2 out of 4 requests need to fail
        MinimumThroughput = 4, // Minimum number of requests to evaluate
        BreakDuration = TimeSpan.FromSeconds(30), // Time to keep the circuit open
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode == HttpStatusCode.RequestTimeout ||
            args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable ||
            args.Outcome.Exception != null ?
            args.Outcome.Exception.Message.Contains("No connection could be made because the target machine actively refused") : args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable)
    }).AddTimeout(new HttpTimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10), // Set the timeout to 10 seconds
    });
});



builder.Services.AddHttpClient("MyHttpClient2", client =>
{
    client.BaseAddress = new Uri("https://localhost:7032");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddResilienceHandler("CustomPipeline", builder =>
{
    builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(120),
        FailureRatio = 0.5, // 50% failure ratio means 2 out of 4 requests need to fail
        MinimumThroughput = 12, // Minimum number of requests to evaluate
        BreakDuration = TimeSpan.FromSeconds(30), // Time to keep the circuit open
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode == HttpStatusCode.RequestTimeout ||
            args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable ||
            args.Outcome.Exception != null ?
            args.Outcome.Exception.Message.Contains("No connection could be made because the target machine actively refused") : args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable)
    }).AddRetry(new HttpRetryStrategyOptions
    {

        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode == HttpStatusCode.RequestTimeout ||
            args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable ||
            args.Outcome.Exception != null ?
            args.Outcome.Exception.Message.Contains("No connection could be made because the target machine actively refused") : args.Outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable)
    }).AddTimeout(new HttpTimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10), // Set the timeout to 10 seconds
    }); ;
});




//builder.Services.AddHttpClient("MyHttpClient", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7061");
//    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//}).AddResilienceHandler("myHandler", b =>
//{
//    b.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>()
//    {
//        FallbackAction = _ => Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
//    })
//    .AddConcurrencyLimiter(100)
//    .AddRetry(new HttpRetryStrategyOptions())
//    .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions())
//    .AddTimeout(new HttpTimeoutStrategyOptions());
//});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
