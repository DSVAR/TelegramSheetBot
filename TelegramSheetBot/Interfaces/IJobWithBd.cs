namespace TelegramSheetBot.Interfaces;

public interface IJobWithBd<T> where T :class
{
    public  Task CreateAsync(T obj);

    public void Delete(T obj);

    public  Task<T> FindAsync(long id);

    public  Task<T> FindAsync(string id);

    public  Task Update(T obj);

    public  Task<IEnumerable<T>> GetItemsAsync();

    public  Task DeleteRangeAsync(IEnumerable<T> list);

    public  Task AddRangeAsync(List<T> list);
}