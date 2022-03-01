//Heater parameters with preset values.
var powerState = false;
var roomTemp = 18.5;
bool switchFlag;

//Other environment and helper variables.
var date = new DateTime(2020, 1, 1);
var random = new Random();
using var sw = new StreamWriter("training.csv");

//Write csv file headers.
await sw.WriteLineAsync("date_time;temp_diff;turned_on");

//Start the main loop for year 2020.
while (date.Year == 2020)
{
    //Get reference room temperature and next heater status.
    var refTemp = ReferenceTemperature();
    var shouldBeOn = HeaterOn(refTemp);

    //Write heater status to the csv.
    await sw.WriteLineAsync($"{date};{roomTemp - refTemp};{shouldBeOn}");

    //Update heater power status and room temperature.
    var oldPowerState = powerState;
    powerState = Convert.ToBoolean(shouldBeOn);
    if (!(switchFlag = oldPowerState != powerState))
    {
        roomTemp += (powerState ? random.Next(1, 3) : random.Next(-1, 0)) / 10.0;
        roomTemp += (powerState ? random.Next(-1, 4) : random.Next(-6, -1)) / 100.0;
    }

    //Advance time.
    date = date.AddMinutes(1);
}

int HeaterOn(double refTemp) => powerState
    ? (roomTemp <= refTemp + 0.16 ? 1 : 0)
    : (roomTemp <= refTemp - 0.23 ? 1 : 0);

double ReferenceTemperature() => (date.Hour >= 23 && date.Minute >= 30) || date.Hour <= 5
    ? 21.5 //night
    : 23.1;//day
