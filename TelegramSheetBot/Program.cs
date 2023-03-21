using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Services;
using TelegramSheetBot.Services.Callbacks;
using TelegramSheetBot.Services.JobWithBd;
using TelegramSheetBot.Services.Qartz;


namespace TelegramSheetBot
{
    class Program
    {
        private IServiceProvider? _service;


        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private string GoogleCredentialsFileName { get; set; }

        public Program()
        {
            _service = ServiceSetting();
        }

        private static void Main() => new Program().AsyncMain().GetAwaiter().GetResult();


        IServiceProvider ServiceSetting()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);


#if DEBUG

            configuration.SetBasePath(GlobalValues.ChatEnvironmentParent);
            GoogleCredentialsFileName = @"D:\пробы\Bots\TelegramSheetBot\TelegramSheetBot\Google.json";
#else
             Console.WriteLine("Release");
             configuration.SetBasePath(Directory.GetCurrentDirectory());
             GoogleCredentialsFileName = $"{Directory.GetCurrentDirectory()}"+@"\Google.json";

#endif


            IConfiguration config = configuration.Build();
            var token = config.GetConnectionString("TelegramToken");
            BaseClientService.Initializer init;

            using (var stream =
                   new FileStream(GoogleCredentialsFileName, FileMode.Open, FileAccess.Read))
            {
                init = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
            }

            var collection = new ServiceCollection()
                .AddDbContext<ApplicationContext>(ServiceLifetime.Transient)
                .AddScoped(_ => { return new TelegramBotClient(token!); })
                
               // .AddTransient(typeof(JobWithBd<>))
                .AddTransient(typeof(IJobWithBd<>),typeof(JobWithBd<>))
                .AddSingleton(new SheetsService(init))
                .AddSingleton<SettingChat>()
                .AddTransient<GoogleSheets>()
                .AddTransient<ManageGroup>()
                .AddTransient<CommandsHandler>()
                .AddTransient<JobFactory>()
                .AddTransient<QuartzService>()
                .AddScoped<StartBot>()
                .AddSingleton<ISchedulerFactory, StdSchedulerFactory>()
                .AddSingleton<DayCallBackService>()
                .AddTransient<PollService>()
                .AddTransient<FindingService>()
                .AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); })
                .AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

            ;
            return collection.BuildServiceProvider();
        }


        async Task AsyncMain()
        {
            Console.WriteLine("1");
            await _service!.GetService<TestService>()!.Quartz();
            await _service!.GetService<StartBot>()!.Init();

            Console.ReadKey();
        }

        //настройка работы бота
    }
}