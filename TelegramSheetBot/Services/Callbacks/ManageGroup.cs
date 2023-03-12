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
        var listGroup = await _jobWithBdChat.GetItemsAsync();
        var key = new List<InlineKeyboardButton>();

        foreach (var group in listGroup)
        {
            key.Add(InlineKeyboardButton.WithCallbackData($"{group.NameChat}", $"/chat_{group.ChatId}/{chatId}"));
        }

        var message = await _client.SendTextMessageAsync(chatId, "Чаты", replyMarkup: new InlineKeyboardMarkup(key));


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
    /// <param name="chatId"></param>
    /// <param name="chatManageId"></param>
    public async Task SettingChat(long tgChatId,long chatManageId)
    {
        var chat = (await _jobWithBdChat.GetItemsAsync()).FirstOrDefault(c=>c.ChatId==tgChatId);
        var chatManage = (await _jobWithBdManageChat.GetItemsAsync()).First(cm => cm.ChatId == chatManageId);
        var isPollStart=chat!.CanStartPoll? "Выключить опросы" : "Включить опросы";
        var callBackIsPollStart= chat!.CanStartPoll? $"/ClosePoll_{tgChatId}" : $"/OpenPoll_{tgChatId}";
        
        var inlube = new InlineKeyboardMarkup(new []
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(isPollStart,callBackIsPollStart),
                InlineKeyboardButton.WithCallbackData("Начать опрос",$"/ForceStartPoll_{tgChatId}")
            },
            new []
            {
                 InlineKeyboardButton.WithCallbackData("Назад","/Back")
            }
        });

       await  _client.EditMessageTextAsync(chatManageId,chatManage.LastMessage,"настройки чата",replyMarkup:inlube);

    }
}