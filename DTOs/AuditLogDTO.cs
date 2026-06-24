namespace Document_Management_System.DTOs
{
    public class AuditLogDTO
    {
        public string Action { get; set; }

        public string Description { get; set; }

        public DateTime ActionDate { get; set; }
    }
}