using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor.Acquire;

public class GitStractorHost : IHost {
    private readonly ILogger<GitStractorHost> _logger;
    private readonly IHostLifetime _hostLifetime;
    private readonly ApplicationLifetime _applicationLifetime;
    private readonly HostOptions _options;
    private IEnumerable<IHostedService>? _hostedServices;

    public GitStractorHost(IServiceProvider services, IHostApplicationLifetime applicationLifetime, ILogger<GitStractorHost> logger,
        IHostLifetime hostLifetime, IOptions<HostOptions> options) {
        Services = services;
        _applicationLifetime = (ApplicationLifetime)applicationLifetime;
        _logger = logger;
        _hostLifetime = hostLifetime;
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public IServiceProvider Services { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default) {
        _logger.LogInformation("Starting");

        await _hostLifetime.WaitForStartAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        _hostedServices = Services.GetService<IEnumerable<IHostedService>>();

        foreach (var hostedService in _hostedServices) {
            // Fire IHostedService.Start
            await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        // Fire IApplicationLifetime.Started
        _applicationLifetime?.NotifyStarted();

        _logger.LogInformation("Started");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default) {
        _logger.LogInformation("Stopping");

        using (var cts = new CancellationTokenSource(_options.ShutdownTimeout))
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken)) {
            var token = linkedCts.Token;
            // Trigger IApplicationLifetime.ApplicationStopping
            _applicationLifetime?.StopApplication();

            IList<Exception> exceptions = new List<Exception>();
            if (_hostedServices != null) // Started?
            {
                foreach (var hostedService in _hostedServices.Reverse()) {
                    token.ThrowIfCancellationRequested();
                    try {
                        await hostedService.StopAsync(token).ConfigureAwait(false);
                    }
                    catch (Exception ex) {
                        exceptions.Add(ex);
                    }
                }
            }

            token.ThrowIfCancellationRequested();
            await _hostLifetime.StopAsync(token);

            // Fire IApplicationLifetime.Stopped
            _applicationLifetime?.NotifyStopped();

            if (exceptions.Count > 0) {
                var ex = new AggregateException("One or more hosted services failed to stop.", exceptions);
                _logger.LogCritical("Stopped with exception");
                throw ex;
            }
        }

        _logger.LogInformation("Stopped");
    }

    public void Dispose() {
        (Services as IDisposable)?.Dispose();
    }
}
