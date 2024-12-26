using Aspire.Hosting;

namespace 亂七八糟.Tests;

public class WebTests
{
    [Fact]
    // This test is an integration test because it requires a running web "frontend" service;
    // it uses the DistributedApplicationTestingBuilder to create a distributed application with the web frontend service.
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {

        // Integration tests are slower than unit tests, so consider running them in parallel.
        // To learn more about running tests in parallel, see https://aka.ms/dotnet/aspire/parallel-tests
        
       
        // Arrange
        IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.亂七八糟_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

        await using DistributedApplication app = await appHost.BuildAsync();
        ResourceNotificationService resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        // Act
        HttpClient httpClient = app.CreateHttpClient("webfrontend");
        await resourceNotificationService.WaitForResourceAsync("webfrontend", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        HttpResponseMessage response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
