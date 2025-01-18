public interface ICalendarService
{
    void CreateEvent(CalendarEvent calendarEvent);
}

public class CalendarEvent
{
    public string Summary { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
