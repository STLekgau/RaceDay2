
namespace RaceDay2.DTOs
{
    public class CreateResultRequest
    {
        public int EnrollmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishingPosition { get; set; }
    }
}
