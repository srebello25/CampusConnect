using System.Diagnostics;
using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        
            ViewBag.Events = _context.Events
                .Include(e => e.Venue)
                .ToList();

            ViewBag.Announcements = _context.Announcements.ToList();

        
        return View();
    }

    [HttpPost]
    public IActionResult Register(string firstname, string lastname, string email, string password, string confirmPassword, string phone)
    {
        if (password != confirmPassword)
        {
            ViewBag.RegisterMessage = "Passwords do not match";
            return View("Index");
        }

        var existingUser = _context.Students.FirstOrDefault(x => x.Email == email);

        if (existingUser != null)
        {
            ViewBag.RegisterMessage = "Email already exists";
            return View("Index");
        }

        var student = new Student
        {
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            Password = password,
            Phone = phone
        };

        _context.Students.Add(student);
        _context.SaveChanges();

        ViewBag.Message = "Registered Successfully";
        return View("Index");
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var student = _context.Students
            .FirstOrDefault(x => x.Email == email && x.Password == password);

        if (student != null)
        {
            HttpContext.Session.SetInt32("StudentId", student.Id);
            HttpContext.Session.SetString("StudentName", student.FirstName);

            return RedirectToAction("Dashboard");
        }

        ViewBag.StudentMessage = "Invalid email or password";
        return View("Index");
    }

    [HttpPost]
    public IActionResult AdminLogin(string email, string password)
    {
        var admin = _context.Admins
            .FirstOrDefault(a => a.Email == email && a.Password == password);

        if (admin != null)
        {
            return RedirectToAction("AdminDashboard");
        }

        ViewBag.AdminMessage = "Invalid admin login";
        return View("Index");
    }

    public IActionResult Dashboard()
    {
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        var events = _context.Events
            .Include(e => e.Venue)
            .ToList();

        var registeredEvents = _context.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e.Venue)
            .Where(r => r.StudentId == studentId.Value)
            .ToList();

        ViewBag.RegisteredEvents = registeredEvents;

        var announcements = _context.Announcements.ToList();
        ViewBag.Announcements = announcements;

        ViewBag.StudentName = HttpContext.Session.GetString("StudentName");

        return View(events);
    }

    public IActionResult SupportRequest()
    {
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    public IActionResult SupportRequest(string subject, string message)
    {
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        var request = new SupportRequest
        {
            StudentId = studentId.Value,
            Subject = subject,
            Message = message,
            RequestDate = DateTime.Now
        };

        _context.SupportRequests.Add(request);
        _context.SaveChanges();

        ViewBag.Message = "Support request submitted successfully";
        return View();
    }

    [HttpPost]
    public IActionResult RegisterForEvent(int eventId)
    {
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        var existing = _context.Registrations
            .FirstOrDefault(r => r.StudentId == studentId.Value && r.EventId == eventId);

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

    [HttpPost]
    public IActionResult CancelRegistration(int eventId)
    {
        var studentId = HttpContext.Session.GetInt32("StudentId");

        if (studentId == null)
        {
            return RedirectToAction("Index");
        }

        var registration = _context.Registrations
            .FirstOrDefault(r => r.StudentId == studentId.Value && r.EventId == eventId);

        if (registration != null)
        {
            _context.Registrations.Remove(registration);
            _context.SaveChanges();
        }

        return RedirectToAction("Dashboard");
    }

    public IActionResult AdminDashboard()
    {
        var events = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
            .ThenInclude(r=> r.Student)
            .ToList();

        var announcements = _context.Announcements.ToList();
        ViewBag.Announcements = announcements;

        return View(events);
    }

    public IActionResult CreateEvent()
    {
        ViewBag.Venues = _context.Venues.ToList();
        return View();
    }

    public IActionResult CreateAnnouncement()
    {
        return View();
    }

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

    public IActionResult EditAnnouncement(int id)
    {
        var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
        return View(announcement);
    }

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

    public IActionResult EditEvent(int id)
    {
        var ev = _context.Events.FirstOrDefault(e => e.Id == id);

        ViewBag.Venues = _context.Venues.ToList();

        return View(ev);
    }

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

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}