namespace SmartHeater.Factories;

public class HeatersFactory
{
    private readonly IDatabaseService _databaseService;
    private readonly HttpClient _httpClient;

    public HeatersFactory(IDatabaseService databaseService, HttpClient httpClient)
    {
        _databaseService = databaseService;
        _httpClient = httpClient;
    }

    public ICollection<IHeaterService> GetHeaters()
    {
        //Read list of IP addresses (strings) from InfluxDb, create ShellyRelayService objects.
        throw new NotImplementedException();
    }
}
