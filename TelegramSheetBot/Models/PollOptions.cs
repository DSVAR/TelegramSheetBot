using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramSheetBot.Models;

public class PollOptions
{
    [Key]
    public Guid Id { get; set; }
    public string? Name { get; set; } 
    public int VoterCount { get; set; }
    
    public string? PollId { get; set; }
    [ForeignKey("ChatId")]
    public long ChatId { get; set; }
    public virtual StructureChat? Chat { get; set; } 
}