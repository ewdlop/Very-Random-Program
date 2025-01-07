using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace 亂七八糟.BackgroundServices;

///<summary>
///<see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=netcore-cli">Hosted services</see>
///</summary>
public class TimedHostedService(ILogger<TimedHostedService> logger) : IHostedService, IDisposable
{
    protected int executionCount = 0;
    protected readonly ILogger<TimedHostedService> _logger = logger;
    protected Timer? _timer = null;

    public virtual Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, stoppingToken, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    protected virtual void DoWork(object? state)
    {
        if (state is CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
                return;
            }
        }
        int count = Interlocked.Increment(ref executionCount);

        _logger.LogInformation(
            "Timed Hosted Service is working. Count: {Count}", count);
    }

    public virtual Task StopAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Cancellation token is not requested?");
        }

        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}