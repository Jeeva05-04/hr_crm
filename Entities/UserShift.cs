public class UserShift
{
    public int Id { get; set; }

    public int UserId { get; set; }   // From auth system

    public int ShiftId { get; set; }

    public Shift Shift { get; set; }
}