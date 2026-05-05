namespace CampusConnect.Models;

public class SupportRequest
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; }

    public string Subject { get; set; }
    public string Message { get; set; }

    public DateTime RequestDate { get; set; }
}