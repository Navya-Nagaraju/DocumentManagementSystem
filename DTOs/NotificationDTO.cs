namespace Document_Management_System.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
