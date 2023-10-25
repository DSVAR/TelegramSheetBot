using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

/// <summary>
/// настройка чата
/// </summary>
public class SettingChat
{
    private readonly IJobWithBd<StructureChat> _jobWithBdChat;
    private readonly DayCallBackService _dayCallBackService;
    private readonly TelegramBotClient _client;

    public SettingChat(IJobWithBd<StructureChat> jobWithBdChat, DayCallBackService dayCallBackService,TelegramBotClient client)
    {
        _jobWithBdChat = jobWithBdChat;
        _dayCallBackService = dayCallBackService;
        _client = client;
    }

    /// <summary>
    /// проверка на существования чата в бд
    /// </summary>
    /// <param name="chatId"></param>
    public async Task CheckChatId(long chatId)
    {
        try
        {
            if ((await _jobWithBdChat.FindAsync(chatId)) == null)
            {
                var chat = await _client.GetChatAsync(chatId);
                await _jobWithBdChat.CreateAsync(new()
                {
                    ChatId = chatId,
                    NameChat = chat.Title,
                    CanStartPoll =true
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message+" checkChatID");
        }
       
    }

  

    /// <summary>
    /// обновление дня начала и конца голосования
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="dayOfWeekStart"></param>
    /// <param name="dayOfWeekEnd"></param>
    public async Task UpdateDayInChat(long chatId, string? dayOfWeekStart = null, string? dayOfWeekEnd = null)
    {
        var chat = await _jobWithBdChat.FindAsync(chatId);

        if ((chat) != null)
        {
            if (!string.IsNullOrEmpty(dayOfWeekStart))
            {
                chat.DayOfWeekStartPoll = dayOfWeekStart;
                await _jobWithBdChat.Update(chat);
            }

            if (!string.IsNullOrEmpty(dayOfWeekEnd))
            {
                chat.DayOfWeekEndPoll = dayOfWeekEnd;
                await  _jobWithBdChat.Update(chat);
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
        var chat = await _jobWithBdChat.FindAsync(chatId);

        if ((chat) != null)
        {
            chat.GoogleSheetToken = token;
            await _jobWithBdChat.Update(chat);
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
                var chat = await _jobWithBdChat.FindAsync(chatId);


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

                        await _jobWithBdChat.Update(chat);
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

/// <summary>
/// проверка на существования чата в таблице
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
    public async Task<bool> Exist(long id)
    {
        try
        {
            var chat =await _jobWithBdChat.FindAsync(id);
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

/// <summary>
/// настройка чата
/// </summary>
/// <param name="client"></param>
/// <param name="chatId"></param>
    public async Task Settings(ITelegramBotClient client,long chatId)
    {
        var item = await _jobWithBdChat.FindAsync(chatId);
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

/// <summary>
/// обновление
/// </summary>
/// <param name="chatId"></param>
    public async Task CheckUpdate(long chatId)
    {
        var chat = await _jobWithBdChat.FindAsync(chatId);
        //
        if (string.IsNullOrEmpty( chat.NameChat))
        {
            var fullChat = await _client.GetChatAsync(chatId);
            chat.NameChat = fullChat.Title;
            await _jobWithBdChat.Update(chat);
        }
    }
}