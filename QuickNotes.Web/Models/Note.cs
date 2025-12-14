namespace QuickNotes.Web.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string Content { get; set; }
    }
}
