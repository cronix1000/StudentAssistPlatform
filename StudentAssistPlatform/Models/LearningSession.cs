namespace StudentAssistPlatform.Models
{
    public class LearningSession
    {
        public string Subject { get; set; }
        public string Topic { get; set; }
        public string StudentExplanation { get; set; }
        public int Score { get; set; }
        public string AIFeedback { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
