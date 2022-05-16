using System.Globalization;

namespace SmartHeater.ML;

internal class MLDataGenerator
{
    private readonly string _weatherDataFilePath;

    public MLDataGenerator(string weatherDataFilePath)
    {
        _weatherDataFilePath = weatherDataFilePath;
    }

    //Heater parameters with preset values.
    private bool _powerState = false;
    private double _roomTemp = MLContants.TrainingStartRoomTemperature;

    //Weather loading helper variables.
    private int _lastDayNumber = 0;
    private double _thisDayTemp = double.NaN;

    //Other environment and helper variables.
    private DateTime _time = new(MLContants.TrainingStartYear, 1, 1);
    private readonly Random _random = new();

    public async Task RunAsync()
    {
        Console.WriteLine("Generating ML training and testing csv files...");

        //Load weather data.
        using var weatherFile = await GetWeatherFileReaderAsync(_weatherDataFilePath);

        //Create generated files and write csv file headers.
        using var trainingFile = new StreamWriter(MLContants.TrainingFileName);
        using var testingFile = new StreamWriter(MLContants.TestingFileName);

        var headers = "dateTime;temperatureDiff;turnedOn";
        await trainingFile.WriteLineAsync(headers);
        await testingFile.WriteLineAsync(headers);

        //Start the main loop until full end year.
        while (_time.Year <= MLContants.TrainingEndYear)
        {
            //Get reference room temperature and next heater status.
            var refTemp = ReferenceTemperature();
            (var shouldBeOn, var weatherFactorUsed) = HeaterOn(refTemp);

            //Check for day change, read weather value.
            if (_lastDayNumber != _time.Day)
            {
                _lastDayNumber = _time.Day;
                _thisDayTemp = await LoadNextWeatherAsync(weatherFile);
            }

            //Write heater status to the csv.
            var line = $"{_time};{(_roomTemp - refTemp).ToString().Replace(',', '.')};{shouldBeOn}";

            var fileRef = _time.Year < MLContants.TrainingEndYear ? trainingFile : testingFile;
            await fileRef.WriteLineAsync(line);

            //Update heater power status and room temperature.
            var oldPowerState = _powerState;
            _powerState = Convert.ToBoolean(shouldBeOn);

            //Room temperature is updated differently for cases:
            //I.  High weather temperature (for example heater is turned off in summer).
            //II. Heater status is acknowledged if power state did not change from last measurement.
            if (weatherFactorUsed)
            {
                _roomTemp += (_random.Next(0, 100) / 10000.0) * (_time.Minute % 2 == 0 && _roomTemp >= refTemp ? -1 : 1);
            }
            else if (oldPowerState == _powerState)
            {
                _roomTemp += (_powerState ? _random.Next(8, 35) : _random.Next(-16, -1)) / 100.0;
            }
            else
            {
                _roomTemp += (_powerState ? _random.Next(10, 30) : _random.Next(-30, -10)) / 1000.0;
            }
            //Advance time.
            _time = _time.AddMinutes(1);
        }
        //End of main loop.
        Console.WriteLine("Generating of csv files is finished.");
    }

    //Returns if heater should be on and if weather factor should be used to update temperature.
    private (int, bool) HeaterOn(double refTemp)
    {
        var weatherFactor = Math.Pow(Math.Clamp(_thisDayTemp, 0, 21.5), 2) / (21.5 * 21.5);
        if (weatherFactor > _random.Next(29, 39) / 100.0)
        {
            return (0, true);
        }
        return _powerState
            ? (_roomTemp <= refTemp + 0.12 ? 1 : 0, false)
            : (_roomTemp <= refTemp - 0.21 ? 1 : 0, false);
    }

    //Returns reference temperature based on time period of the day.
    private double ReferenceTemperature() => (_time.Hour >= 23 && _time.Minute >= 30) || _time.Hour <= 5
        ? 21.5 //night
        : 23.1;//day

    //Loads weather data file and moves pointer to the start of the required year.
    private static async Task<StreamReader> GetWeatherFileReaderAsync(string path)
    {
        var sr = new StreamReader(path);
        string? line;
        do
        {
            line = await sr.ReadLineAsync();
        }
        while (line is null || !line.StartsWith($"{MLContants.TrainingStartYear - 1};12;31;"));
        return sr;
    }

    //Moves weather file by one line and returns the temperature value from that line.
    private async Task<double> LoadNextWeatherAsync(StreamReader weatherFile)
    {
        int day;
        string[]? line; //year;month;day;temperature;
        do
        {
            line = (await weatherFile.ReadLineAsync())?.Split(';');
            if (line is null || line.Length < 4)
            {
                return double.NaN;
            }
            day = Convert.ToInt32(line[2]);
        }
        //Loop should execute only once
        //(but it's possible to encounter previous day for some reason).
        while (day != _lastDayNumber);

        return Convert.ToDouble(line[3].Replace(',', '.'), CultureInfo.InvariantCulture);
    }
}
