using System.Net.Http.Headers;
using System.Text.Json;
using LogProducer.Data;
using LogProducer.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace LogProducer.Services;

// TODO: implement remote posting and retrieval of logs
// TODO: if posting log to remote fails, save log locally
// TODO: add worker to periodically check if there are local logs saved and retry posting to remote
public class DistributedLogService: IDistributedLogService
{
    private string _authToken;
    private readonly string _username;
    private readonly string _password;
    private readonly HttpClient _httpClient;
    private static string _authUrl = "http://localhost:5039/api/auth/login";
    private static string _logUrl = "http://localhost:5039/api/log";

    public DistributedLogService(HttpClient httpClient, IConfiguration config)
    {

        _httpClient = httpClient;
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
                    _authToken = resultProperty.GetString();
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
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await _httpClient
                .PostAsJsonAsync(url, messages ?? (object)filter);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retryCount < 1)
            {
                await Authenticate();
                return await MakeRemoteLogRequest(action, messages, filter, retryCount + 1);
            }

            return response;
        } catch (Exception e)
        {
            Console.WriteLine(e);
            return new HttpResponseMessage();
        }
    }
    
    public async Task<LogMessage> LogAsync(LogMessage message)
    {
        var messages = new List<LogMessage>();
        messages.Add(message);
        
        const string action = "Batch";
        var response = await MakeRemoteLogRequest(action, messages, null, 0);
        return message;
    }

    public async Task<List<LogMessage>> LogManyAsync(List<LogMessage> messages)
    {
        // TODO: add error handling
        const string action = "Batch";
        var response = await MakeRemoteLogRequest(action, messages, null, 0);
        return messages;
    }

    public async Task<List<LogMessage>> GetLogsAsync(LogSearchFilter filter)
    {
        const string action = "Search";
        var response = await MakeRemoteLogRequest(action, null, filter);
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var logMessages = JsonSerializer.Deserialize<List<LogMessage>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Optional: To handle different casing in JSON
            });
            return logMessages ?? [];
        }

        return [];
    }

    public async Task DeleteLogsAsync(LogSearchFilter filter)
    {
        throw new NotImplementedException();
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