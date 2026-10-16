namespace RaceDay2.DTOs
{
    public class CreateEventRequest
    {
        // OrganizerID is now taken from the JWT, not the request body.
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "RUN";
    }
}
