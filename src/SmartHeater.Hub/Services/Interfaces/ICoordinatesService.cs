namespace SmartHeater.Hub.Services.Interfaces;

public interface ICoordinatesService
{
    Task<(double?, double?)> GetLatitudeLongitude();
}
