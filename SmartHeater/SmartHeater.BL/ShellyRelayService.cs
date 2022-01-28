namespace SmartHeater.BL;

public class ShellyRelayService : IHeaterService
{
    private readonly HttpClient _httpClient;

    public ShellyRelayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public double ReadTemperature()
    {
        throw new NotImplementedException();
    }

    public void TurnOff()
    {
        throw new NotImplementedException();
    }

    public void TurnOn()
    {
        throw new NotImplementedException();
    }
}
