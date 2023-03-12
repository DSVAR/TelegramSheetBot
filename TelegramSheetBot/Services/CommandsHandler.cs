using Telegram.Bot;
using TelegramSheetBot.Models;
using TelegramSheetBot.Services.Callbacks;

namespace TelegramSheetBot.Services;

public class CommandsHandler
{
    private readonly GoogleSheets _googleSheets;
    private readonly SettingChat _settingChat;
    private readonly DayCallBackService _dayCallBackService;
    private readonly JobWithBd<StructureChat> _structureChat;
    private readonly ManageGroup _manageGroup;
    public CommandsHandler(GoogleSheets googleSheets,SettingChat settingChat,
        DayCallBackService dayCallBackService,JobWithBd<StructureChat> structureChat,ManageGroup manageGroup)
    {
        _googleSheets = googleSheets;
        _settingChat = settingChat;
        _dayCallBackService = dayCallBackService;
        _structureChat = structureChat;
        _manageGroup = manageGroup;
    }

    public async Task TextCommand(string text, ITelegramBotClient client, long chatId)
    {
        switch (text.ToLower())
        {
            case "/start":
            {
                await _settingChat.CheckChatId(chatId);

                await _settingChat.Settings(client, chatId);
               break;
            }
           
            case "/emoji":
            {
                await client.SendTextMessageAsync(chatId, $"{GlobalValues.SmileNumberZero} " +
                                                          $"{GlobalValues.SmileNumberSecond} " +
                                                          $"{GlobalValues.SmileStar} " +
                                                          $"{GlobalValues.SmileNumberTenth}",disableNotification:true);
                break;
            }
            case "/setting":
            {
                await _settingChat.CheckChatId(chatId);

               await _settingChat.Settings(client, chatId);

              
                break;
            }
            case "/day":
            {
                await _dayCallBackService.DayStartPollInChat(client, chatId);
                break;
            }
            case "/interval":
            {
                await client.SendTextMessageAsync(chatId,
                    "Установите временной интервал. Пример: !timeInterval 7:00-14:00 ",disableNotification:true);
                break;
            }
            case "/googleToken":
            {
                break;
            }
          
        }

       
    }

  
    
    public async Task ExMarkCommand(string text,ITelegramBotClient client,long chatId)
    {

        var item = await _structureChat.FindAsync(chatId);
        
         if (text.Contains("Start"))
         {
             var dayStart = text.Replace("!Start_", "");
             dayStart = dayStart.TrimEnd(' ');
             dayStart = dayStart.TrimStart(' ');
         
             await _settingChat.UpdateDayInChat(chatId, dayStart);
             
             await _dayCallBackService.DayEndPollInChat(client, chatId);
             return;
         }
         
         if (text.Contains("End"))
         {
             var dayEnd = text.Replace("!End_", "");
             dayEnd = dayEnd.TrimEnd(' ');
             dayEnd = dayEnd.TrimStart(' ');
         
             await _settingChat.UpdateDayInChat(chatId, dayOfWeekEnd:dayEnd);
         
         
             if (item.FirstSetSettings)
             {
                 await client.SendTextMessageAsync(chatId, "для ввода гугл токена напишите '!googleToken token'",disableNotification:true);
             }
             return;
         }

         if (text.Contains("googleToken"))
         {
             var token = text.Replace("!googleToken", "");
             token = token.TrimEnd(' ');
             token = token.TrimStart(' ');

             if (await _googleSheets.TestConnection(token))
             {
                 await _settingChat.UpdateToken(chatId, token);
                 if (item.FirstSetSettings)
                 {
                     await client.SendTextMessageAsync(chatId,
                         "Установите временной интервал. Пример: !timeInterval 7:00-14:00 ",disableNotification:true);
                 }
                 else
                 {
                     await client.SendTextMessageAsync(chatId, "Токен обновлен",disableNotification:true);
                 }
             }
             else
             {
                 await client.SendTextMessageAsync(chatId,
                     "ALAAAAAARM, чет пошло не так. токен не прошел проверку/ попробуйте начать настройку самого начала",disableNotification:true);
             }
             return;
         }

         if (text.Contains("timeInterval"))
         {
             var timeInterval = text.Replace("!timeInterval", "");
             timeInterval = timeInterval.TrimEnd(' ');
             timeInterval = timeInterval.TrimStart(' ');
             
           
           if (!await _settingChat.UpdateTimeInterval(chatId, timeInterval))
           {
               await client.SendTextMessageAsync(chatId, "Неправильный синтаксис!",disableNotification:true);
           }
           else
           {
               await client.SendTextMessageAsync(chatId, "настройка окончена",disableNotification:true);
           }

          
           return;
         }

         if (text.Contains("createPoll"))
         {
             await _settingChat.CreatePoll(client, chatId);
         }
    }

    public async Task AdminsCallback(string text,long chatId)
    {
        switch (text.ToLower())
        {
            case "/start":
            {
              break;
            }
            case "/managegroup":
            {
                await _manageGroup.GetAllGroups(chatId);
                break;
            }
        }

        switch (text)
        {
            case { } a when a.Contains("/chat_"):
            {
                var replacedText = a.Replace("/chat_", "");
                var idTg = long.Parse( replacedText.Substring(0, replacedText.IndexOf('/')) );
                var id =long.Parse( replacedText.Remove(0, replacedText.IndexOf('/')+1) );
                
                await _manageGroup.SettingChat(idTg,id);
                break;
            }
            case { } a when a.Contains(""):
            {
                break;
            }
        }
        
    }

   

}
