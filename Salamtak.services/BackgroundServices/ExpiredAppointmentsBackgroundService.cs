using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Salamtak.services.Abstractions.Interfaces_Services;

namespace Salamtak.services.BackgroundServices
{
    public class ExpiredAppointmentsBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredAppointmentsBackgroundService> _logger;

        public ExpiredAppointmentsBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExpiredAppointmentsBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var appointmentService =
                        scope.ServiceProvider.GetRequiredService<IAppointmentService>();

                    var cancelledCount =
                        await appointmentService.CancelExpiredPendingAppointmentsAsync();

                    if (cancelledCount > 0)
                    {
                        _logger.LogInformation(
                            "Cancelled {Count} expired pending appointments.",
                            cancelledCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while cancelling expired pending appointments.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(15),
                    stoppingToken);
            }
        }
    }
}