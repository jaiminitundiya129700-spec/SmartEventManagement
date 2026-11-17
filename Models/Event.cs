using System.ComponentModel.DataAnnotations;
namespace SmartEventManagement.Models;
public class Event
{
    public int Id { get; set; }
    [Required] public string Title { get; set; }
    public string Description { get; set; }
    [Required] public string Venue { get; set; }
    [Required] public string Category { get; set; }
    [DataType(DataType.DateTime)] public DateTime Date { get; set; } = DateTime.Today.AddDays(7);
    [Range(1, 100000)] public int Capacity { get; set; } = 100;
    [Range(0, 1000000)] public decimal Price { get; set; }
    public List<Registration> Registrations { get; set; } = new();
    public int SeatsTaken => Registrations?.Sum(r => r.Tickets) ?? 0;
    public int SeatsLeft => Capacity - SeatsTaken;
    public string Theme => Category?.ToLower() switch { "technical" => "t-blue", "cultural" => "t-pink", "workshop" => "t-green", "sports" => "t-orange", _ => "t-violet" };
    public string Icon => Category?.ToLower() switch { "technical" => "bi-cpu", "cultural" => "bi-music-note-beamed", "workshop" => "bi-tools", "sports" => "bi-trophy", _ => "bi-stars" };
    public string Status => Date < DateTime.Now ? "Completed" : SeatsLeft <= 0 ? "Sold Out" : SeatsLeft <= Capacity * 0.2 ? "Filling Fast" : "Open";
}
public class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int? UserId { get; set; }
    public Event Event { get; set; }
    [Required] public string Name { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Phone] public string Phone { get; set; }
    [Range(1, 10)] public int Tickets { get; set; } = 1;
    public DateTime RegisteredOn { get; set; } = DateTime.Now;
}
