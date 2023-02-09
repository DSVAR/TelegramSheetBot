using Telegram.Bot.Types;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class PollService
{
   
    private JobWithBd<StructureChat>? JobWithBdChat { get; set; }
    private JobWithBd<PollOptions>? JobWithBdPollOptions { get; set; }

    public PollService(JobWithBd<StructureChat>? jobWithBdChat,JobWithBd<PollOptions>? jobWithBdPollOptions)
    {
        JobWithBdChat = jobWithBdChat;
        JobWithBdPollOptions = jobWithBdPollOptions;
    }


    public async Task AddPoll( Poll poll)
    {
        var listOptionsBd = (await JobWithBdPollOptions!.GetItemsAsync()).ToList();

        var pollOptions = listOptionsBd.Any() ? listOptionsBd.Where(pollO => pollO.PollId == poll.Id) : listOptionsBd;

        var listOptions = new List<PollOptions>();
        var chat =(await JobWithBdChat!.GetItemsAsync()).First(ch => ch.PollId==poll.Id);
        
       

        foreach (var item in poll.Options)
        {
            listOptions.Add(new ()
            {
                PollId = poll.Id,
                Name = item.Text,
                ChatId = chat.ChatId,
                VoterCount = item.VoterCount
            });
        }
        
        
        if ((pollOptions) != null)
        {
            //очистить и перезаписать
            await JobWithBdPollOptions.DeleteRangeAsync(pollOptions);
            await JobWithBdPollOptions.AddRangeAsync(listOptions);
        }
        else
        {
            await JobWithBdPollOptions.AddRangeAsync(listOptions);
        }

    }

}