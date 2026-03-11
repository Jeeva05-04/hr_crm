namespace hr_crm.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Module { get; set; }

        public int ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}