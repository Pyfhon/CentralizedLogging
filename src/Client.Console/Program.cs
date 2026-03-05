using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddNLog();
var serviceAUrl = builder.Configuration["SERVICEA_URL"] ?? "http://localhost:5001";
builder.Services.AddHttpClient("serviceA", client => client.BaseAddress = new Uri(serviceAUrl));

using var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Client.Console");
var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();

var jobId = Guid.NewGuid().ToString();
using var jobScope = MappedDiagnosticsLogicalContext.SetScoped("jobId", jobId);
using var activity = new ActivitySource("Client.Console").StartActivity("client-call", ActivityKind.Client);

logger.LogInformation("Starting request chain with JobId={JobId}", jobId);

var client = httpClientFactory.CreateClient("serviceA");
client.DefaultRequestHeaders.Add("X-Job-Id", jobId);

logger.LogInformation("Calling ServiceA /do");
var response = await client.GetAsync("/do");
var body = await response.Content.ReadAsStringAsync();

logger.LogInformation("ServiceA response: Status={StatusCode} Body={Body}", (int)response.StatusCode, body);

LogManager.Shutdown();

