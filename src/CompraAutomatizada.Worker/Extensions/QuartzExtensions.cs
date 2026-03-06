using CompraAutomatizada.Worker.Jobs;
using Quartz;

namespace CompraAutomatizada.Worker.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddCompraQuartz(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("CompraJob");

            q.AddJob<CompraJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("CompraJob-trigger")
                .WithCronSchedule("0 0 18 ? * MON-FRI"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
