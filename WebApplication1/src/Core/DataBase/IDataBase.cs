using WebApplication1.Entity;
namespace WebApplication1.Core.DataBase;

public interface IDataBase
{
    Task<Roll> AddAsync(Roll entity);
    Task<Roll?> GetAsync(int id);
    Task<Roll?> DeleteAsync(int id);
    Task<IEnumerable<Roll>> GetByFilterAsync();
}