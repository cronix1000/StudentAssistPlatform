using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;


namespace StudentAssistPlatform.Controllers
{
    public class CalenderController : Controller
    {
        //private readonly CalendarService _googleCalendarService;
        private readonly string _openAIApiKey;

        public CalenderController(IConfiguration configuration)
        {
            //_googleCalendarService = googleCalendarService;
            _openAIApiKey = configuration["OpenAI:ApiKey"];
        }

        public class ScheduleFormModel
        {
            public string Subject { get; set; }
            public string Struggles { get; set; }
            public int TimelineInDays { get; set; }
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ScheduleFormModel());
        }


        [HttpPost]
        public async Task<IActionResult> Generate(ScheduleFormModel model)
        {
            try
            {
                // Generate tasks using OpenAI
                var tasks = await GenerateTasks(model.Subject, model.Struggles, model.TimelineInDays);



                //// Add tasks to Google Calendar
                //await AddTasksToGoogleCalendar(tasks);

                return RedirectToAction("Calander", new { tasks = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<List<CalenderTask>> GenerateTasks(string subject, string struggles, int timelineInDays)
        {
            var openAi = new OpenAI_API.OpenAIAPI(_openAIApiKey);
            var prompt = $@"    You are an AI assistant. Generate a detailed study plan for '{subject}' considering these struggles: '{struggles}'.
    The plan should span {timelineInDays} days with actionable daily tasks. Each task should include:
    - A title summarizing the task.
    - A brief description of the task.
    - A built-in function with a corresponding URL (e.g., 'Active Recall: /functions/active-recall').

    Return the output as a JSON array in this format:
    [
        {{
            'date': 'YYYY-MM-DD',
            'title': 'Task Title',
            'description': 'Task Description',
            'url': '/functions/active-recall'
        }},
        ...
    ]";



            var completion = await openAi.Completions.CreateCompletionAsync(prompt, max_tokens: 500, temperature: 0.7);


            var tasks = new List<CalenderTask>();
            var taskLines = completion.Completions[0].Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in taskLines)
            {
                tasks.Add(new CalenderTask { Title = line.Trim() });
            }

            return tasks;
        }

        //public IActionResult Calander(List<CalanderTask> tasks)
        //{
        //    return View(tasks);
        //}

        public IActionResult Calender()
        {
            // Call your GenerateTasks method and fetch the tasks (dummy data for demonstration)
            var tasks = new List<CalenderTask>
    {
        new CalenderTask { Title = "Review calculus concepts", Url = "/functions/active-recall", Description = "Work on remembering equations", Date = "2025-01-20" },
        new CalenderTask { Title = "Practice derivatives", Url = "/functions/spaced-repetition", Description = "Solve derivative problems", Date = "2025-01-21" },
        new CalenderTask { Title = "Learn integration techniques", Url = "/functions/active-recall", Description = "Learn and practice integrations", Date = "2025-01-22" },
        new CalenderTask { Title = "Solve integration problems", Url = "/functions/spaced-repetition", Description = "Focus on solving advanced problems", Date = "2025-01-23" }
    };

            return View(tasks);
        }


        public class CalenderTask
        {
            public string Date { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
        }
    }

    public class ScheduleRequest
    {
        public string Subject { get; set; }
        public string Struggles { get; set; }
        public int TimelineInDays { get; set; }
    }


}