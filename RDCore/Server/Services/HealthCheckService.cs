using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RDCore.Server.Services;

internal interface IHealthCheckService : IDisposable
{
    void Start(Action handleUnhealthyClient, long? clientProcessId, int healthCheckIntervalSeconds);
    void Pause();
    void Resume();
}

internal class HealthCheckService : IHealthCheckService
{
    private readonly Timer _timer;
    private readonly ILogger _logger;

    private Action? _handleUnhealthyClient;
    private TimeSpan _interval;

    private bool _didNotify;
    private Process? _process;

    public HealthCheckService(ILoggerFactory loggerFactory)
    {
        _timer = new Timer(CheckClientProcessHealth, null, Timeout.Infinite, Timeout.Infinite);
        _logger = loggerFactory.CreateLogger<HealthCheckService>();
    }

    public void Start(Action handleUnhealthyClient, long? clientProcessId, int healthCheckIntervalSeconds)
    {
        if (!clientProcessId.HasValue)
        {
            throw new ArgumentNullException(nameof(clientProcessId));
        }
        if (clientProcessId.Value > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(clientProcessId));
        }

        _process = Process.GetProcessById((int)clientProcessId.Value);

        _didNotify = false;
        _handleUnhealthyClient = handleUnhealthyClient;
        _interval = TimeSpan.FromSeconds(healthCheckIntervalSeconds);

        _logger.LogInformation("Starting health check service (Interval: {TotalSeconds} seconds)", _interval.TotalSeconds);
        _timer.Change(TimeSpan.Zero, _interval);
    }

    public void Pause()
    {
        if (!_didNotify && _handleUnhealthyClient is not null)
        {
            _logger.LogInformation("Pausing health check service...");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void Resume()
    {
        if (!_didNotify && _handleUnhealthyClient is not null)
        {
            _logger.LogInformation("Resuming health check service...");
            _timer.Change(TimeSpan.Zero, _interval);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        if (_timer is IDisposable disposableTimer)
        {
            disposableTimer.Dispose();
        }
        if (_process is IDisposable disposableProcess)
        {
            disposableProcess.Dispose();
        }
    }

    private void CheckClientProcessHealth(object? state)
    {
        if (_process!.HasExited)
        {
            _logger.LogWarning("Client process (ID:{ProcessId}) has exited", _process!.Id);
            OnUnhealthyClientDetected();
        }
    }

    private void OnUnhealthyClientDetected()
    {
        if (!_didNotify)
        {
            _logger.LogCritical("Client process health check failed; server process will be terminated.");

            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            _didNotify = true;

            _handleUnhealthyClient?.Invoke();
        }
    }
}