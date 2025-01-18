using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


public class CalendarController : Controller
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }


    private async Task AddTasksToGoogleCalendar(CalendarService calendarService, List<CalendarEvent> tasks)
    {
        var currentDate = DateTime.Today;

        foreach (var (task, index) in tasks.Select((value, i) => (value, i)))
        {
            var eventDate = currentDate.AddDays(index);

            var newEvent = new Event
            {
                Summary = task.Summary,
                Start = new EventDateTime { Date = eventDate.ToString("yyyy-MM-dd") },
                End = new EventDateTime { Date = eventDate.ToString("yyyy-MM-dd") }
            };

            await calendarService.Events.Insert(newEvent, "primary").ExecuteAsync();
        }
    }
}