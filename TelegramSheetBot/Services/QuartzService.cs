using Quartz;
using Telegram.Bot;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class QuartzService : IJob
{
    private readonly TelegramBotClient _client;
    private readonly GoogleSheets _sheets;
    private readonly JobWithBd<StructureChat> _jobWithBdStructureChat;
    private readonly JobWithBd<PollOptions> _jobWithBdOptions;
    private readonly SettingChat _settingChat;

    public QuartzService(TelegramBotClient client, GoogleSheets sheets, 
        JobWithBd<StructureChat> jobWithBdStructureChat,JobWithBd<PollOptions> jobWithBdOptions
        ,SettingChat settingChat)
    {
        _client = client;
        _sheets = sheets;
        _jobWithBdStructureChat = jobWithBdStructureChat;
        _jobWithBdOptions = jobWithBdOptions;
        _settingChat = settingChat;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await GetFiles();
    }


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

    private async Task CheckChat(StructureChat item)
    {
        try
        {
            //проверяем если с последнего опроса прошло больше 6 дней то обнуляем значения времени 
            if ((DateTime.Now - item.LastChangeTime).Days >= 6 )
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
                            TimeSpan.Parse(item.TimeIntervalEnd!)<= TimeSpan.Parse(item.TimeEndPoll!) 
                            && TimeSpan.Parse(item.TimeStartPoll)>TimeSpan.Parse(item.TimeEndPoll!))
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

            if (today.DayOfWeek.ToString() == item.DayOfWeekStartPoll )
            {
                if (DateTime.Now > timeStart && !item.CreatedRequestPoll && timeStart.Ticks != 0 && !item.CreatedPollThisWeek)
                {
                    item.CreatedRequestPoll = true;


                    var googleListSecond = (await _sheets.ListForPoll(item.GoogleSheetToken!)).Select(x=>x.Name);

                    var message = await _client.SendPollAsync(item.ChatId, "голосование", googleListSecond!,
                        allowsMultipleAnswers: true, isAnonymous: false);

                    item.CreatedPoll = true;
                    item.IdMessageLastPoll = message.MessageId;
                    item.PollId = message.Poll!.Id;
                    
                    
                    var hourEnd = int.Parse(item.TimeIntervalEnd!.Remove(item.TimeIntervalEnd!.IndexOf(':')));
                    int minuteEnd = int.Parse(item.TimeIntervalEnd!.Substring(item.TimeIntervalEnd.IndexOf(':')+1));
                    today = today.AddHours(hourEnd);
                    today = today.AddMinutes(minuteEnd);
                    if (DateTime.Now > today)
                    {
                        var now= DateTime.Now;
                        now = now.AddHours(1);

                        item.TimeEndPoll = now.Hour + ":" + now.Minute;
                    }

                    await  _jobWithBdStructureChat.Update(item);
                    return;
                }
            }
            
            if (today.DayOfWeek.ToString() == item.DayOfWeekEndPoll && DateTime.Now > timeEnd &&
                item is { CreatedRequestPoll: true, CreatedPoll: true })
            {
                //закрытие голосование
                item.CreatedRequestPoll = false;
                item.CreatedPoll = false;
                
                item.CreatedPollThisWeek = true;

                item.TimeEndPoll = "";
                item.TimeStartPoll = "";

                var options =(await _jobWithBdOptions.GetItemsAsync()).Where(i=>i.ChatId==item.ChatId).ToList();

                if (options.Count > 0)
                {
                    var max=options.Max(i => i.VoterCount);
                    var listMax = (options.Where(i => i.VoterCount == max) ).ToList();

                    var rnd = new Random();
                    string name="";
                
                    if (listMax.Count() > 1)
                    {
                        var ind = rnd.Next(listMax.Count);
                        name = listMax[ind].Name!;
                    }

                    if (listMax.Count == 1)
                    {
                        name = listMax.First().Name!;
                    }
                    await _sheets.BanSystem(item.GoogleSheetToken!, name);


                    await  _jobWithBdStructureChat.Update(item);

                    await _client.StopPollAsync(item.ChatId, item.IdMessageLastPoll);
                    await _client.SendTextMessageAsync(item.ChatId, $"закрытие голосования! Победитель:{name}");
                  
                    
                    await _jobWithBdOptions.DeleteRangeAsync(options);
                }
                
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
        
            int randomMinuteStart = int.Parse(chat.TimeIntervalStart!.Substring(chat.TimeIntervalStart.IndexOf(':')+1));
            int randomMinuteEnd = int.Parse(chat.TimeIntervalEnd!.Substring(chat.TimeIntervalEnd.IndexOf(':')+1));
        
            if ( hourEnd  - hourStart > 3)
            {
                randomHourStart = new Random().Next(hourStart + 1, hourEnd - 1);
                randomHourEnd= new Random().Next(hourStart + 1, hourEnd - 1);
            
                randomMinuteStart=new Random().Next(0, 59);
                randomMinuteEnd=new Random().Next(0, 59);
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