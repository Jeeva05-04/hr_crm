public class AttendanceTracking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string IpAddress { get; set; }

    public string DeviceInfo { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }
}