using Microsoft.EntityFrameworkCore;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class ApplicationContext : DbContext
{
    public DbSet<StructureChat>? Chats { get; set; }
    public DbSet<PollOptions>? PollOptions { get; set; }
    public DbSet<ManageChat>? ManageChats { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // builder.Entity<ShortPoll>()
        //     .HasOne(s => s.StructureChat)
        //     .WithOne(s=>s.Polls)
        //     .HasForeignKey<ShortPoll>(shortPoll=>shortPoll.ChatId)
        //     .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<PollOptions>()
            .HasOne(s => s.Chat)
            .WithMany(s => s.Options)
            .OnDelete(DeleteBehavior.Cascade);

  
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Server=localhost;User Id=postgres;Password=125348220;Port=5432;Database=TGBot_Test;");
        // options.UseNpgsql("Server=localhost;User Id=postgres;Password=125348220;Port=5432;Database=TelegramBot;");
        
    }
}