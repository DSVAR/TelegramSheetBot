// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
// using TelegramSheetBot.Services;
//
// namespace TelegramSheetBot;
//
// public class Factory : IDesignTimeDbContextFactory<ApplicationContext>
// {
//     public ApplicationContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder();
//
//         optionsBuilder.UseNpgsql(
//             "Server=localhost;User Id=postgres;Password=125348220;Port=5432;Database=TelegramBot;");
//
//         return new ApplicationContext(optionsBuilder.Options);
//     }
// }