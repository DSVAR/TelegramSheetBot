namespace TelegramSheetBot.Interfaces;

public interface IJobTelegramBot
{
    public Task SendMessage(long chatId,string message);
    
}