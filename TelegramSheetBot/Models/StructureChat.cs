using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramSheetBot.Models;

public class StructureChat
{
    [Key]
     public long ChatId { get; set; }
     public string? NameChat { get; set; }
    public bool CreatedPoll { get; set; }
    public bool CreatedRequestPoll { get; set; }
    public bool CreatedPollThisWeek { get; set; }
    public bool FirstSetSettings { get; set; } = true;
    public string? TimeIntervalStart { get; set; }
    public string? TimeIntervalEnd { get; set; }
    
    public string? TimeStartPoll { get; set; }
    public string? TimeEndPoll { get; set; }
    public DateTime LastChangeTime { get; set; }
    
    public string? GoogleSheetToken { get; set; }
    
    public string? DayOfWeekStartPoll { get; set; }
    public string? DayOfWeekEndPoll { get; set; }
    public bool CanStartPoll { get; set; }
    public List<string>? ListSheet { get; set; }

    public int IdMessageLastPoll { get; set; }
    
    public string? PollId { get; set; }
    // public ShortPoll? Polls { get; set; }
    public List<PollOptions>? Options { get; set; }
    // public List<MyPollOptions>? Options { get; set; }
    
    
}