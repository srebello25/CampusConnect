namespace CampusConnect.Models;

// stores event details
public class Event
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime EventDate { get; set; }

    public int VenueId { get; set; }
    public Venue Venue { get; set; }

    public int AdminId { get; set; }
    public List<Registration> Registrations { get; set; } = new List<Registration>();
}