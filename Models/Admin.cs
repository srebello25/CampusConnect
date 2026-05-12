namespace CampusConnect.Models;

// Stores admin details
public class Admin
{
    public int Id { get; set; }

    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}