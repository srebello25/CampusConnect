using System.Diagnostics;
using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    // Gives  controller access to the database
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Loads homepage events and announcements
    private void LoadHomePageData()
    {
        ViewBag.Events = _context.Events
            .Include(e => e.Venue)
            .ToList();

        ViewBag.Announcements = _context.Announcements.ToList();
    }

    // Shows the homepage with events and announcements
    public IActionResult Index()
    {
        LoadHomePageData();
        return View();
    }

    // Saves a new student account
    [HttpPost]
    public IActionResult Register(string firstname, string lastname, string email, string password, string confirmPassword, string phone)
    {
        // Checks that both passwords are the same
        if (password != confirmPassword)
        {
            ViewBag.RegisterMessage = "Passwords do not match";
            LoadHomePageData();
            return View("Index");
        }

        // Checks if this email is already used
        var existingUser = _context.Students.FirstOrDefault(x => x.Email == email);

        if (existingUser != null)
        {
            ViewBag.RegisterMessage = "Email already exists";
            LoadHomePageData();
            return View("Index");
        }

        // Creates a new student
        var student = new Student
        {
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            Password = password,
            Phone = phone
        };

        // Saves the student
        _context.Students.Add(student);
        _context.SaveChanges();

        ViewBag.Message = "Registered Successfully";
        LoadHomePageData();
        return View("Index");
    }

    // Logs in a student
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        // Finds a student with matching email and password
        var student = _context.Students
            .FirstOrDefault(x => x.Email == email && x.Password == password);

        if (student != null)
        {
            // Remembers the logged-in student
            HttpContext.Session.SetInt32("StudentId", student.Id);
            HttpContext.Session.SetString("StudentName", student.FirstName);

            return RedirectToAction("Dashboard");
        }

        ViewBag.StudentMessage = "Invalid email or password";
        LoadHomePageData();
        return View("Index");
    }

    // Logs in an admin
    [HttpPost]
    public IActionResult AdminLogin(string email, string password)
    {
        // Finds an admin with matching email and password
        var admin = _context.Admins
            .FirstOrDefault(a => a.Email == email && a.Password == password);

        if (admin != null)
        {
            return RedirectToAction("AdminDashboard");
        }

        ViewBag.AdminMessage = "Invalid admin login";
        LoadHomePageData();
        return View("Index");
    }

    // Shows the student dashboard
    public IActionResult Dashboard()
    {
        // Gets the logged-in student ID
        var studentId = HttpContext.Session.GetInt32("StudentId");

        // Sends user to homepage if not logged in
        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        // Gets all events with venue details
        var events = _context.Events
            .Include(e => e.Venue)
            .ToList();

        // Gets events registered by this student
        var registeredEvents = _context.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e.Venue)
            .Where(r => r.StudentId == studentId.Value)
            .ToList();

        ViewBag.RegisteredEvents = registeredEvents;

        // Gets announcements for the dashboard
        var announcements = _context.Announcements.ToList();
        ViewBag.Announcements = announcements;

        // Sends student name to the dashboard
        ViewBag.StudentName = HttpContext.Session.GetString("StudentName");

        return View(events);
    }

    // Opens the support request page
    public IActionResult SupportRequest()
    {
        // Checks if student is logged in
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    // Saves a support request
    [HttpPost]
    public IActionResult SupportRequest(string subject, string message)
    {
        // Gets the logged-in student ID
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        // Creates a support request
        var request = new SupportRequest
        {
            StudentId = studentId.Value,
            Subject = subject,
            Message = message,
            RequestDate = DateTime.Now
        };

        // Saves the support request
        _context.SupportRequests.Add(request);
        _context.SaveChanges();

        ViewBag.Message = "Support request submitted successfully";
        return View();
    }

    // Registers the student for an event
    [HttpPost]
    public IActionResult RegisterForEvent(int eventId)
    {
        // Gets the logged-in student ID
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        // Checks if the student is already registered
        var existing = _context.Registrations
            .FirstOrDefault(r => r.StudentId == studentId.Value && r.EventId == eventId);

        // Adds registration only if it does not already exist
        if (existing == null)
        {
            var registration = new Registration
            {
                StudentId = studentId.Value,
                EventId = eventId,
                RegistrationDate = DateTime.Now
            };

            _context.Registrations.Add(registration);
            _context.SaveChanges();
        }

        return RedirectToAction("Dashboard");
    }

    // Cancels the student's event registration
    [HttpPost]
    public IActionResult CancelRegistration(int eventId)
    {
        // Gets the logged-in student ID
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        // Finds the registration
        var registration = _context.Registrations
            .FirstOrDefault(r => r.StudentId == studentId.Value && r.EventId == eventId);

        // Removes the registration if it exists
        if (registration != null)
        {
            _context.Registrations.Remove(registration);
            _context.SaveChanges();
        }

        return RedirectToAction("Dashboard");
    }

    // Shows the admin dashboard
    public IActionResult AdminDashboard()
    {
        // Gets events with venue and student registration details
        var events = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
            .ThenInclude(r => r.Student)
            .ToList();

        // Gets announcements for the admin dashboard
        var announcements = _context.Announcements.ToList();
        ViewBag.Announcements = announcements;

        return View(events);
    }

    // Opens the create event page
    public IActionResult CreateEvent()
    {
        // Gets venues for the dropdown list
        ViewBag.Venues = _context.Venues.ToList();
        return View();
    }

    // Opens the create announcement page
    public IActionResult CreateAnnouncement()
    {
        return View();
    }

    // Saves a new announcement
    [HttpPost]
    public IActionResult CreateAnnouncement(string title, string content)
    {
        var announcement = new Announcement
        {
            AdminId = 1,
            Title = title,
            Content = content,
            DatePosted = DateTime.Now
        };

        _context.Announcements.Add(announcement);
        _context.SaveChanges();

        return RedirectToAction("AdminDashboard");
    }

    // Opens the edit announcement page
    public IActionResult EditAnnouncement(int id)
    {
        var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
        return View(announcement);
    }

    // Saves edited announcement details
    [HttpPost]
    public IActionResult EditAnnouncement(int id, string title, string content)
    {
        var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);

        if (announcement != null)
        {
            announcement.Title = title;
            announcement.Content = content;
            announcement.DatePosted = DateTime.Now;

            _context.SaveChanges();
        }

        return RedirectToAction("AdminDashboard");
    }

    // Deletes an announcement
    [HttpPost]
    public IActionResult DeleteAnnouncement(int announcementId)
    {
        var announcement = _context.Announcements.FirstOrDefault(a => a.Id == announcementId);

        if (announcement != null)
        {
            _context.Announcements.Remove(announcement);
            _context.SaveChanges();
        }

        return RedirectToAction("AdminDashboard");
    }

    // Saves a new event
    [HttpPost]
    public IActionResult CreateEvent(string title, string description, DateTime eventDate, int venueId)
    {
        var newEvent = new Event
        {
            Title = title,
            Description = description,
            EventDate = eventDate,
            VenueId = venueId,
            AdminId = 1
        };

        _context.Events.Add(newEvent);
        _context.SaveChanges();

        return RedirectToAction("AdminDashboard");
    }

    // Opens the edit event page
    public IActionResult EditEvent(int id)
    {
        var ev = _context.Events.FirstOrDefault(e => e.Id == id);

        // Gets venues for the dropdown list
        ViewBag.Venues = _context.Venues.ToList();

        return View(ev);
    }

    // Saves edited event details
    [HttpPost]
    public IActionResult EditEvent(int id, string title, string description, DateTime eventDate, int venueId)
    {
        var ev = _context.Events.FirstOrDefault(e => e.Id == id);

        if (ev != null)
        {
            ev.Title = title;
            ev.Description = description;
            ev.EventDate = eventDate;
            ev.VenueId = venueId;

            _context.SaveChanges();
        }

        return RedirectToAction("AdminDashboard");
    }

    // Deletes an event
    [HttpPost]
    public IActionResult DeleteEvent(int eventId)
    {
        var ev = _context.Events.FirstOrDefault(e => e.Id == eventId);

        if (ev != null)
        {
            _context.Events.Remove(ev);
            _context.SaveChanges();
        }

        return RedirectToAction("AdminDashboard");
    }

    // Logs out the user
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Shows the error page
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}