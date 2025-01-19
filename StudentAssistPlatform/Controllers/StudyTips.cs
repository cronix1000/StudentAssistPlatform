using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI_API.Completions;
using StudentAssistPlatform.Models;
using System.Text.Json;

namespace StudentAssistPlatform.Controllers
{
    public class StudyTips : Controller
    {
        private readonly ITranscriptionService _transcriptionService;

        //private readonly CalendarService _googleCalendarService;
        private readonly string _openAIApiKey;


        public StudyTips(ITranscriptionService transcriptionService, IConfiguration configuration)
        {
            _transcriptionService = transcriptionService;
            //_googleCalendarService = googleCalendarService;
            _openAIApiKey = configuration["OpenAI:ApiKey"];
        }

        public IActionResult Index()
        {
            return View();
        }

       
        public class GradingResult
        {
            public int Score { get; set; }
            public string Feedback { get; set; }
            public List<string> ImprovementAreas { get; set; }
            public List<string> StrengthAreas { get; set; }
        }

        public class VoiceSubmission
        {
            public string StudentId { get; set; }
            public string Topic { get; set; }
            public string AudioBase64 { get; set; }
            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        }




        [HttpGet("active-recall")]
        public IActionResult ActiveRecall(string topic, string subject)
        {
            LearningSession learningSession = new()
            {
                Topic = topic,
                Subject = subject

            };
            
            return View(learningSession);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVoice([FromBody] VoiceSubmission submission)
        {
            try
            {
                //// Save to database
                //_context.Add(submission);
                //await _context.SaveChangesAsync();

                // Here you would process the audio file
                // For example, send it to speech-to-text service

                var base64String = submission.AudioBase64;
                if (base64String.Contains(","))
                {
                    base64String = base64String.Substring(base64String.IndexOf(",") + 1);
                }

                // 2. Convert base64 to byte array
                byte[] audioBytes;
                try
                {
                    audioBytes = Convert.FromBase64String(base64String);
                }
                catch
                {
                    return BadRequest("AudioBase64 is not valid base64.");
                }

                // 3. Call the transcription service
                //    Adjust fileName and contentType to match your actual data
                string fileName = submission.Topic + ".wav";
                string contentType = "audio/wav"; // or "audio/mpeg" or "audio/mp4", etc.

                var transcription = await _transcriptionService.TranscribeAudioAsync(audioBytes, fileName, contentType);


        
                return Ok(new
                {
                    transcription = transcription,
                    submissionId = submission.StudentId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error processing voice submission");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GiveFeedback([FromBody] LearningSession session)
        {
            if (!ModelState.IsValid)
                return View("StartSession", session);

            GradingResult result = await GradeExplanation(session);


            return Ok(new {
                score = result.Score,
                feedback = result.Feedback,
                improvementAreas = result.ImprovementAreas,
                strengthAreas = result.StrengthAreas
                });

        }

        public async Task<GradingResult> GradeExplanation(LearningSession session)
        {

            var openAi = new OpenAI_API.OpenAIAPI(_openAIApiKey);

            string prompt = $@"You are an expert teacher in {session.Subject}. 
A student is learning about {session.Topic}.
If the user says any swear words, it is an automatic 0 score and let them know that
they will not get better at studies if they swear and get mad.
You will also be very strict, analyzing the effectiveness of the user in educating
you as if you were a beginner in any subject that is related to school (i.e. math's,
science, art), etc.
Here is their explanation of the concept:
{session.StudentExplanation}

Please evaluate their understanding and provide only a JSON object with the following fields:
{{
  ""Score"": <score out of 100>,
  ""Feedback"": ""constructive feedback"",
  ""ImprovementAreas"": [""list"", ""of"", ""areas"", ""for"", ""improvement""],
  ""StrengthAreas"": [""list"", ""of"", ""strengths""]
}}

";

            // Call OpenAI's API
            var completion = await openAi.Completions.CreateCompletionAsync(new CompletionRequest()
            {
                Prompt = prompt,
                MaxTokens = 2000,
                Temperature = 0.7
            });

            // Assuming the response returns a JSON string that matches our GradingResult structure
            string jsonResponse = completion.Completions[0].Text.Trim();

            // Deserialize the JSON response into a GradingResult object
            GradingResult result;
            try
            {
                result = JsonSerializer.Deserialize<GradingResult>(jsonResponse);
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors
                throw new Exception("Failed to parse the AI response into a GradingResult.", ex);
            }

            return result;
        }
    }
}
