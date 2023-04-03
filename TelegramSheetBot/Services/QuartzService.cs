using Quartz;
using Telegram.Bot;
using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class QuartzService : IJob
{
    private readonly TelegramBotClient _client;
    private readonly GoogleSheets _sheets;
    private readonly IJobWithBd<StructureChat> _jobWithBdStructureChat;
    private readonly SettingChat _settingChat;
    private readonly PollService _pollService;

    public QuartzService(TelegramBotClient client, GoogleSheets sheets,
        IJobWithBd<StructureChat> jobWithBdStructureChat,
        SettingChat settingChat, PollService pollService)
    {
        _client = client;
        _sheets = sheets;
        _jobWithBdStructureChat = jobWithBdStructureChat;
        _settingChat = settingChat;
        _pollService = pollService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await GetFiles();
    }

    /// <summary>
    /// получение всех чатов
    /// </summary>
    private async Task GetFiles()
    {
        try
        {
            var listOfChats = await _jobWithBdStructureChat.GetItemsAsync();
            foreach (var item in listOfChats)
            {
                if (!item.FirstSetSettings)
                {
                    await _settingChat.CheckUpdate(item.ChatId);
                    await CheckChat(item);
                    await CheckStartPoll(item);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// проверка чатов на изменение времени
    /// </summary>
    /// <param name="item"></param>
    private async Task CheckChat(StructureChat item)
    {
        try
        {
            //проверяем если с последнего опроса прошло больше 6 дней то обнуляем значения времени 
            if ((DateTime.Now - item.LastChangeTime).Days >= 6)
            {
                bool cycle = true;

                item.CreatedPollThisWeek = false;
                item.TimeStartPoll = "";
                item.TimeEndPoll = "";


                while (cycle)
                {
                    if (string.IsNullOrEmpty(item.TimeStartPoll))
                    {
                        item = RandomTime(item);
                    }
                    else
                    {
                        if (TimeSpan.Parse(item.TimeStartPoll!) >= TimeSpan.Parse(item.TimeEndPoll!) &&
                            TimeSpan.Parse(item.TimeIntervalEnd!) <= TimeSpan.Parse(item.TimeEndPoll!)
                            && TimeSpan.Parse(item.TimeStartPoll) > TimeSpan.Parse(item.TimeEndPoll!))
                        {
                            item = RandomTime(item);
                        }
                        else
                        {
                            cycle = false;
                        }
                    }
                }

                await _jobWithBdStructureChat.Update(item);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message + " CheckChat");
        }
    }

    /// <summary>
    /// запуск/закрытие голосования
    /// </summary>
    /// <param name="item"></param>
    private async Task CheckStartPoll(StructureChat item)
    {
        try
        {
            var today = DateTime.Now;

            today = today.AddHours(-today.Hour);
            today = today.AddMinutes(-today.Minute);
            today = today.AddSeconds(-today.Second);

            DateTime timeStart = default;
            DateTime timeEnd = default;

            if (!string.IsNullOrEmpty(item.TimeStartPoll) && !string.IsNullOrEmpty(item.TimeEndPoll))
            {
                timeStart = today + TimeSpan.Parse(item.TimeStartPoll!);
                timeEnd = today + TimeSpan.Parse(item.TimeEndPoll!);
            }

            if (today.DayOfWeek.ToString() == item.DayOfWeekStartPoll)
            {
                if (DateTime.Now > timeStart && !item.CreatedRequestPoll && timeStart.Ticks != 0 &&
                    !item.CreatedPollThisWeek)
                {
                    item.CreatedRequestPoll = true;


                    var googleList = (await _sheets.ListForPoll(item.GoogleSheetToken!)).Select(x => x.Name);

                    var message = await _client.SendPollAsync(item.ChatId, "голосование", googleList!,
                        allowsMultipleAnswers: true, isAnonymous: false);

                    item.CreatedPoll = true;
                    item.IdMessageLastPoll = message.MessageId;
                    item.PollId = message.Poll!.Id;


                    var hourEnd = int.Parse(item.TimeIntervalEnd!.Remove(item.TimeIntervalEnd!.IndexOf(':')));
                    int minuteEnd = int.Parse(item.TimeIntervalEnd!.Substring(item.TimeIntervalEnd.IndexOf(':') + 1));
                    today = today.AddHours(hourEnd);
                    today = today.AddMinutes(minuteEnd);
                    if (DateTime.Now > today)
                    {
                        var now = DateTime.Now;
                        now = now.AddHours(1);

                        item.TimeEndPoll = now.Hour + ":" + now.Minute;
                    }

                    item.StartedPollForced = false;
                    await _jobWithBdStructureChat.Update(item);
                    return;
                }
            }


            //закрытие голосование стандарный/вызванный насильно
            if ((today.DayOfWeek.ToString() == item.DayOfWeekEndPoll && DateTime.Now > timeEnd &&
                 item is { CreatedRequestPoll: true, CreatedPoll: true, StartedPollForced: false })
                || (item.StartedPollForced && DateTime.Now > item.TimeEndForcePoll &&
                    item.TimeEndForcePoll != new DateTime()))
            {
                await _pollService.EndPoll(item.ChatId);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message + " CheckStartPoll");
        }
    }

    /// <summary>
    /// рандомное время для начала и окончания голосования
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    private StructureChat RandomTime(StructureChat chat)
    {
        try
        {
            var hourStart = int.Parse(chat.TimeIntervalStart!.Remove(chat.TimeIntervalStart.IndexOf(':')));

            var hourEnd = int.Parse(chat.TimeIntervalEnd!.Remove(chat.TimeIntervalEnd!.IndexOf(':')));

            int randomHourStart = hourStart;
            int randomHourEnd = hourEnd;

            int randomMinuteStart =
                int.Parse(chat.TimeIntervalStart!.Substring(chat.TimeIntervalStart.IndexOf(':') + 1));
            int randomMinuteEnd = int.Parse(chat.TimeIntervalEnd!.Substring(chat.TimeIntervalEnd.IndexOf(':') + 1));

            if (hourEnd - hourStart > 3)
            {
                randomHourStart = new Random().Next(hourStart + 1, hourEnd - 1);
                randomHourEnd = new Random().Next(hourStart + 1, hourEnd - 1);

                randomMinuteStart = new Random().Next(0, 59);
                randomMinuteEnd = new Random().Next(0, 59);
            }


            chat.TimeStartPoll = randomHourStart + ":" + randomMinuteStart;
            chat.TimeEndPoll = randomHourEnd + ":" + randomMinuteEnd;
            chat.LastChangeTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);


            return chat;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}