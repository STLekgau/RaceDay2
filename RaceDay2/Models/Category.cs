namespace RaceDay2.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}