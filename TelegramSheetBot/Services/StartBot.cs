using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramSheetBot.Services;

public class StartBot
{

    private readonly TelegramBotClient _client;
    private readonly SettingChat _settingChat;
    private readonly CommandsHandler _handler;
    private readonly DayCallBackService _dayCallBackService;
    private readonly PollService _pollService;

    public StartBot(TelegramBotClient client, SettingChat settingChat, CommandsHandler handler,DayCallBackService dayCallBackService,
        PollService pollService)
    {
        _client = client;
        _settingChat = settingChat;
        _handler = handler;
        _dayCallBackService = dayCallBackService;
        _pollService = pollService;
    }
    
    
    public async Task Init()
    {
        await _client.ReceiveAsync(FuncUpdate, FuncError);
        Console.WriteLine( await _client.TestApiAsync() );
    }





    private async Task FuncUpdate(ITelegramBotClient client, Update update, CancellationToken token)
    {
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



        if (update.Type == UpdateType.CallbackQuery)
        {
            var callback = update.CallbackQuery;
            var id = update.CallbackQuery!.Message!.Chat.Id;
            
           
            var administrators=await client.GetChatAdministratorsAsync(chatId: id, cancellationToken: token);
            Console.WriteLine($"Кто нажал кнопку{callback!.From}, {callback.Data}");
          
            if (callback.From.Id == 421814730 || ThereAdmin(administrators,id))
            {
                await CallbackMessage(callback, client, id);
            }
            

            return;
        }


        if (update.Type == UpdateType.Poll)
        {
          
            await _pollService.AddPoll(update.Poll!);
            return;
        }

        if (update.Type == UpdateType.PollAnswer)
        {
            //какой человек проголосовал
            return;
        }


        if(update.Type==UpdateType.Message)
        {
            var message = update.Message;

            Console.WriteLine($"От кого:{message!.From} {message.Text}");
            if (message.Type == MessageType.ChatMemberLeft)
            {
                return;
            }

            if (message.Type == MessageType.ChatMembersAdded)
            {
                if (!await _settingChat.Exist(message.Chat.Id))
                { //добавление чат
                    await _dayCallBackService.DayStartPollInChat(client, update.MyChatMember!.Chat.Id);
                }
               
                return;
            }
        
            

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
    }

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