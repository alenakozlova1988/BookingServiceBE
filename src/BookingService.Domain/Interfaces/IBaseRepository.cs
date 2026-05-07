namespace BookingService.Domain.Interfaces;

public interface IBaseRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByUserIdAsync(Guid userId);
    Task<Guid> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync(); 
}
