using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

app.MapGet("/work", async (HttpContext context, ILogger<Program> logger) =>
{
    var headerJobId = context.Request.Headers["X-Job-Id"].FirstOrDefault();
    var jobId = string.IsNullOrWhiteSpace(headerJobId) ? Guid.NewGuid().ToString() : headerJobId;

    using var jobScope = MappedDiagnosticsLogicalContext.SetScoped("jobId", jobId);

    logger.LogInformation("ServiceB start processing /work");
    var delayMs = Random.Shared.Next(200, 501);
    logger.LogInformation("ServiceB simulating work for {DelayMs}ms", delayMs);
    await Task.Delay(delayMs, context.RequestAborted);
    logger.LogInformation("ServiceB finish processing /work");

    return Results.Json(new
    {
        service = "ServiceB",
        jobId,
        processedAtUtc = DateTime.UtcNow,
        delayMs
    });
});

app.Run();
