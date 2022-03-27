namespace SmartHeater.Hub.Services.Interfaces;

public interface IHeatersRepositoryService
{
    Task<HeaterListModel> GetHeaterAsync(string ipAddress);
    Task<HeaterDetailModel?> GetHeaterDetailAsync(string ipAddress);

    Task<ICollection<HeaterListModel>> ReadHeatersAsync();
    Task WriteHeatersAsync(ICollection<HeaterListModel> heaters);
    
    Task<ICollection<IHeaterControlService>> GetHeaterServicesAsync();
    Task<IHeaterControlService?> GetHeaterServiceAsync(string ipAddress);
    IHeaterControlService? GetHeaterService(HeaterListModel heater);

    Task<ICollection<HeaterListModel>> InsertAsync(HeaterListModel heater);
    Task<ICollection<HeaterListModel>> UpdateAsync(string originalIpAddress, HeaterListModel heater);
    Task<ICollection<HeaterListModel>> DeleteAsync(string ipAddress);
}
