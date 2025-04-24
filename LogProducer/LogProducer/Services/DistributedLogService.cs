using System.Net.Http.Headers;
using System.Text.Json;
using LogProducer.Data;
using LogProducer.Services.Interfaces;
using Shared.Models;

namespace LogProducer.Services;

public class DistributedLogService: IDistributedLogService
{
    private string _authToken;
    private readonly string _username;
    private readonly string _password;
    private readonly HttpClient _httpClient;
    private readonly LogProducerDbContext _context;
    private static string _authUrl = "http://localhost:5039/api/auth/login";
    private static string _logUrl = "http://localhost:5039/api/log";

    public DistributedLogService(HttpClient httpClient, LogProducerDbContext context, IConfiguration config)
    {

        _httpClient = httpClient;
        _context = context;
        _username = config["DistributedLogUser:UserName"];
        _password = config["DistributedLogUser:Password"];

        if (_username == null || _password == null)
        {
            throw new Exception("User secrets must contain DistributedLogUser username and password");
        }
    }

    public async Task Authenticate()
    {
        var response = await _httpClient.PostAsJsonAsync(_authUrl, new
        {
            username = _username,
            password = _password
        });

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            // Assuming the authentication endpoint returns a JSON object with a "token" property
            using var document = JsonDocument.Parse(responseContent);
            if (document.RootElement.TryGetProperty("token", out var tokenElement))
            {
                if (tokenElement.TryGetProperty("result", out var resultProperty))
                {
                    _authToken = resultProperty.GetString() ?? "";
                }
            }
            else
            {
                Console.WriteLine("Authentication response did not contain a 'token' property.");
            }
        }
    }

    private async Task<HttpResponseMessage> MakeRemoteLogRequest(string action, List<LogMessage>? messages, LogSearchFilter? filter, int retryCount = 0)
    {
        var url = _logUrl + (string.IsNullOrEmpty(action) ? "" : "/" + action);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var response = await _httpClient
            .PostAsJsonAsync(url, messages ?? (object)filter);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retryCount < 1)
        {
            await Authenticate();
            return await MakeRemoteLogRequest(action, messages, filter, retryCount + 1);
        }

        return response;
    }
    
    public async Task<LogMessage?> LogAsync(LogMessage message)
    {
        var messages = new List<LogMessage> { message };

        var messagesResponse = await LogManyAsync(messages);
        return messagesResponse?[0];
    }

    public async Task<List<LogMessage>?> LogManyAsync(List<LogMessage> messages)
    {
        const string action = "Batch";
        try
        {
            var response = await MakeRemoteLogRequest(action, messages, null, 0);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var logMessages = JsonSerializer.Deserialize<List<LogMessage>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Optional: To handle different casing in JSON
                });
                return logMessages;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Error making HTTP request to remote server: {0}", e);
            await _context.TempLogMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();
        }
        catch (JsonException e)
        {
            Console.WriteLine("Error deserializing json from response : {0}", e);
        }
        return null;
    }

    public async Task<List<LogMessage>?> SearchLogsAsync(LogSearchFilter filter)
    {
        const string action = "Search";
        List<LogMessage> logs = [];
        try
        {
            logs = await _context.SearchLogsAsync(filter);
            var response = await MakeRemoteLogRequest(action, null, filter);
            
            if (!response.IsSuccessStatusCode) return logs;
            
            var jsonString = await response.Content.ReadAsStringAsync();
            var logMessages = JsonSerializer.Deserialize<List<LogMessage>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
                
            return logs.Concat(logMessages ?? []).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return logs;
        }
    }

    public async Task DeleteLogsAsync(LogSearchFilter filter)
    {
        const string action = "Delete";
        
        try
        {
            await MakeRemoteLogRequest(action, null, filter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<LogMessage> DebugAsync(LogMessage message)
    {
        message.Level = "DEBUG";
        return await LogAsync(message);
    }

    public async Task<LogMessage> InfoAsync(LogMessage message)
    {
        message.Level = "INFO";
        return await LogAsync(message);
    }

    public async Task<LogMessage> WarnAsync(LogMessage message)
    {
        message.Level = "WARNING";
        return await LogAsync(message);
    }

    public async Task<LogMessage> ErrorAsync(LogMessage message)
    {
        message.Level = "ERROR";
        return await LogAsync(message);
    }

    public async Task<LogMessage> FatalAsync(LogMessage message)
    {
        message.Level = "FATAL";
        return await LogAsync(message);
    }
}