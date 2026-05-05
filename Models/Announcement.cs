namespace CampusConnect.Models;

public class Announcement
{
    public int Id { get; set; }

    public int AdminId { get; set; }
    public Admin Admin { get; set; }

    public string Title { get; set; }
    public string Content { get; set; }

    public DateTime DatePosted { get; set; }
}