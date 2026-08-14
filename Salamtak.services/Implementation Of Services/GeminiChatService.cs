using Salamtak.services.Abstractions.Interfaces_Services;
using Microsoft.Extensions.Configuration;
using Salamtak.services.Abstractions;
using Salamtak.Shared.Chat;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Google.GenAI;
using Google.GenAI.Types;

namespace Salamtak.services
{
    public class GeminiChatService : IAiChatService
    {
        private readonly IConfiguration _config;

        public GeminiChatService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ChatResponseDto> GetMedicalSpecialtyAsync(ChatRequestDto request)
        {
            var apiKey = _config["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API Key is missing from configuration.");
            }

            var client = new Client(apiKey: apiKey);
            var prompt = $@"
                You are a helpful AI medical assistant. 
                The user describes their symptoms: '{request.Message}'.
                
                Tasks:
                1. Write a brief, empathetic response advising them on what to do. Reply in the same language the user typed in.
                2. Identify the exact medical specialty they should visit. The specialty MUST be in English.
                
                Output format constraints:
                Return a valid JSON object with exactly two keys: 'text' and 'specialty'.";

            var configOptions = new GenerateContentConfig { ResponseMimeType = "application/json" };

            int maxRetries = 3;
            int delayMilliseconds = 1500;
            string rawJson = "";

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await client.Models.GenerateContentAsync(
                        model: "gemini-2.5-flash",
                        contents: prompt,
                        config: configOptions
                    );
                    rawJson = response.Text ?? "";
                    break;
                }
                catch (Exception) when (attempt < maxRetries)
                {
                    await Task.Delay(delayMilliseconds);
                    delayMilliseconds *= 2;
                }
                catch (Exception)
                {
                    return new ChatResponseDto
                    {
                        Text = "Chat Is Busy Now. Please try again.",
                        Specialty = "General Practice"
                    };
                }
            }

            try
            {
                rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
                var result = JsonSerializer.Deserialize<ChatResponseDto>(rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ChatResponseDto { Text = "Error reading response.", Specialty = "General Practice" };
            }
            catch
            {
                return new ChatResponseDto { Text = "Error interpreting the AI response.", Specialty = "General Practice" };
            }
        }
    }
}