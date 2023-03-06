using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services.Callbacks;

public class ManageGroup
{
    private readonly JobWithBd<StructureChat> _jobWithBd;
    private readonly TelegramBotClient _client;
    public ManageGroup(JobWithBd<StructureChat> jobWithBd,TelegramBotClient client)
    {
        _jobWithBd = jobWithBd;
        _client = client;
    }
    
    
    public async Task GetAllGroups(long chatId)
    {
        var listGroup = await _jobWithBd.GetItemsAsync();
        var keyboardList = new List<InlineKeyboardMarkup>();
        var key = new List<InlineKeyboardButton>();
        
        foreach (var group in listGroup )
        {
            key.Add(InlineKeyboardButton.WithCallbackData($"{group.ChatId}",$"chat_{group.ChatId}"));
        }

       var message=await _client.SendTextMessageAsync(chatId,"58",replyMarkup: new InlineKeyboardMarkup(key));
       var sw = 4;
    }
}