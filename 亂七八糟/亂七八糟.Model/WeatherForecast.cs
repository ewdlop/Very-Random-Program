namespace 亂七八糟.Model;

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
