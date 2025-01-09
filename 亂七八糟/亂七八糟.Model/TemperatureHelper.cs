namespace 亂七八糟.Model;

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