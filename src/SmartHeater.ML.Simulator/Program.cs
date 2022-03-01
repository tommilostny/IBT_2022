var date = new DateTime(2020, 1, 1);
using var sw = new StreamWriter("training.csv");

await sw.WriteLineAsync("date_time;temperature;turned_on");

for (uint currentTime = 0; date.Year == 2020; currentTime++, date = date.AddMinutes(1))
{
    var roomTemp = RoomTemperature(currentTime);
    var shouldBeOn = HeaterOn(roomTemp);

    await sw.WriteLineAsync($"{date};{roomTemp};{shouldBeOn}");
}

static double RoomTemperature(double time)
{
    if (time == 0)
    {
        time += 0.0000001;
    }
    return -8 * Math.Sin(time / 2) / time;
}

static int HeaterOn(double roomTemperature)
{
    return roomTemperature < 0 ? 1 : 0;
}
