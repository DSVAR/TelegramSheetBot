using Microsoft.EntityFrameworkCore;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class JobWithBd<T> where T:class
{
    private ApplicationContext Application { get; set; }
    private readonly DbSet<T> _dbSet;
    
    public JobWithBd(ApplicationContext application)
    {
        Application = application;
        _dbSet = application.Set<T>();
    }


    public async Task CreateAsync(T obj)
    {
       await Application.AddAsync(obj);
      await Application.SaveChangesAsync();
    }

    public void Delete(T obj)
    {
         Application.Remove(obj);
         Application.SaveChanges();
    }

    public async Task<T> FindAsync(long id)
    {
        return (await  _dbSet.FindAsync(id))!;
    }
    public async Task<T> FindAsync(string id)
    {
        return (await _dbSet.FindAsync(id))!;
    }

    public async Task Update(T obj)
    {
        _dbSet.Update(obj);
        await Application.SaveChangesAsync();
    }


    public async Task<IEnumerable<T>> GetItemsAsync()
    {
        return await _dbSet.ToListAsync();
    }


    public async Task DeleteRangeAsync(IEnumerable<T> list)
    {
        try
        {
            
            _dbSet.RemoveRange(list);
            await Application.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
       
    }



    public async Task AddRangeAsync(List<T> list)
    {
        await _dbSet.AddRangeAsync(list);
        await Application.SaveChangesAsync();
    }
}