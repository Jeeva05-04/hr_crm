namespace hr_crm.Entities
{
    public class EmployeeLocationTrail
    {
        public int Id { get; set; }

        public int AttendanceId { get; set; }

        public int UserId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime RecordedAt { get; set; }
    }
}
