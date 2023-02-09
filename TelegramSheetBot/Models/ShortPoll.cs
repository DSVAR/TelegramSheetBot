using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TelegramSheetBot.Models;

public class ShortPolls
{
    [Key]
    public Guid Id { get; set; }
    
    public string? PollId { get; set; }

   
    public virtual long ChatId { get; set; }
    public virtual StructureChat? StructureChat { get; set; } = null!;
    
    public List<PollOptions>? PollOptions { get; set; }
}