using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;
using Newtonsoft.Json;
using StudentAssistPlatform.Models;


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

        private async Task<List<CalenderTask>> GenerateTasks(string topic, string struggles, int timelineInDays)
        {
            var openAi = new OpenAI_API.OpenAIAPI(_openAIApiKey);

            var prompt = $@"
You are an AI assistant. Generate a detailed daily learning flow for the topic '{topic}' considering these struggles: '{struggles}'.
The plan should span {timelineInDays} days, and each day should include:
- A title summarizing the task.
- A list of 2-3 learning resources (like websites, videos, or books).
- An assessment question to evaluate understanding.
- A recall prompt asking the user to explain the topic as if you know nothing.

Include links for active recall as: '/StudyTips/active-recall?Topic={topic}'.
Format the output as a JSON array:
[
    {{
        'date': 'YYYY-MM-DD',
        'title': 'Task Title',
        'resources': 'List of resources',
        'assessmentQuestion': 'Question for assessment',
        'recallPrompt': 'Recall prompt with links'
    }},
    ...
]";
            var completion = await openAi.Completions.CreateCompletionAsync(prompt, max_tokens: 500, temperature: 0.7);

            var jsonResponse = completion.Completions[0].Text;

            var tasks = JsonConvert.DeserializeObject<List<CalenderTask>>(jsonResponse);

            return tasks ?? new List<CalenderTask>();
        }

        [HttpGet("resources")]
        public IActionResult Resources([FromQuery] string subject, [FromQuery] string topic, [FromQuery] string resources, [FromQuery] DateTime createdAt)
        {
            var viewModel = new LearningSession
            {
                Subject = subject,
                Topic = topic,
                CreatedAt = createdAt
            };

            // Split resources into a list for display
            ViewBag.Resources = resources.Split('\n').ToList();
            return View(viewModel);
        }



        public IActionResult Calander(List<CalenderTask> tasks)
        {
            return View(tasks);
        }

//        public IActionResult Calender()
//        {
//            var tasks = new List<CalenderTask>
//{
//    // Math-related tasks
//    new CalenderTask
//    {
//        Title = "Understand Derivatives Basics",
//        Description = "Learn the foundational concepts of derivatives in calculus.",
//        Url = "/StudyTips/resources?Subject=Math&Topic=Derivatives",
//        Resources = "1. https://www.khanacademy.org/math/calculus-1/derivatives\n2. 'Calculus Made Easy' by Silvanus P. Thompson\n3. YouTube: https://youtube.com/derivatives101",
//        AssessmentQuestion = "What is the derivative of x^2, and how does it represent the slope of the curve?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Math&Topic=Derivatives&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-20"
//    },
//    new CalenderTask
//    {
//        Title = "Learn Integration Techniques",
//        Description = "Explore key integration techniques for solving calculus problems.",
//        Url = "/StudyTips/resources?Subject=Math&Topic=Integration",
//        Resources = "1. https://example.com/integration-techniques\n2. 'Advanced Calculus' by James Stewart\n3. YouTube: https://youtube.com/integration-techniques",
//        AssessmentQuestion = "What is the integral of sin(x), and what method do you use to solve it?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Math&Topic=Integration&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-21"
//    },

//    // Biology-related tasks
//    new CalenderTask
//    {
//        Title = "Understand Photosynthesis",
//        Description = "Learn the process of how plants convert sunlight into energy.",
//        Url = "/StudyTips/resources?Subject=Biology&Topic=Photosynthesis",
//        Resources = "1. https://www.khanacademy.org/science/photosynthesis\n2. 'Photosynthesis Explained' by Biology101\n3. YouTube: https://youtube.com/photosynthesis-basics",
//        AssessmentQuestion = "What are the two stages of photosynthesis, and where do they occur in the cell?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Biology&Topic=Photosynthesis&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-20"
//    },
//    new CalenderTask
//    {
//        Title = "Explore Cellular Respiration",
//        Description = "Understand how cells convert glucose into ATP.",
//        Url = "/StudyTips/resources?Subject=Biology&Topic=Cellular Respiration",
//        Resources = "1. https://example.com/cellular-respiration\n2. 'Biology for Beginners' by Dr. Smith\n3. YouTube: https://youtube.com/cellular-respiration",
//        AssessmentQuestion = "What are the three stages of cellular respiration, and what are their outputs?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Biology&Topic=Cellular Respiration&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-21"
//    },

//    // History-related tasks
//    new CalenderTask
//    {
//        Title = "Study the Causes of World War I",
//        Description = "Analyze the key causes and events leading to World War I.",
//        Url = "/StudyTips/resources?Subject=History&Topic=World War I",
//        Resources = "1. https://history.com/ww1-causes\n2. 'The Great War' by John Keegan\n3. YouTube: https://youtube.com/world-war-1-causes",
//        AssessmentQuestion = "What were the main causes of World War I, and how did alliances contribute to the conflict?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=History&Topic=World War I&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-20"
//    },
//    new CalenderTask
//    {
//        Title = "Learn About the French Revolution",
//        Description = "Understand the events and impact of the French Revolution.",
//        Url = "/StudyTips/resources?Subject=History&Topic=French Revolution",
//        Resources = "1. https://example.com/french-revolution\n2. 'Revolutions in History' by Jane Doe\n3. YouTube: https://youtube.com/french-revolution-overview",
//        AssessmentQuestion = "What were the key events of the French Revolution, and how did they change France?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=History&Topic=French Revolution&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-21"
//    },

//    // Language-learning tasks
//    new CalenderTask
//    {
//        Title = "Practice Spanish Vocabulary",
//        Description = "Learn and practice 20 new Spanish words.",
//        Url = "/StudyTips/resources?Subject=Language&Topic=Spanish Vocabulary",
//        Resources = "1. https://duolingo.com\n2. 'Spanish for Beginners' by Maria Lopez\n3. YouTube: https://youtube.com/spanish-vocabulary",
//        AssessmentQuestion = "What is the Spanish word for 'apple', and how do you use it in a sentence?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Language&Topic=Spanish Vocabulary&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-20"
//    },
//    new CalenderTask
//    {
//        Title = "Learn Spanish Grammar Basics",
//        Description = "Understand the rules of Spanish grammar and sentence structure.",
//        Url = "/StudyTips/resources?Subject=Language&Topic=Spanish Grammar",
//        Resources = "1. https://spanishdict.com/grammar\n2. 'Grammar Essentials' by Carlos Rivera\n3. YouTube: https://youtube.com/spanish-grammar",
//        AssessmentQuestion = "What is the rule for conjugating -ar verbs in Spanish?",
//        RecallPrompt = "/StudyTips/active-recall?Subject=Language&Topic=Spanish Grammar&StudentExplanation=&Score=0&AIFeedback=&CreatedAt=2025-01-21"
//    }
//};

//            return View(tasks);

//        }


        public class CalenderTask
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string Resources { get; set; } // List of learning resources
            public string AssessmentQuestion { get; set; } // Question for assessment
            public string RecallPrompt { get; set; } // Recall prompt with links
        }
    }

    public class ScheduleRequest
    {
        public string Subject { get; set; }
        public string Struggles { get; set; }
        public int TimelineInDays { get; set; }
    }


}