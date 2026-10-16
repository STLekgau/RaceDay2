namespace RaceDay2.DTOs
{
    public class CreateEnrollmentRequest
    {
        // ParticipantID is now taken from the JWT, not the request body.
        public int EventID { get; set; }
        public int CategoryID { get; set; }
    }
}