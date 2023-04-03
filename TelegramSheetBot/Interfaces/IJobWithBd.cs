namespace TelegramSheetBot.Interfaces;

public interface IJobWithBd<T> where T :class
{
    /// <summary>
    /// добавление в бд
    /// </summary>
    /// <param name="obj"></param>
    public  Task CreateAsync(T obj);
    /// <summary>
    /// удаление из бд
    /// </summary>
    /// <param name="obj"></param>
    public void Delete(T obj);
    /// <summary>
    /// поиск по id в бд
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public  Task<T> FindAsync(long id);
    /// <summary>
    /// обновление бд
    /// </summary>
    /// <param name="obj"></param>
    public  Task Update(T obj);
    /// <summary>
    /// получение таблицы
    /// </summary>
    /// <returns></returns>
    public  Task<IEnumerable<T>> GetItemsAsync();
    /// <summary>
    /// удаление массивом
    /// </summary>
    /// <param name="list"></param>
    public  Task DeleteRangeAsync(IEnumerable<T> list);
    /// <summary>
    /// добавление массивом
    /// </summary>
    /// <param name="list"></param>
    public  Task AddRangeAsync(List<T> list);
}