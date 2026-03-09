namespace hr_crm.Entities
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int UserId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }

        public string? Status { get; set; }
    }
}