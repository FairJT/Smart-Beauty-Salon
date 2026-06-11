using Hangfire;
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

/// <summary>
/// Hangfire-based job dispatcher for domain events.
/// Replaces the BackgroundService-based approach with Hangfire for better reliability.
/// </summary>
public interface IJobDispatcher
{
    void Enqueue<TJob>(TJob job) where TJob : IJob;
    void Enqueue<TJob>(TJob job, TimeSpan delay) where TJob : IJob;
}

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public class HangfireJobDispatcher : IJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobDispatcher(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void Enqueue<TJob>(TJob job) where TJob : IJob
    {
        _backgroundJobClient.Enqueue<IJobExecutor<TJob>>(x => x.ExecuteAsync(job, CancellationToken.None));
    }

    public void Enqueue<TJob>(TJob job, TimeSpan delay) where TJob : IJob
    {
        _backgroundJobClient.Schedule<IJobExecutor<TJob>>(x => x.ExecuteAsync(job, CancellationToken.None), delay);
    }
}

public interface IJobExecutor<TJob> where TJob : IJob
{
    Task ExecuteAsync(TJob job, CancellationToken cancellationToken);
}

public class JobExecutor<TJob> : IJobExecutor<TJob> where TJob : IJob
{
    private readonly TJob _job;

    public JobExecutor(TJob job)
    {
        _job = job;
    }

    public Task ExecuteAsync(TJob job, CancellationToken cancellationToken)
    {
        return job.ExecuteAsync(cancellationToken);
    }
}
