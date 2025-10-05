namespace Draw.it.Server.Repositories;

public interface IRepository<T, TId> where T : class
{
    void Save(T entity);
    void Delete(T entity);
    T? GetById(TId id);
    IEnumerable<T> GetAll();
}
