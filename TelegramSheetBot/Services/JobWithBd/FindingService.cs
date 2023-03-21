using TelegramSheetBot.Interfaces;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services.JobWithBd;

public class FindingService
{
    private readonly IJobWithBd<StructureChat> _jobWithBdStructure;
    private readonly IJobWithBd<PollOptions> _jobWithBdOptions;
    private readonly IJobWithBd<ManageChat> _jobWithBdManageChat;
    public FindingService(IJobWithBd<StructureChat> jobWithBdStructure,IJobWithBd<PollOptions> jobWithBdOptions, 
        IJobWithBd<ManageChat> jobWithBdManageChat)
    {
        _jobWithBdStructure = jobWithBdStructure;
        _jobWithBdOptions = jobWithBdOptions;
        _jobWithBdManageChat = jobWithBdManageChat;
    }

    public async Task<StructureChat?> SChatFindByPollIdAsync(string pollId)
    {
        return (await _jobWithBdStructure.GetItemsAsync()).FirstOrDefault(ch => ch.PollId == pollId);
    }
    public async Task<StructureChat?> SChatFindByChatIdAsync(long chatId)
    {
        return (await _jobWithBdStructure.GetItemsAsync()).FirstOrDefault(c=>c.ChatId==chatId);
    }


    public async Task<List<PollOptions>> FindPollsByIdAsync(long chatId)
    {
        return (await _jobWithBdOptions!.GetItemsAsync()).Where(i => i.ChatId == chatId).ToList();
    }


    public async Task<ManageChat?> MChatFindByIdAsync(long id)
    {
        return (await _jobWithBdManageChat.GetItemsAsync()).FirstOrDefault(m => m.ChatId == id);
    }
}