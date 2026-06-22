namespace Document_Management_System.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
