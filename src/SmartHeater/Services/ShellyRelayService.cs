using SmartHeater.Services.Interfaces;
using System.Net.Http.Json;

namespace SmartHeater.Services;

public class ShellyRelayService : IHeaterService
{
    private readonly HttpClient _httpClient;
    private readonly string _ipAddress;

    public ShellyRelayService(HttpClient httpClient, string ipAddress)
    {
        _httpClient = httpClient;
        _ipAddress = ipAddress;
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
