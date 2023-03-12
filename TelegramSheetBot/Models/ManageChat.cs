namespace TelegramSheetBot.Models;

public class ManageChat
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public int LastMessage { get; set;}
}