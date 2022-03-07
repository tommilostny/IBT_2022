const int startYear = 2019;
const int endYear = 2020;

//Heater parameters with preset values.
var powerState = false;
var roomTemp = 18.5;

//Weather loading helper variables.
int lastDayNumber = 0;
double thisDayTemp = double.NaN;

//Other environment and helper variables.
var time = new DateTime(startYear, 1, 1);
var random = new Random();

//Load weather data.
using var weatherFile = await GetWeatherFileReaderAsync(@"C:\Users\tommi\Downloads\B2MBUD01_T_N.csv\B2MBUD01_T_N.csv");

//Create generated file and write csv file headers.
using var generatedFile = new StreamWriter("training.csv");
await generatedFile.WriteLineAsync("date_time;temperature;weather;turned_on");

//Start the main loop until full end year.
while (time.Year <= endYear)
{
    //Get reference room temperature and next heater status.
    var refTemp = ReferenceTemperature();
    (var shouldBeOn, var weatherFactorUsed) = HeaterOn(refTemp);

    //Check for day change, read weather value.
    if (lastDayNumber != time.Day)
    {
        lastDayNumber = time.Day;
        thisDayTemp = await LoadNextWeatherAsync();
    }

    //Write heater status to the csv.
    await generatedFile.WriteLineAsync($"{time};{roomTemp};{thisDayTemp};{shouldBeOn}");

    //Update heater power status and room temperature.
    var oldPowerState = powerState;
    powerState = Convert.ToBoolean(shouldBeOn);

    //Room temperature is updated differently for cases:
    //I.  High weather temperature (for example heater is turned off in summer).
    //II. Heater status is acknowledged if power state did not change from last measurement.
    if (weatherFactorUsed)
    {
        roomTemp += (random.Next(0, 100) / 10000.0) * (time.Minute % 2 == 0 && roomTemp >= refTemp ? -1 : 1);
    }
    else if (oldPowerState != powerState)
    {
        roomTemp += (powerState ? random.Next(1, 35) : random.Next(-10, -1)) / 100.0;
    }
    //Advance time.
    time = time.AddMinutes(1);
}
//End of main loop.


//Returns if heater should be on and if weather factor should be used to update temperature.
(int, bool) HeaterOn(double refTemp)
{
    var weatherFactor = Math.Pow(Math.Clamp(thisDayTemp, 0, 21.5), 2) / (21.5 * 21.5);
    if (weatherFactor > random.Next(29, 39) / 100.0)
    {
        return (0, true);
    }
    return powerState
        ? ((roomTemp <= refTemp + 0.16 ? 1 : 0), false)
        : ((roomTemp <= refTemp - 0.23 ? 1 : 0), false);
}

//Returns reference temperature based on time period of the day.
double ReferenceTemperature() => (time.Hour >= 23 && time.Minute >= 30) || time.Hour <= 5
    ? 21.5 //night
    : 23.1;//day

//Loads weather data file and moves pointer to the start of the required year.
async Task<StreamReader> GetWeatherFileReaderAsync(string path)
{
    var sr = new StreamReader(path);
    string? line;
    do
    {
        line = await sr.ReadLineAsync();
    }
    while (line is null || !line.StartsWith($"{startYear - 1};12;31;"));
    return sr;
}

//Moves weather file by one line and returns the temperature value from that line.
async Task<double> LoadNextWeatherAsync()
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
    while (day != lastDayNumber);
    
    return Convert.ToDouble(line[3]);
}
