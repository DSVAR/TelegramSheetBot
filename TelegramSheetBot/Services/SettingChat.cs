using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

/// <summary>
/// настройка чата
/// </summary>
public class SettingChat
{
    private readonly JobWithBd<StructureChat> _jobWithBd;
    private readonly DayCallBackService _dayCallBackService;

    public SettingChat(JobWithBd<StructureChat> jobWithBd, DayCallBackService dayCallBackService)
    {
        _jobWithBd = jobWithBd;
        _dayCallBackService = dayCallBackService;
    }

    /// <summary>
    /// проверка на существования чата в бд
    /// </summary>
    /// <param name="chatId"></param>
    public async Task CheckChatId(long chatId)
    {
        try
        {
            if ((await _jobWithBd.FindAsync(chatId)) == null)
            {
                await _jobWithBd.CreateAsync(new()
                {
                    ChatId = chatId
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message+" checkChatID");
        }
       
    }

    // /// <summary>
    // /// добавление строки в лист
    // /// </summary>
    // /// <param name="chatId"></param>
    // /// <param name="name"></param>
    // /// <param name="client"></param>
    // public async Task AddListOfSheet(long chatId, string name, ITelegramBotClient client)
    // {
    //     var chat = await _jobWithBd.FindAsync(chatId);
    //
    //     if ((chat) != null)
    //     {
    //         if (chat.ListSheet == null)
    //         {
    //             var list = new List<string>();
    //             list.Add(name);
    //             chat.ListSheet = list;
    //             await _jobWithBd.Update(chat);
    //         }
    //         else
    //         {
    //             if (chat.ListSheet!.Count < 10)
    //             {
    //                 chat.ListSheet!.Add(name);
    //                 await  _jobWithBd.Update(chat);
    //             }
    //             else await client.SendTextMessageAsync(chatId, "больше 10 объектов добавить нельзя ", disableNotification: true);
    //         }
    //         
    //     }
    // }

    /// <summary>
    /// обновление дня начала и конца голосования
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="dayOfWeekStart"></param>
    /// <param name="dayOfWeekEnd"></param>
    public async Task UpdateDayInChat(long chatId, string? dayOfWeekStart = null, string? dayOfWeekEnd = null)
    {
        var chat = await _jobWithBd.FindAsync(chatId);

        if ((chat) != null)
        {
            if (!string.IsNullOrEmpty(dayOfWeekStart))
            {
                chat.DayOfWeekStartPoll = dayOfWeekStart;
                await _jobWithBd.Update(chat);
            }

            if (!string.IsNullOrEmpty(dayOfWeekEnd))
            {
                chat.DayOfWeekEndPoll = dayOfWeekEnd;
                await  _jobWithBd.Update(chat);
            }
        }
    }

    /// <summary>
    /// обновление гугл токена
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="token"></param>
    public async Task UpdateToken(long chatId, string token)
    {
        var chat = await _jobWithBd.FindAsync(chatId);

        if ((chat) != null)
        {
            chat.GoogleSheetToken = token;
            await _jobWithBd.Update(chat);
        }
    }

    /// <summary>
    /// обновление временного интервала
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="timeInterval"></param>
    /// <returns></returns>
    public async Task<bool> UpdateTimeInterval(long chatId, string timeInterval)
    {
        try
        {
            string pattern = @"\d{1,2}:\d{2}-\d{1,2}:\d{2}";
            var reg = new Regex(pattern);

            if (reg.IsMatch(timeInterval))
            {
                var chat = await _jobWithBd.FindAsync(chatId);


                var startInterval = timeInterval.Remove(timeInterval.IndexOf('-'));
                var endInterval = timeInterval.Substring(timeInterval.IndexOf('-') + 1);

                var timeFirst = DateTime.Parse(startInterval);
                var timeSecond = DateTime.Parse(endInterval);

                if (timeFirst < timeSecond)
                {
                    if ((chat) != null)
                    {
                        chat.TimeIntervalStart = startInterval;
                        chat.TimeIntervalEnd = endInterval;
                        chat.FirstSetSettings = false;
                        chat.CreatedRequestPoll = false;
                        chat.CreatedPollThisWeek = false;
                        chat.CreatedPoll = false;
                        chat.LastChangeTime = new DateTime();
                        chat.TimeStartPoll = "";
                        chat.TimeEndPoll = "";

                        await _jobWithBd.Update(chat);
                        return true;
                    }
                }
                else return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return false;
    }


    public async Task CreatePoll(ITelegramBotClient client,long id)
    {
        try
        {
            var chat = await _jobWithBd.FindAsync(id);
            
            if ((chat.ListSheet) != null)
            {
                if (chat.ListSheet!.Count >= 5)
                {
                    var message=await client.SendPollAsync(chat.ChatId, "голосование", chat.ListSheet!, 
                        allowsMultipleAnswers:true,isAnonymous:false);
                
                    chat!.CreatedPoll = true;
                    chat.IdMessageLastPoll = message.MessageId;
                    chat.ListSheet.Clear();
                    chat.PollId= message.Poll!.Id;
                    
                    await _jobWithBd.Update(chat);
               
               
                }
                else await client.SendTextMessageAsync(id, "объектов должно быть больше 5", disableNotification: true);
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task<bool> Exist(long id)
    {
        try
        {
            var chat =await _jobWithBd.FindAsync(id);
            if ((chat) == null)
            {
                return false;
            }


            return true;
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task Settings(ITelegramBotClient client,long chatId)
    {
        var item = await _jobWithBd.FindAsync(chatId);
        if (!item!.FirstSetSettings)
        {
            var listOfDayWeek =  new InlineKeyboardMarkup(new []
            {
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData("настройка временного интервала","/interval"),
                },
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData("настройка дней","/day")
                },
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData("изменение токена","/token")
                },
                
            }) ;

            await client.SendTextMessageAsync(chatId: chatId, "Настройки:", replyMarkup: listOfDayWeek, disableNotification: true);

        }
        else
        {
            await  _dayCallBackService.DayStartPollInChat(client, chatId);
        }
    }
    // public async Task UpdatePoll(PollOption[] pollOption,string pollId)
    // {
    //     var chat = await _jobWithBd.FindAsyncString(pollId);
    //     var n = new StructureChat();
    //     n.Options = new List<MyPollOptions>();
    //
    //     if ((chat) != null)
    //     {
    //         foreach (var option in pollOption)
    //         {
    //             n.Options!.Add(new MyPollOptions()
    //             {
    //                 Name = option.Text,
    //                 VoterCount = option.VoterCount
    //             });
    //             chat.Options!.Add(new MyPollOptions()
    //             {
    //                 Name = option.Text,
    //                 VoterCount = option.VoterCount
    //             });
    //         }
    //     }
    //     
    //
    //    
    //
    // }
}