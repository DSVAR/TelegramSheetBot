using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Models;
using TelegramSheetBot.Services.JobWithBd;

namespace TelegramSheetBot.Services.Callbacks;

public class ManageGroup
{
    private readonly IJobWithBd<StructureChat> _jobWithBdChat;
    private readonly IJobWithBd<ManageChat> _jobWithBdManageChat;
    private readonly TelegramBotClient _client;
    private readonly PollService _pollService;
    private readonly FindingService _findingService;

    public ManageGroup(IJobWithBd<StructureChat> jobWithBdChat, TelegramBotClient client,
        IJobWithBd<ManageChat> jobWithBdManageChat, PollService pollService,
        FindingService findingService)
    {
        _jobWithBdChat = jobWithBdChat;
        _jobWithBdManageChat = jobWithBdManageChat;
        _client = client;
        _pollService = pollService;
        _findingService = findingService;
    }

    /// <summary>
    /// получение всех чатов
    /// </summary>
    /// <param name="chatId"></param>
    public async Task GetAllGroups(long chatId)
    {
        var task = _findingService.MChatFindByIdAsync(chatId);
        var inlineMarkup = await GetChatsInlineMarkup();

        var message = await _client.SendTextMessageAsync(chatId, "Чаты", replyMarkup: inlineMarkup);


        var foundManageChat = await task;
        // var foundManageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(m => m.ChatId == chatId);


        if (foundManageChat is null)
        {
            var manaChat = new ManageChat() { ChatId = chatId, LastMessage = message.MessageId };
            await _jobWithBdManageChat.CreateAsync(manaChat);
        }
        else
        {
            foundManageChat.LastMessage = message.MessageId;
            await _jobWithBdManageChat.Update(foundManageChat);
        }
    }

    /// <summary>
    /// tgchatid - id для отправки
    /// </summary>
    /// <param name="tgChatId"></param>
    /// <param name="chatManageId"></param>
    public async Task SettingChat(long tgChatId, long chatManageId)
    {
        var chatManage = await _findingService.MChatFindByIdAsync(chatManageId);
        var chat = await _findingService.SChatFindByChatIdAsync(tgChatId);

        chatManage!.SelectedChat = tgChatId;
        await _jobWithBdManageChat.Update(chatManage);

        var isPollStart = chat!.CanStartPoll ? "Выключить опросы" : "Включить опросы";
        var callBackIsPollStart = chat.CanStartPoll ? $"/ClosePoll" : $"/OpenPoll";


        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(isPollStart, callBackIsPollStart),
                InlineKeyboardButton.WithCallbackData("Начать опрос", $"/DaysEndPoll")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Настройка времени", "/SettingTime_"),
                InlineKeyboardButton.WithCallbackData("Исправление", "/SendButton")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Время начала голосования", $"/TimeStart")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Закрыть голосование", $"/EndPoll")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Время конца голосования", $"/TimeEnd")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Настройка типа голосования", $"/TypeBot")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"/Back")
            }
        });

        await _client.EditMessageTextAsync(chatManageId, chatManage.LastMessage, $"{chat.NameChat}",
            replyMarkup: keyboard);
    }

    /// <summary>
    /// обработка кнопки назад
    /// </summary>
    /// <param name="chatId"></param>
    public async Task FunctionBack(long chatId)
    {
        var inlineMarkup = await GetChatsInlineMarkup();
        var chatManage = await _findingService.MChatFindByIdAsync(chatId);
        // var chatManage=(await _jobWithBdManageChat.GetItemsAsync()).First(cm => cm.ChatId == chatId);

        await _client.EditMessageTextAsync(chatId, chatManage!.LastMessage, "Чаты", replyMarkup: inlineMarkup);
    }

    /// <summary>
    /// выбрать день конца голосования
    /// </summary>
    /// <param name="chatId"></param>
    public async Task DaysForEndPoll(long chatId)
    {
        var dayToday = DateTime.Now.DayOfWeek;
        DayOfWeek[] arrayOfDay =
        {
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };
        List<DayOfWeek> listOfDays;
        List<InlineKeyboardButton[]> correctList = new List<InlineKeyboardButton[]>();
        if (dayToday == 0)
        {
            listOfDays = arrayOfDay.Where(x => x != 0).ToList();
        }
        else
        {
            listOfDays = arrayOfDay.Where(x => x >= dayToday).ToList();
            listOfDays.Add(DayOfWeek.Sunday);
        }

        if (listOfDays.Count > 0)
        {
            correctList = listOfDays.Chunk(2).Select(x => x.Select(y =>
                InlineKeyboardButton.WithCallbackData(y.ToString(), $"/ForceStartPoll_{y}")).ToArray()).ToList();
            correctList.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", "/Back") });
        }

        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == chatId);
        await _client.EditMessageTextAsync(chatId, manageChat!.LastMessage, "Выбери день окончания голосования",
            replyMarkup: new InlineKeyboardMarkup(correctList));
    }

    /// <summary>
    /// начать голосование
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="week"></param>
    public async Task ForceStartPoll(long chatId, string dayOfEnd)
    {
        
        var manageChat = await _findingService.MChatFindByIdAsync(chatId);
        // var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == chatId);

        var chat = await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);

        var dayToday = DateTime.Now.DayOfWeek;
        var endDay = Enum.Parse<DayOfWeek>($"{dayOfEnd}");
        var countDay=0;
        if (dayOfEnd == "Sunday")
        {
            var t = (int)dayToday;
            countDay = 7 - t;
        }
        else
        {
            countDay=endDay - dayToday;
        }
        if (countDay >= 0)
        {
            var nextDay = DateTime.Now.AddDays(countDay).AddMinutes(2);
            await _pollService.StartPoll(chat.ChatId, nextDay,true);
        }
    }

    /// <summary>
    /// удаление клавиатуры из чаты
    /// </summary>
    /// <param name="id"></param>
    public async Task SendButton(long id)
    {
        var chat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);


        await _client.SendTextMessageAsync(chat!.SelectedChat, "удаление клавиатуры",
            replyMarkup: new ReplyKeyboardRemove());
    }

    /// <summary>
    /// получение чатов
    /// </summary>
    /// <returns></returns>
    private async Task<InlineKeyboardMarkup> GetChatsInlineMarkup()
    {
        var listGroup = await _jobWithBdChat.GetItemsAsync();
        var key = new List<InlineKeyboardButton>();

        foreach (var group in listGroup)
        {
            key.Add(InlineKeyboardButton.WithCallbackData($"{group.NameChat}", $"/chat_{group.ChatId}"));
        }

        return new InlineKeyboardMarkup(key);
    }

    /// <summary>
    /// получить время старта голосования
    /// </summary>
    /// <param name="id"></param>
    public async Task GetTimeStartPoll(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat = await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);

        var timeStart = chat.DayOfWeekStartPoll + " " + chat.TimeStartPoll;
        await _client.SendTextMessageAsync(id, $"дни {timeStart}");
    }

    /// <summary>
    /// получение времени конца голосования
    /// </summary>
    /// <param name="id"></param>
    public async Task GetTimeEndPoll(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat = await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);

        var timeStart = chat.DayOfWeekEndPoll + " " + chat.TimeEndPoll;
        await _client.SendTextMessageAsync(id, $"дни {timeStart}");
    }

    /// <summary>
    /// Закрытие голосования
    /// </summary>
    /// <param name="id"></param>
    public async Task ForceEndPoll(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat = await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);
        await _client.StopPollAsync(chat.ChatId, chat.IdMessageLastPoll);

        chat.CreatedPoll = false;
        chat.CreatedRequestPoll = false;
        chat.CreatedPollThisWeek = true;
        chat.LastChangeTime = chat.LastChangeTime.AddDays(3);
        await  _jobWithBdChat.Update(chat);
       
        await _client.SendTextMessageAsync(chat.ChatId, $"Голосования закрыто");
    } 
 
    
    public async Task TypeBot(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat = await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);
        
        string? isOne = null;
        string? isPeriod=null;
        
        
        if (chat.UsualTime)
        {
            isOne = GlobalValues.SmileStar;
        }
        else
        {
            isPeriod = GlobalValues.SmileStar;
        }
        
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{isOne}выбранный один день", $"/IsOne"),
                InlineKeyboardButton.WithCallbackData($"{isPeriod}период", $"/Period")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", $"/Back")
            }
        });
        
        
        await _client.EditMessageTextAsync(id, manageChat!.LastMessage, $"{chat!.NameChat}",
            replyMarkup: keyboard);
    }
    public async Task IsOneDayMethod(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat =await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);
        
        List<DayOfWeek> listOfDays=new ()
        {
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };
        List<InlineKeyboardButton[]> correctList = new List<InlineKeyboardButton[]>();
        List<string> daysStrings = new List<string>();
  
        foreach (var day in listOfDays)
        {
            if (day.ToString() == chat.UsualDayStart)
            {
                daysStrings.Add(GlobalValues.SmileStar+day);
            }
            else
            {
                daysStrings.Add(day.ToString());
            }
        }
        
        if (listOfDays.Count > 0)
        {
            correctList =daysStrings.Chunk(2).Select(x => x.Select(y =>
                InlineKeyboardButton.WithCallbackData(y.ToString(), $"/ChangeOneDay_{y}")).ToArray()).ToList();
            
            correctList.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", "/TypeBot") });
        }

       
        await _client.EditMessageTextAsync(id, manageChat!.LastMessage, "Выбери день старта и окончания голосования",
            replyMarkup: new InlineKeyboardMarkup(correctList));
    }

    public async Task ChangeOneDay(long id,string? day)
    {
      
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat =await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);

        chat.UsualDayStart = day;
        chat.UsualTime = true;
        
        await _jobWithBdChat.Update(chat);

        await IsOneDayMethod(id);

    }

    public async Task Period(long id)
    {
        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        var chat =await _jobWithBdChat.FindAsync(manageChat!.SelectedChat);

        chat.UsualDayStart = "";
        chat.UsualTime = false;
        
        await _jobWithBdChat.Update(chat);
        await TypeBot(id);
    }
}