using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DambyulK2J.Services
{
    public class TranslationService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> _glossary = new();
        
        // [Safety] 한글 감지 정규식 (using System.Text.RegularExpressions 필요)
        private readonly Regex _koreanRegex = new Regex(@"[가-힣ㄱ-ㅎㅏ-ㅣ]");

        public TranslationService()
        {
            // (using System.Net.Http 필요)
            _httpClient = new HttpClient();
        }

        public async Task LoadDictionaryAsync()
        {
            await Task.Delay(100); 
            _glossary = new Dictionary<string, string>
            {
                { "리즈", "リズ" },
                { "산개", "散開" },
                { "쉐어", "頭割り" }
            };
        }

        public async Task<string> TranslateAsync(string text, string apiKey, string partyInfo)
        {
            if (string.IsNullOrEmpty(apiKey)) return "Error: API Key is missing.";

            string systemPrompt = SystemPrompts.GetBasePrompt(partyInfo, _glossary);

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = $"{systemPrompt}\n\n<input_text>\n{text}\n</input_text>" }
                        }
                    }
                }
            };

            try 
            {
                // (using Newtonsoft.Json 필요)
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode) return $"Error: API Status {response.StatusCode}";

                var responseString = await response.Content.ReadAsStringAsync();
                
                // (using Newtonsoft.Json.Linq 필요)
                var json = JObject.Parse(responseString);
                string translatedText = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().Trim() ?? "Translation Failed";

                if (_koreanRegex.IsMatch(translatedText))
                {
                    return "Error: Output contained Korean characters (Blocked for safety).";
                }

                return translatedText;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task SendReportAsync(string original, string translated, string reason)
        {
            await Task.Delay(100); 
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}