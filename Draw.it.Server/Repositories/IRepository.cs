namespace Draw.it.Server.Repositories;

public interface IRepository<T, TId> where T : class
{
    void Save(T entity);
    bool DeleteById(TId id);
    T? FindById(TId id);
    IEnumerable<T> GetAll();
}
