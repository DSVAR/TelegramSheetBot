using Quartz;
using Quartz.Impl;

namespace TelegramSheetBot.Services.Qartz;

public class SettingQuartzService
{
    private ISchedulerFactory _factory;
    private JobFactory _jobFactory;
    
    public SettingQuartzService(ISchedulerFactory factory,JobFactory jobFactory)
    {
        _factory = factory;
        _jobFactory = jobFactory;
    }
    
    public async Task Quartz()
    {
        var schedulerFactory = new StdSchedulerFactory();
        
        var scheduler =await schedulerFactory.GetScheduler();

        scheduler.JobFactory = _jobFactory;
        var schedule = await _factory.GetScheduler();

        schedule.Start().Wait();
        var jobDetail = JobBuilder.Create<QuartzService>()
            .WithIdentity("Test2").Build();

        var trigger = TriggerBuilder.Create().WithIdentity("Test23")
            .WithSimpleSchedule(o => o.RepeatForever()
                .WithIntervalInSeconds(30))
            .StartNow().Build();


        // var scheduler=await schedule.GetScheduler();
        schedule.ScheduleJob(jobDetail, trigger).Wait();

       
       

        Console.WriteLine("test quartz");
    }

}