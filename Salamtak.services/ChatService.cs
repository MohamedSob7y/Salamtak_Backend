using Microsoft.Extensions.Configuration;
using Salamtak.Domain.Models;
using Salamtak.Persistance;
using Salamtak.services.Abstractions.Interfaces_Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salamtak.services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ChatService(AppDbContext db, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _db = db;
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = config["OpenAI:ApiKey"]!;
        }

        public async Task<string> SendMessageAsync(Guid patientId, string userMessage)
        {
            _db.ChatMessages.Add(new ChatMessage
            {
                PatientId = patientId,
                Role = "user",
                Content = userMessage,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var history = _db.ChatMessages
                .Where(m => m.PatientId == patientId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new { role = m.Role, content = m.Content })
                .ToList<object>();

            var request = new { model = "gpt-3.5-turbo", messages = history };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);
            var reply = result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "لا يوجد رد";

            _db.ChatMessages.Add(new ChatMessage
            {
                PatientId = patientId,
                Role = "assistant",
                Content = reply,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return reply;
        }

        public async Task<List<ChatMessage>> GetHistoryAsync(Guid patientId)
        {
            return _db.ChatMessages
                .Where(m => m.PatientId == patientId)
                .OrderBy(m => m.CreatedAt)
                .ToList();
        }
    }
}