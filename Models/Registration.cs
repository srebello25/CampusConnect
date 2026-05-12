namespace CampusConnect.Models;

//Stores student event registrations
public class Registration
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; }

    public DateTime RegistrationDate { get; set; }
}