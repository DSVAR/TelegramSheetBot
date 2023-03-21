using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Models;
using TelegramSheetBot.Services.JobWithBd;

namespace TelegramSheetBot.Services;

public class PollService
{

    private readonly IJobWithBd<StructureChat>? _jobWithBdChat;
    private readonly IJobWithBd<PollOptions>? _jobWithBdPollOptions;
    private readonly TelegramBotClient _client;
    private readonly GoogleSheets _sheets;
    private readonly FindingService _findingService;
    

    public PollService(IJobWithBd<StructureChat>? jobWithBdChat,IJobWithBd<PollOptions>? jobWithBdPollOptions,
        TelegramBotClient client,GoogleSheets sheets,FindingService findingService)
    {
        _jobWithBdChat = jobWithBdChat;
        _jobWithBdPollOptions = jobWithBdPollOptions;
        _client = client;
        _sheets = sheets;
        _findingService = findingService;
    }


    public async Task AddPoll( Poll poll)
    {
        try
        {
            var task= _findingService.SChatFindByPollIdAsync(poll.Id);
            var listOptionsBd = (await _jobWithBdPollOptions!.GetItemsAsync()).ToList();

            var pollOptions = listOptionsBd.Any() ? 
                listOptionsBd.Where(pollO => pollO.PollId == poll.Id) : listOptionsBd;

            var listOptions = new List<PollOptions>();
            var chat = await task;
           // var chat =(await _jobWithBdChat!.GetItemsAsync()).FirstOrDefault(ch => ch.PollId==poll.Id);

            foreach (var item in poll.Options)
            {
                listOptions.Add(new ()
                {
                    PollId = poll.Id,
                    Name = item.Text,
                    ChatId = chat!.ChatId,
                    VoterCount = item.VoterCount
                });
            }
        
        
            if ((pollOptions) != null)
            {
                //очистить и перезаписать
                await _jobWithBdPollOptions.DeleteRangeAsync(pollOptions);
                await _jobWithBdPollOptions.AddRangeAsync(listOptions);
            }
            else
            {
                await _jobWithBdPollOptions.AddRangeAsync(listOptions);
            }
        }
        catch (Exception ex)
        {
           Console.WriteLine(ex.Message); 
        }

    }


    public async Task StartPoll(long idChat,DateTime endPollTime)
    {
        var chat =await _jobWithBdChat!.FindAsync(idChat);

        var googleList = (await _sheets.ListForPoll(chat.GoogleSheetToken!)).Select(x=>x.Name).ToList();

        var message = await _client.SendPollAsync(chat.ChatId, "голосование", googleList!,
            allowsMultipleAnswers: true, isAnonymous: false);
        
        if (googleList.Any())
        {
            chat.CreatedRequestPoll = true;
            chat.CreatedPoll = true;
            chat.IdMessageLastPoll = message.MessageId;
            chat.PollId = message.Poll!.Id;
                    
            chat.LastChangeTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            chat.StartedPollForced = true;
            chat.TimeEndForcePoll = DateTime.SpecifyKind(endPollTime, DateTimeKind.Utc);

            await  _jobWithBdChat.Update(chat);
        }

    }


    public async Task EndPoll(long idChat)
    {
       
        var chat = await _jobWithBdChat!.FindAsync(idChat);
        var task = _findingService.FindPollsByIdAsync(chat.ChatId);
        //закрытие голосование
        chat.CreatedRequestPoll = false;
        chat.CreatedPoll = false;

        chat.CreatedPollThisWeek = true;

        chat.TimeEndPoll = "";
        chat.TimeStartPoll = "";
        chat.StartedPollForced = false;
        chat.TimeEndForcePoll = DateTime.SpecifyKind(new DateTime(), DateTimeKind.Utc);
        var options = await task;
        
       // var options = (await _jobWithBdPollOptions!.GetItemsAsync()).Where(i => i.ChatId == chat.ChatId).ToList();

        if (options.Count > 0)
        {
            var max = options.Max(i => i.VoterCount);
            var listMax = (options.Where(i => i.VoterCount == max)).ToList();

            var rnd = new Random();
            string name = "";

            if (listMax.Count() > 1)
            {
                var ind = rnd.Next(listMax.Count);
                name = listMax[ind].Name!;
            }

            if (listMax.Count == 1)
            {
                name = listMax.First().Name!;
            }

            await _sheets.BanSystem(chat.GoogleSheetToken!, name);


            await _jobWithBdChat.Update(chat);

            await _client.StopPollAsync(chat.ChatId, chat.IdMessageLastPoll);
            await _client.SendTextMessageAsync(chat.ChatId, $"закрытие голосования! Победитель:{name}");


            await _jobWithBdPollOptions!.DeleteRangeAsync(options);
        }
    }

}