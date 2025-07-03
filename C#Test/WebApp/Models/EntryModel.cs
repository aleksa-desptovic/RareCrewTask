namespace WebApp.Models 
{
    public class EntryModel
    {
        public required string Id { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string? EntryNotes { get; set; }
        public string? DeletedOn { get; set; }
    }
}