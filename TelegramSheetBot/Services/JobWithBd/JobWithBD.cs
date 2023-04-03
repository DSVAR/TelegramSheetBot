using Microsoft.EntityFrameworkCore;
using TelegramSheetBot.Entity;
using TelegramSheetBot.Interfaces;

namespace TelegramSheetBot.Services.JobWithBd;

public class JobWithBd<T> : IJobWithBd<T> where T : class
{
    private ApplicationContext Application { get; set; }
    private readonly DbSet<T> _dbSet;

    public JobWithBd(ApplicationContext application)
    {
        Application = application;
        _dbSet = application.Set<T>();
    }

    /// <summary>
    /// добавление в бд
    /// </summary>
    /// <param name="obj"></param>
    public async Task CreateAsync(T obj)
    {
        await Application.AddAsync(obj);
        await Application.SaveChangesAsync();
    }

    /// <summary>
    /// удаление из бд
    /// </summary>
    /// <param name="obj"></param>
    public void Delete(T obj)
    {
        Application.Remove(obj);
        Application.SaveChanges();
    }

    /// <summary>
    /// поиск по id в бд
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<T> FindAsync(long id)
    {
        return (await _dbSet.FindAsync(id))!;
    }

    /// <summary>
    /// обновление бд
    /// </summary>
    /// <param name="obj"></param>
    public async Task Update(T obj)
    {
        _dbSet.Update(obj);
        await Application.SaveChangesAsync();
    }

    /// <summary>
    /// получение таблицы
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetItemsAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// удаление массивом
    /// </summary>
    /// <param name="list"></param>
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


    /// <summary>
    /// добавление массивом
    /// </summary>
    /// <param name="list"></param>
    public async Task AddRangeAsync(List<T> list)
    {
        await _dbSet.AddRangeAsync(list);
        await Application.SaveChangesAsync();
    }
}