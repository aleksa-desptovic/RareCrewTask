using WebApp.Models;

namespace WebApp.Services
{
    public interface ITimeEntryService
    {
        Task<List<KeyValuePair<string, double>>> GetProcessedEntriesAsync();
    }
}