using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace StudentAssistPlatform.Controllers
{
    public class StudyTips : Controller
    {
        private readonly ITranscriptionService _transcriptionService;

        public StudyTips(ITranscriptionService transcriptionService)
        {
            _transcriptionService = transcriptionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public class LearningSession
        {
            public int Id { get; set; }
            public string StudentId { get; set; }
            public string Subject { get; set; }
            public string Topic { get; set; }
            public string LearningContext { get; set; }
            public string StudentExplanation { get; set; }
            public int Score { get; set; }
            public string AIFeedback { get; set; }
            public DateTime CreatedAt { get; set; }
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




        public IActionResult ActiveRecall()
        {
            return View();
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
                    message = "Voice recording uploaded successfully",
                    submissionId = submission.StudentId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error processing voice submission");
            }
        }



        [HttpPost]
        public async Task<IActionResult> SubmitExplanation(LearningSession session)
        {
            if (!ModelState.IsValid)
                return View("StartSession", session);

            GradingResult result = await GradeExplanation(session);

            return View();


        }


        public async Task<GradingResult> GradeExplanation(LearningSession session)
        {
            // Here you would integrate with an AI service (e.g., Cl
            //
            // e API)
            // Example prompt structure:
            string prompt = $@"You are an expert teacher in {session.Subject}. 
            A student is learning about {session.Topic}. 
            Here is their explanation of the concept:
            {session.StudentExplanation}
            
            Context provided by the student:
            {session.LearningContext}
            
            Please evaluate their understanding and provide:
            1. A score out of 100
            2. Constructive feedback
            3. Areas for improvement
            4. Areas of strength";

            // AI service call would go here
            // For now, returning mock data
            return new GradingResult
            {
                Score = 85,
                Feedback = "Good explanation with clear understanding of core concepts...",
                ImprovementAreas = new List<string> { "Consider adding more examples" },
                StrengthAreas = new List<string> { "Clear articulation of main ideas" }
            };
        }
    }
}
