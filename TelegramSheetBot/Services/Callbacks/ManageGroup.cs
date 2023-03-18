using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services.Callbacks;

public class ManageGroup
{
    private readonly JobWithBd<StructureChat> _jobWithBdChat;
    private readonly JobWithBd<ManageChat> _jobWithBdManageChat;
    private readonly TelegramBotClient _client;

    public ManageGroup(JobWithBd<StructureChat> jobWithBdChat, TelegramBotClient client,
        JobWithBd<ManageChat> jobWithBdManageChat)
    {
        _jobWithBdChat = jobWithBdChat;
        _jobWithBdManageChat = jobWithBdManageChat;
        _client = client;
    }

    /// <summary>
    /// получение всех чатов
    /// </summary>
    /// <param name="chatId"></param>
    public async Task GetAllGroups(long chatId)
    {

        var inlineMarkup =await GetChatsInlineMarkup();

        var message = await _client.SendTextMessageAsync(chatId, "Чаты", replyMarkup: inlineMarkup);


        var foundManageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(m => m.ChatId == chatId);


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
    /// tgchatid- id для отправки
    /// </summary>
    /// <param name="tgChatId"></param>
    /// <param name="chatManageId"></param>
    public async Task SettingChat(long tgChatId,long chatManageId)
    {
        
        var chatManage = (await _jobWithBdManageChat.GetItemsAsync()).First(cm => cm.ChatId == chatManageId);
        var chat = (await _jobWithBdChat.GetItemsAsync()).FirstOrDefault(c=>c.ChatId==chatManage.SelectedChat);

        chatManage.SelectedChat = tgChatId;
        await _jobWithBdManageChat.Update(chatManage);
        
        var isPollStart=chat!.CanStartPoll? "Выключить опросы" : "Включить опросы";
        var callBackIsPollStart= chat.CanStartPoll? $"/ClosePoll" : $"/OpenPoll";
        
        
        var inlube = new InlineKeyboardMarkup(new []
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(isPollStart,callBackIsPollStart),
                InlineKeyboardButton.WithCallbackData("Начать опрос",$"/DaysEndPoll")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("Настройка времени","/SettingTime_"),
                InlineKeyboardButton.WithCallbackData("Исправление","/SendButton")
                // InlineKeyboardButton.WithCallbackData("",$"")
            },
            new []
            {
                 InlineKeyboardButton.WithCallbackData("Назад",$"/Back")
            }
        });

       await  _client.EditMessageTextAsync(chatManageId,chatManage.LastMessage,$"{chat.NameChat}",replyMarkup:inlube );
    }


    public async Task FunctionBack(long chatId)
    {
        var inlineMarkup = await GetChatsInlineMarkup();
        var chatManage=(await _jobWithBdManageChat.GetItemsAsync()).First(cm => cm.ChatId == chatId);
        
        await  _client.EditMessageTextAsync(chatId,chatManage.LastMessage,"Чаты",replyMarkup:inlineMarkup);
    }

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
            listOfDays= arrayOfDay.Where(x => x != 0).ToList();
        }
        else
        {
            listOfDays = arrayOfDay.Where(x => x >=dayToday).ToList();
            listOfDays.Add(DayOfWeek.Sunday);
        }

        if (listOfDays.Count > 0)
        {
            correctList  = listOfDays.Chunk(2).Select(x => x.Select(y =>
                InlineKeyboardButton.WithCallbackData(y.ToString(), $"/ForceStartPoll_{y}")).ToArray()).ToList();
            correctList.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад","/Back") });

        }

        var manageChat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == chatId);
        await _client.EditMessageTextAsync(chatId, manageChat!.LastMessage, "Выбери день окончания голосования",
            replyMarkup:new InlineKeyboardMarkup(correctList));
        
        
    }


    public async Task ForceStartPoll(long chatId,string week)
    {
        var dayToday = DateTime.Now.DayOfWeek;
        var das = Enum.Parse<DayOfWeek>($"{week}");
        var te = das - dayToday;
        var nextDay = DateTime.Now.AddDays(te);
        
        var sw = 5;

    }


    public async Task SendButton(long id)
    {
        var chat = (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(ch => ch.ChatId == id);
        
      
        await _client.SendTextMessageAsync(chat!.SelectedChat,"удаление клавиатуры", replyMarkup:new ReplyKeyboardRemove());

    }
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
}