namespace 亂七八糟.Model;

public class WeatherForecastClass(DateOnly Date, double TemperatureK, string? Summary)
{
    public DateOnly Date { get; init; } = Date;
    public double TemperatureK { get; init; } = TemperatureK;
    public string? Summary { get; init; } = Summary;
}



