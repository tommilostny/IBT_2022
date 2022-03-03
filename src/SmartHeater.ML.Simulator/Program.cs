//Heater parameters with preset values.
var powerState = false;
var roomTemp = 18.5;
bool heaterPowerSwitchFlag;

//Weather loading helper variables.
int lastDayNumber = 0;
double thisDayTemp = double.NaN;

//Other environment and helper variables.
var time = new DateTime(2020, 1, 1);
var random = new Random();

//Load weather data.
using var weatherFile = await GetWeatherDataStreamReader(@"C:\Users\tommi\Downloads\B2MBUD01_T_N.csv\B2MBUD01_T_N.csv");

//Create generated file and write csv file headers.
using var generatedFile = new StreamWriter("training.csv");
await generatedFile.WriteLineAsync("date_time;temperature;weather;turned_on");

//Start the main loop for year 2020.
while (time.Year == 2020)
{
    //Get reference room temperature and next heater status.
    var refTemp = ReferenceTemperature();
    var shouldBeOn = HeaterOn(refTemp);

    //Check for day change, read weather value.
    if (lastDayNumber != time.Day)
    {
        lastDayNumber = time.Day;
        thisDayTemp = await LoadNextWeatherTemperature();
    }

    //Write heater status to the csv.
    await generatedFile.WriteLineAsync($"{time};{roomTemp};{thisDayTemp};{shouldBeOn}");

    //Update heater power status and room temperature.
    var oldPowerState = powerState;
    powerState = Convert.ToBoolean(shouldBeOn);
    if (!(heaterPowerSwitchFlag = oldPowerState != powerState))
    {
        roomTemp += (powerState ? random.Next(1, 35) : random.Next(-10, 0)) / 100.0;
    }
    //Advance time.
    time = time.AddMinutes(1);
}
//End of main loop.


int HeaterOn(double refTemp) => powerState
    ? (roomTemp <= refTemp + 0.16 ? 1 : 0)
    : (roomTemp <= refTemp - 0.23 ? 1 : 0);

double ReferenceTemperature() => (time.Hour >= 23 && time.Minute >= 30) || time.Hour <= 5
    ? 21.5 //night
    : 23.1;//day

async Task<StreamReader> GetWeatherDataStreamReader(string path)
{
    var sr = new StreamReader(path);
    string? line;
    do
    {
        line = await sr.ReadLineAsync();
    }
    while (line is null || !line.StartsWith("2019;12;31;"));
    return sr;
}

async Task<double> LoadNextWeatherTemperature()
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
    while (day != lastDayNumber);
    
    return Convert.ToDouble(line[3]);
}
