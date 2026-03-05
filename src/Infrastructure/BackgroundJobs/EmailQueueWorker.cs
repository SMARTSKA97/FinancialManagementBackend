using Application.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class EmailQueueWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<EmailQueueWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private const string QueueName = "MailQueue";

    public EmailQueueWorker(IConnectionMultiplexer redis, ILogger<EmailQueueWorker> logger, IConfiguration configuration)
    {
        _redis = redis;
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailQueueWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var db = _redis.GetDatabase();
                // Use LPop to check for messages. If nil, wait.
                // Using a simple polling mechanism with delay to avoid blocking the connection excessively if not supported.
                var message = await db.ListLeftPopAsync(QueueName);

                if (message.HasValue)
                {
                    await ProcessEmailAsync(message.ToString());
                }
                else
                {
                    await Task.Delay(1000, stoppingToken); // Poll every second
                }
            }
            catch (OperationCanceledException)
            {
                // Task was canceled due to application shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email queue.");
                // Prevent infinite tight loop on persistent error, using Safe Task.Delay
                try { await Task.Delay(5000, stoppingToken); } catch (OperationCanceledException) { break; }
            }
        }
        
        _logger.LogInformation("EmailQueueWorker stopping.");
    }

    private async Task ProcessEmailAsync(string message)
    {
        try
        {
            var mailRequest = JsonSerializer.Deserialize<MailRequest>(message);
            if (mailRequest == null) return;

            // 1. Always log the body in development so the developer can see the Magic Token!
            _logger.LogInformation("Email to {To}: {Subject}\n--- BODY ---\n{Body}\n--- END BODY ---", 
                mailRequest.To, mailRequest.Subject, mailRequest.Body);
            
            // 2. Try to use Brevo API if configured 
            var apiKey = _configuration["Brevo:ApiKey"];
            
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_BREVO_API_KEY")
            {
                var senderName = _configuration["Brevo:SenderName"] ?? "Financial Planner App";
                var senderEmail = _configuration["Brevo:SenderEmail"] ?? "no-reply@localhost";

                var brevoPayload = new
                {
                    sender = new { name = senderName, email = senderEmail },
                    to = new[] { new { email = mailRequest.To } },
                    subject = mailRequest.Subject,
                    htmlContent = mailRequest.Body // Upgraded to HTML content
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(brevoPayload), Encoding.UTF8, "application/json");
                
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Add("api-key", apiKey);
                requestMessage.Content = requestContent;

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Actual email sent successfully to {To} via Brevo API.", mailRequest.To);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Brevo API failed. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorResponse);
                }
            }
            else
            {
                // Artificial delay to simulate email if no config provided
                await Task.Delay(500); 
                _logger.LogWarning("No valid Brevo API key found. Email was simulated and printed to logs only.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email.");
        }
    }
}
