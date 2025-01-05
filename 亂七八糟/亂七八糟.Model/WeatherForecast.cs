namespace 亂七八糟.Model;


public class WeatherForcecastClass(DateOnly Date, double TemperatureK, string? Summary)
{
    public DateOnly Date { get; init; } = Date;
    public double TemperatureK { get; init; } = TemperatureK;
    public string? Summary { get; init; } = Summary;
}


public class WeatherForcecastRecord(DateOnly Date, double TemperatureK, string? Summary)
{
    public DateOnly Date { get; protected set; } = Date;
    public double TemperatureK { get; protected set; } = TemperatureK;
    public string? Summary { get; protected set; } = Summary;
}


public record WeatherForecast(DateOnly Date, double TemperatureK, string? Summary);

public record WeatherForecastC(DateOnly Date, double TemperatureC, string? Summary) : WeatherForecast(Date, TemperatureC + 273.15, Summary);

public record WeatherForecastF(DateOnly Date, double TemperatureF, string? Summary) : WeatherForecastC(Date, (TemperatureF - 32) * 5.0 / 9, Summary);

public readonly record struct ValueTemperature
{
    public int Value { get; init; }
    public TemperatureUnit TemperatureUnit { get; init; }


    public ValueTemperature(int value, TemperatureUnit temperatureUnit)
    {
        Value = value;
        TemperatureUnit = temperatureUnit;
    }

    public double ToCelsius() => TemperatureUnit switch
    {
        TemperatureUnit.Kelvin => Value - 273.15,
        TemperatureUnit.Fahrenheit => (Value - 32) * 5.0 / 9,
        _ => Value
    };

    public double ToFahrenheit() => TemperatureUnit switch
    {
        TemperatureUnit.Kelvin => (Value - 273.15) * 9.0 / 5 + 32,
        TemperatureUnit.Celsius => Value * 9.0 / 5 + 32,
        _ => Value
    };

    public double ToKelvin() => TemperatureUnit switch
    {
        TemperatureUnit.Celsius => Value + 273.15,
        TemperatureUnit.Fahrenheit => (Value - 32) * 5.0 / 9 + 273.15,
        _ => Value
    };
}

public enum TemperatureUnit
{
    Kelvin,
    Celsius,
    Fahrenheit
}

public static class TemperatureHelper
{
    public static double ToCelsius(this double temperature, TemperatureUnit temperatureUnit) => temperatureUnit switch
    {
        TemperatureUnit.Kelvin => temperature - 273.15,
        TemperatureUnit.Fahrenheit => (temperature - 32) * 5.0 / 9,
        _ => temperature
    };

    public static double ToFahrenheit(this double temperature, TemperatureUnit temperatureUnit) => temperatureUnit switch
    {
        TemperatureUnit.Kelvin => (temperature - 273.15) * 9.0 / 5 + 32,
        TemperatureUnit.Celsius => temperature * 9.0 / 5 + 32,
        _ => temperature
    };

    public static double ToKelvin(this double temperature, TemperatureUnit temperatureUnit) => temperatureUnit switch
    {
        TemperatureUnit.Celsius => temperature + 273.15,
        TemperatureUnit.Fahrenheit => (temperature - 32) * 5.0 / 9 + 273.15,
        _ => temperature
    };
}