using Telegram.Bot;
using TelegramSheetBot.Interfaces;

namespace TelegramSheetBot.Services;

public class JobTelegramBot:IJobTelegramBot
{
    private readonly ITelegramBotClient _client;
    public JobTelegramBot(ITelegramBotClient client)
    {
        _client = client;
    }
    
    public async Task SendMessage(long chatId,string message)
    {
      await  _client.SendTextMessageAsync(chatId, message);
      
    }
}