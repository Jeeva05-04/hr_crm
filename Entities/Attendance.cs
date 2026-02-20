public class Attendance
{
    public int AttendanceId { get; set; }
    public int UserId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public string? Status { get; set; }
}
