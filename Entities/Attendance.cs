namespace hr_crm.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string? Status { get; set; }

        public string? IPAddress { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? DeviceInfo { get; set; }
    }
}