using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseNLog();
var serviceBUrl = builder.Configuration["SERVICEB_URL"] ?? "http://localhost:5002";
builder.Services.AddHttpClient("serviceB", client => client.BaseAddress = new Uri(serviceBUrl));

var app = builder.Build();

app.MapGet("/do", async (HttpContext context, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    var headerJobId = context.Request.Headers["X-Job-Id"].FirstOrDefault();
    var jobId = string.IsNullOrWhiteSpace(headerJobId) ? Guid.NewGuid().ToString() : headerJobId;

    using var jobScope = MappedDiagnosticsLogicalContext.SetScoped("jobId", jobId);

    logger.LogInformation("ServiceA start processing /do");

    var client = httpClientFactory.CreateClient("serviceB");
    client.DefaultRequestHeaders.Remove("X-Job-Id");
    client.DefaultRequestHeaders.Add("X-Job-Id", jobId);

    logger.LogInformation("ServiceA calling ServiceB /work");
    var serviceBResponse = await client.GetAsync("/work", context.RequestAborted);
    var serviceBPayload = await serviceBResponse.Content.ReadAsStringAsync(context.RequestAborted);

    logger.LogInformation("ServiceA got ServiceB response Status={StatusCode}", (int)serviceBResponse.StatusCode);
    logger.LogInformation("ServiceA finish processing /do");

    return Results.Json(new
    {
        service = "ServiceA",
        jobId,
        serviceB = serviceBPayload
    });
});

app.Run();
