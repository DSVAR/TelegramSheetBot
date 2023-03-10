using Quartz;
using Quartz.Spi;

namespace TelegramSheetBot.Services.Qartz;

public class JobFactory:IJobFactory
{
     readonly IServiceProvider _container;

    public JobFactory(IServiceProvider container)
    {
        _container = container;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return (_container.GetService(bundle.JobDetail.JobType) as IJob)!;
    }

    public void ReturnJob(IJob job)
    {
    }
}