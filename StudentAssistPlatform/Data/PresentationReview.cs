namespace StudentAssistPlatform.Data
{
    public class PresentationReview
    {
        public int Id { get; set; }
        public int PresentationId { get; set; }
        public string ReviewerId { get; set; }
        public string Review { get; set; }
        public int Rating { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerEmail { get; set; }
        public string ReviewerProfilePicture { get; set; }
    }
}
