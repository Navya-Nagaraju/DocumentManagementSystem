namespace Document_Management_System.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ActionDate
        {
            get;
            set;
        } = DateTime.UtcNow;
    }
}