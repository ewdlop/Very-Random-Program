using 亂七八糟.Model;

namespace 亂七八糟.Web.Components.Pages;

public partial class Weather
{
    public WeatherForecast[] Forecasts { get; private set;} = [];

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherApi.GetWeatherAsync();
    }
}