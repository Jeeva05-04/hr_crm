namespace hr_crm.Service.Interface
{
    public interface IIpGeolocationService
    {
        Task<(double? Latitude, double? Longitude, string? City, string? Country)> GetLocationAsync(string ipAddress);
    }
}
