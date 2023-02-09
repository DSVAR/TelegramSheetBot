using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramSheetBot;

public class GlobalValues
{
   public static readonly string ChatsEnvironment = $"{Environment.CurrentDirectory}/ChatsId/";
   public static readonly string ChatEnvironmentParent = $"{Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName}";
   
   
   
   public static readonly string SmileNumberZero=char.ConvertFromUtf32(0x0030) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberFirst=char.ConvertFromUtf32(0x0031) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberSecond=char.ConvertFromUtf32(0x0032) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberFourth=char.ConvertFromUtf32(0x0033) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberFifth=char.ConvertFromUtf32(0x0034) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberSixth=char.ConvertFromUtf32(0x0035) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberSeventh=char.ConvertFromUtf32(0x0036) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberEight=char.ConvertFromUtf32(0x0037) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberNinth=char.ConvertFromUtf32(0x0038) +char.ConvertFromUtf32(0xFE0F)+char.ConvertFromUtf32(0x20E3);
   public static readonly string SmileNumberTenth=char.ConvertFromUtf32(0x1F51F);
   public static readonly string SmileStar=char.ConvertFromUtf32(0x2B50);
   
   
   
   
  
}