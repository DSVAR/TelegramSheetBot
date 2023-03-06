using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramSheetBot.Services;

public class DayCallBackService
{
    /// <summary>
    /// дни недели для callback
    /// </summary>
    /// <param name="client"></param>
    /// <param name="chatId"></param>
      public  async Task DayStartPollInChat(ITelegramBotClient client,long chatId)
    {
        var listOfDayWeek =  new InlineKeyboardMarkup(new []
        {
            new List<InlineKeyboardButton>()
            {
                 InlineKeyboardButton.WithCallbackData("Понедельник","!Start_Monday"),
                 InlineKeyboardButton.WithCallbackData("Вторник","!Start_Tuesday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Среда","!Start_Wednesday"),
                InlineKeyboardButton.WithCallbackData("Четверг","!Start_Thursday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Пятница","!Start_Friday"),
                InlineKeyboardButton.WithCallbackData("Суббота","!Start_Saturday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Воскресенье","!Start_Sunday")
            }
        }) ;

       await client.SendTextMessageAsync(chatId: chatId, "Выберите день недели для старта голосования.", replyMarkup: listOfDayWeek,
           disableNotification: true);

    }

    public  async Task DayEndPollInChat(ITelegramBotClient client,long chatId)
    {
        var listOfDayWeek =  new InlineKeyboardMarkup(new []
        {
            new List<InlineKeyboardButton>()
            {
                 InlineKeyboardButton.WithCallbackData("Понедельник","!End_Monday"),
                 InlineKeyboardButton.WithCallbackData("Вторник","!End_Tuesday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Среда","!End_Wednesday"),
                InlineKeyboardButton.WithCallbackData("Четверг","!End_Thursday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Пятница","!End_Friday"),
                InlineKeyboardButton.WithCallbackData("Суббота","!End_Saturday")
            },
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Воскресенье","!End_Sunday")
            }
        }) ;

        await client.SendTextMessageAsync(chatId: chatId, "Выберите день недели для окончания голосования.",
            replyMarkup: listOfDayWeek, disableNotification: true);

    }
}