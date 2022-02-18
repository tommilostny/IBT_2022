namespace SmartHeater.Services.Interfaces;

public interface ICoordinatesService
{
    Task<(double?, double?)> GetLatitudeLongitude();
}
