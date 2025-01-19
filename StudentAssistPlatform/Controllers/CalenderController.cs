using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;
using Newtonsoft.Json;
using StudentAssistPlatform.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


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
                
                //var tasks = await GenerateTasks(model.Subject, model.Struggles, model.TimelineInDays);

                //// Add tasks to Google Calendar
                //await AddTasksToGoogleCalendar(tasks);

                return RedirectToAction("Calender");
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
You are an AI assistant. Generate a JSON array containing daily learning tasks for the topic '{topic}' considering these struggles: '{struggles}'. 
Each object should include:
- 'title': Task title
- 'description': Brief description of the task
- 'url': A link to the task details
- 'resources': a string of resources broken by \n in the string, can be links or books
- 'assessmentQuestion': A question for evaluating understanding
- 'recallPrompt': A link to the recall page

Ensure the output is a single JSON array (not nested) in this format:
[
    {{
        ""title"": ""Task Title"",
        ""description"": ""Task Description"",
        ""url"": ""/lessons/task"",
        ""resources"" ""Resources"",
        ""assessmentQuestion"": ""Question text"",
        ""recallPrompt"": ""/StudyTips/active-recall?Topic={topic}""
    }}
]";

            var completion = await openAi.Completions.CreateCompletionAsync(prompt, max_tokens: 2000, temperature: 0.7);

            var jsonResponse = completion.Completions[0].Text;

            if (!jsonResponse.StartsWith("["))
                jsonResponse = "[" + jsonResponse;
            if (!jsonResponse.EndsWith("]"))
                jsonResponse = jsonResponse + "]";

            jsonResponse = CleanJsonResponse(jsonResponse);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Handle special characters better

                };

                return System.Text.Json.JsonSerializer.Deserialize<List<CalenderTask>>(jsonResponse, options);
            }
            catch (System.Text.Json.JsonException ex)
            {
                // Log the cleaned JSON for debugging
                Console.WriteLine($"Cleaned JSON that failed to parse: {jsonResponse}");
                throw new Exception($"Failed to parse OpenAI response: {ex.Message}\nJSON: {jsonResponse}");
            }
        }

        private string CleanJsonResponse(string json)
        {
            // Remove any potential Unicode characters or invalid whitespace
            json = Regex.Replace(json, @"[\u0000-\u001F]", "");

            // Ensure the response is a proper array
            json = json.Trim();
            if (!json.StartsWith("["))
                json = "[" + json;
            if (!json.EndsWith("]"))
                json = json + "]";

            // Remove any trailing commas before closing brackets
            json = Regex.Replace(json, @",(\s*[\}\]])", "$1");

            // Handle apostrophes and quotes properly
            json = Regex.Replace(json, @"(?<!\\)'", "\\'"); // Escape unescaped single quotes
            json = json.Replace("\"", "\\\"").Replace("'", "\""); // Replace all quotes with escaped double quotes

            // Remove any duplicate whitespace
            json = Regex.Replace(json, @"\s+", " ");

            // Additional cleaning for common issues
            json = json.Replace("\\'", "'"); // Fix any over-escaped apostrophes
            json = Regex.Replace(json, @"\\""([,\]}])", "\"$1"); // Fix escaped quotes before delimiters

            return json;
        }

        [HttpGet]
        public IActionResult Resources(string title,string description,  string resources)
        {
            var viewModel = new LearningSession
            {
                Subject = description,
                Topic = title,
                CreatedAt = DateTime.Now
            };

            // Split resources into a list for display
            ViewBag.Resources = resources.Split(",").ToList();
            return View(viewModel);
        }



        //public IActionResult Calender(List<CalenderTask> tasks)
        //{
        //    return View(tasks);
        //}

        public IActionResult Calender()
        {

            var tasks = new List<CalenderTask>() { 

    // Mathematics Tasks
    new CalenderTask
    {
        Title = "Introduction to Linear Algebra",
        Description = "Learn the fundamentals of vectors, matrices, and linear transformations",
        Resources = "1. MIT OpenCourseware Linear Algebra (https://ocw.mit.edu/courses/mathematics/18-06-linear-algebra-spring-2010/), 2. 3Blue1Brown Essence of Linear Algebra (https://www.3blue1brown.com/essence-of-linear-algebra-series), 3. Linear Algebra and Its Applications by Gilbert Strang (https://math.mit.edu/~gs/linearalgebra)",
        AssessmentQuestion = "Explain how matrix multiplication represents the composition of linear transformations",
        RecallPrompt = "/StudyTips/active-recall?Topic=LinearAlgebra",
        Date = "2025-01-20" // Assign specific date
    },
    new CalenderTask
    {
        Title = "Calculus: Limits and Continuity",
        Description = "Master the concepts of limits and continuity as foundations for calculus",
        Resources = "1. Khan Academy Calculus Course (https://www.khanacademy.org/math/calculus-1), 2. Paul's Online Math Notes (http://tutorial.math.lamar.edu/Classes/CalcI/CalcI.aspx), 3. Calculus: Early Transcendentals by James Stewart (https://www.stewartcalculus.com)",
        AssessmentQuestion = "What is the formal epsilon-delta definition of a limit, and how does it relate to continuity?",
        RecallPrompt = "/StudyTips/active-recall?Topic=Calculus",
        Date = "2025-01-21" // Assign specific date
    },

    // Physics Tasks
    new CalenderTask
    {
        Title = "Classical Mechanics Foundations",
        Description = "Study Newton's laws of motion and their applications",
        Resources = "1. Feynman Lectures on Physics Vol 1 (http://www.feynmanlectures.caltech.edu/I_00.html), 2. HyperPhysics - Mechanics (http://hyperphysics.phy-astr.gsu.edu/hbase/mechanics.html), 3. Physics for Scientists and Engineers by Serway (https://www.cengage.com/c/physics-for-scientists-and-engineers-9e-serway/)",
        AssessmentQuestion = "How do Newton's three laws of motion explain the motion of a satellite in orbit?",
        RecallPrompt = "/StudyTips/active-recall?Topic=ClassicalMechanics",
        Date = "2025-01-22" // Assign specific date
    },
    new CalenderTask
    {
        Title = "Quantum Mechanics Basics",
        Description = "Explore wave-particle duality and the Schrödinger equation",
        Resources = "1. Quantum Mechanics: The Theoretical Minimum by Leonard Susskind (https://www.theoreticalminimum.com/quantum-mechanics), 2. MIT OCW Quantum Physics I (https://ocw.mit.edu/courses/physics/8-04-quantum-physics-i-spring-2016/), 3. Quantum Mechanics by Claude Cohen-Tannoudji (https://www.wiley.com/en-us/Quantum+Mechanics%2C+Volume+1-p-9780471169617)",
        AssessmentQuestion = "Explain the double-slit experiment and its implications for quantum mechanics",
        RecallPrompt = "/StudyTips/active-recall?Topic=QuantumMechanics",
        Date = "2025-01-23" // Assign specific date
    },

    // Computer Science Tasks
    new CalenderTask
    {
        Title = "Data Structures Implementation",
        Description = "Learn to implement and use fundamental data structures",
        Resources = "1. Introduction to Algorithms (CLRS) (https://mitpress.mit.edu/books/introduction-algorithms-third-edition), 2. GeeksforGeeks Data Structures (https://www.geeksforgeeks.org/data-structures/), 3. Coursera Algorithms Specialization (https://www.coursera.org/specializations/algorithms)",
        AssessmentQuestion = "Compare and contrast the performance characteristics of different tree structures (BST, AVL, Red-Black)",
        RecallPrompt = "/StudyTips/active-recall?Topic=DataStructures",
        Date = "2025-01-24" // Assign specific date
    },
    new CalenderTask
    {
        Title = "Algorithm Analysis",
        Description = "Master Big O notation and algorithm complexity analysis",
        Resources = "1. Algorithm Design Manual by Skiena (https://www.algorist.com/), 2. LeetCode Problems Collection (https://leetcode.com/problemset/all/), 3. MIT OCW Introduction to Algorithms (https://ocw.mit.edu/courses/electrical-engineering-and-computer-science/6-006-introduction-to-algorithms-spring-2020/)",
        AssessmentQuestion = "Analyze the time and space complexity of quicksort, including best and worst cases",
        RecallPrompt = "/StudyTips/active-recall?Topic=Algorithms",
        Date = "2025-01-25" // Assign specific date
    },

    // Biology Tasks
    new CalenderTask
    {
        Title = "Cell Biology Fundamentals",
        Description = "Study cell structure, function, and cellular processes",
        Resources = "1. Essential Cell Biology by Alberts (https://www.essentialcellbiology.com/), 2. Khan Academy Cell Biology (https://www.khanacademy.org/science/biology), 3. HHMI BioInteractive Resources (https://www.biointeractive.org/classroom-resources)",
        AssessmentQuestion = "Describe the process of cellular respiration and its relationship with photosynthesis",
        RecallPrompt = "/StudyTips/active-recall?Topic=CellBiology",
        Date = "2025-01-26" // Assign specific date
    },
    new CalenderTask
    {
        Title = "Genetics and Inheritance",
        Description = "Explore DNA structure, replication, and inheritance patterns",
        Resources = "1. Genetics: A Conceptual Approach by Pierce (https://www.macmillanlearning.com/college/us/product/Genetics/p/1319050692), 2. Nature Education Genetics (https://www.nature.com/scitable/topic/genetics-5), 3. DNA Learning Center Resources (https://www.dnalc.org/)",
        AssessmentQuestion = "Explain how DNA mutations can lead to phenotypic changes and their evolutionary implications",
        RecallPrompt = "/StudyTips/active-recall?Topic=Genetics",
        Date = "2025-01-27" // Assign specific date
    }
};


        
    return View(tasks);
    }

        public class CalenderTask
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Resources  { get; set; } // List of learning resources
            public string AssessmentQuestion { get; set; } // Question for assessment
            public string RecallPrompt { get; set; } // Recall prompt with links
            public string Date { get; set; }
        }
    }

    public class ScheduleRequest
    {
        public string Subject { get; set; }
        public string Struggles { get; set; }
        public int TimelineInDays { get; set; }
    }


}