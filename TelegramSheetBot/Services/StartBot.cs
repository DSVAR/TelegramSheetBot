using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramSheetBot.Services.Callbacks;

namespace TelegramSheetBot.Services;

public class StartBot
{
//сервисы
    private readonly TelegramBotClient _client;
    private readonly SettingChat _settingChat;
    private readonly CommandsHandler _handler;
    private readonly DayCallBackService _dayCallBackService;
    private readonly PollService _pollService;
    private readonly ManageGroup _manageGroup;

    public StartBot(TelegramBotClient client, SettingChat settingChat, CommandsHandler handler,DayCallBackService dayCallBackService,
        PollService pollService, ManageGroup manageGroup)
    {
        _client = client;
        _settingChat = settingChat;
        _handler = handler;
        _dayCallBackService = dayCallBackService;
        _pollService = pollService;
        _manageGroup = manageGroup;
    }

    /// <summary>
    /// инициализация функции  чтения сообщений и ошибок
    /// </summary>
    public async Task Init()
    {
        try
        {
            await _client.ReceiveAsync(FuncUpdate, FuncError);
           
            Console.WriteLine( await _client.TestApiAsync() );
        }
        catch (Exception ex)
        {
            Console.WriteLine( ex.Message );
            return;
        }

        
    }




/// <summary>
/// получение сообщения
/// </summary>
/// <param name="client"></param>
/// <param name="update"></param>
/// <param name="token"></param>
    private async Task FuncUpdate(ITelegramBotClient client, Update update, CancellationToken token)
    {
        //логи при добавлении в чат и удалении
        if (update.Type == UpdateType.MyChatMember)
        {
            if (update.MyChatMember!.NewChatMember.Status == ChatMemberStatus.Left)
            {
                Console.WriteLine(
                    $"нас удалил чат пидоров : {update.MyChatMember!.Chat.Id} {update.MyChatMember!.Chat.Title}");
                Console.WriteLine($"удаливший пидор : {update.MyChatMember.From.Username}");
                return;
            }

            if (update.MyChatMember!.NewChatMember.Status == ChatMemberStatus.Member)
            {
                Console.WriteLine(
                    $"нас добавил чат пидоров : {update.MyChatMember!.Chat.Id} {update.MyChatMember!.Chat.Title}");
                Console.WriteLine($"добавивший пидор : {update.MyChatMember.From.Username}");

                await _settingChat.CheckChatId(update.MyChatMember!.Chat.Id);
                return;
            }
        }


        //управление callbacks
        if (update.Type == UpdateType.CallbackQuery)
        {
            var callback = update.CallbackQuery;
            var id = update.CallbackQuery!.Message!.Chat.Id;
         
            //await _client.AnswerCallbackQueryAsync(callback!.Id, cancellationToken: token);
            
            if (callback!.Message!.Chat.Type != ChatType.Private)
            {
                var administrators=await client.GetChatAdministratorsAsync(chatId: id, cancellationToken: token);
            
                Console.WriteLine($"Кто нажал кнопку{callback!.From}, {callback.Data}");

                if (callback.From.Id == 421814730 || ThereAdmin(administrators,id))
                {
                    await CallbackMessage(callback, client, id);
                }
            }
            else
            {
                if (callback.Data![0].ToString() == "/" && callback.From!.Id==421814730 || callback.From.Id== 5898221054)
                {
                    await _handler.AdminsCallback(callback.Data!, id);
                }
            }
            return;
        }


        //изменения опроса
        if (update.Type == UpdateType.Poll)
        {
            if(!update.Poll!.IsClosed)
                 await _pollService.AddPoll(update.Poll!);
            // return;
        }

        if (update.Type == UpdateType.PollAnswer)
        {
            var pollAnswer = update.PollAnswer;
            Console.WriteLine($"Проголосовал :{pollAnswer!.User.FirstName} ({pollAnswer.User.Id})");
        }


        if(update.Type==UpdateType.Message)
        {
            var message = update.Message;
            
            var chat = await client.GetChatAsync(message!.Chat.Id);
            //если отправляется из группы то мы фиксируем сообщение
            if (chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup)
            {
                Console.WriteLine($"От кого:{message!.From} {message.Text}");
           
                if (string.IsNullOrEmpty(message.Text!))
                    return;

            
                var administrators=await client.GetChatAdministratorsAsync(chatId: message.Chat.Id, cancellationToken: token);

            
                if (message.From!.Id == 421814730 || ThereAdmin(administrators,message.From.Id))
                {
                    if (message.Text!.ToCharArray()[0] == '/')
                    {
                        await _handler.TextCommand(message.Text!, client, message.Chat.Id);
                    }

                    if (message.Text!.ToCharArray()[0].ToString() == "!")
                    {
                        await _handler.ExMarkCommand(message.Text!, client, message.Chat.Id);
                    }
                }
            }

            if (chat.Type == ChatType.Private && message.From!.Id==421814730 || message.From!.Id==5898221054)
            {
                if (message.Text!.ToCharArray()[0] == '/')
                {
                    await _handler.AdminsCallback(message.Text!,  message.Chat.Id);
                }
            }
            
        }
    }

/// <summary>
/// получение ошибок
/// </summary>
/// <param name="clent"></param>
/// <param name="exception"></param>
/// <param name="token"></param>
/// <returns></returns>
    private Task FuncError(ITelegramBotClient clent, Exception exception, CancellationToken token)
    {
        var error = exception switch
        {
            ApiRequestException apiRequestException=>
                $"ошибка по API :\n {apiRequestException.ErrorCode } \n {apiRequestException.Message}",
            _ =>exception.Message 
            
        };
        Console.WriteLine($"{error}");
        return Task.CompletedTask;
    }


    private async Task CallbackMessage(CallbackQuery callback,ITelegramBotClient client,long id)
    {
        if (callback.Data![0].ToString() == "!")
        {
            await _handler.ExMarkCommand(callback.Data!, client, id);
        }

        if (callback.Data![0].ToString() == "/")
        {
            await _handler.TextCommand(callback.Data!, client, id);
        }
    }


    private  bool ThereAdmin(ChatMember[] admins,long idUser)
    {
        foreach (var admin in admins)
        {
            if (admin.User.Id == idUser)
            {
                return true;
            }
        }

        return false;
    }
}