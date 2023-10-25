using Microsoft.EntityFrameworkCore;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Entity;

public class ApplicationContext : DbContext
{
    public DbSet<StructureChat>? Chats { get; set; }
    public DbSet<PollOptions>? PollOptions { get; set; }
    public DbSet<ManageChat>? ManageChats { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
        builder.Entity<PollOptions>()
            .HasOne(s => s.Chat)
            .WithMany(s => s.Options)
            .OnDelete(DeleteBehavior.Cascade);  
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        //options.UseNpgsql("Server=localhost;User Id=postgres;Password=125348220;Port=5432;Database=Test_TG;"); 
        options.UseNpgsql("Server=localhost;User Id=postgres;Password=125348220;Port=5432;Database=TGBot_Test;");
        
    }
}