namespace WebApplication1.Core.DataBase;

public interface IDataBaseFactory
{
    IDataBase Create();
    public void Configure(string connectionString, string databaseName);
}