using EnglishCenter.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnglishCenter.Infrastructure.Jobs;

/// <summary>Định kỳ sinh buổi học 30 ngày tới từ lịch tuần.</summary>
public class LessonSessionGeneratorJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LessonSessionGeneratorJob> _logger;

    public LessonSessionGeneratorJob(IServiceScopeFactory scopeFactory, ILogger<LessonSessionGeneratorJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var generator = scope.ServiceProvider.GetRequiredService<ILessonSessionGeneratorService>();
                await generator.GenerateAsync(cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lesson session generation failed.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
