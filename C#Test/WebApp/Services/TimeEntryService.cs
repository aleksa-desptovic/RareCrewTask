using System.Net.Http;
using System.Text.Json;
using WebApp.Models;
using Microsoft.Extensions.Options;

namespace WebApp.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public TimeEntryService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> options)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiSettings = options.Value;
        }
        public async Task<List<KeyValuePair<string, double>>> GetProcessedEntriesAsync()
        {
            var url = $"{_apiSettings.TimeEntryApiUrl}?code={_apiSettings.TimeEntryApiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<List<EntryModel>>(json, new JsonSerializerOptions { });

            var employeeHours = new Dictionary<string, double>();

            if (entries == null) return [];

            foreach (var entry in entries)
            {
                if (entry.DeletedOn != null) continue;
                if (entry.EmployeeName == null) continue;

                var duration = (entry.EndTimeUtc - entry.StarTimeUtc).TotalHours;
                if (duration < 0) continue;

                if (employeeHours.ContainsKey(entry.EmployeeName))
                    employeeHours[entry.EmployeeName] += duration;
                else
                    employeeHours[entry.EmployeeName] = duration;
            }

            return employeeHours.OrderByDescending(e => e.Value).ToList();
        }
    }
}