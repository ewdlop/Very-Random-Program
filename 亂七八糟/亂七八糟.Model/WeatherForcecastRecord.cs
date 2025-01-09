namespace 亂七八糟.Model;

public class WeatherForcecastRecord(DateOnly Date, double TemperatureK, string? Summary)
{
    public DateOnly Date { get; protected set; } = Date;
    public double TemperatureK { get; protected set; } = TemperatureK;
    public string? Summary { get; protected set; } = Summary;
}