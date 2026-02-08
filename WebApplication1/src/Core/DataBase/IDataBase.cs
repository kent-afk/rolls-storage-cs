using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Statistics;

namespace WebApplication1.Core.DataBase;

public interface IDataBase
{
    Task<Roll> AddAsync(Roll entity);
    Task<Roll?> GetAsync(int id);
    Task<Roll?> DeleteAsync(int id);
    Task<IEnumerable<Roll>> GetByFilterAsync(FilterRolls filter);
    Task<StatisticResponse> GetStatisticAsync(StatisticRequest request);
}