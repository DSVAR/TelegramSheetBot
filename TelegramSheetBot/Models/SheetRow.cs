using System.ComponentModel.DataAnnotations;
using Google.Apis.Sheets.v4.Data;

namespace TelegramSheetBot.Models;

public class SheetRow
{
    
    public string? Name { get; set; }
    public Color? ColorBd { get; set; }
}