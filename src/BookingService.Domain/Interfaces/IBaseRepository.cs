namespace BookingService.Domain.Interfaces;

public interface IBaseRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByUserIdAsync(Guid userId);
    Task<Guid> AddAsync(T booking);
    void Update(T booking);
    void Delete(T booking);
    Task SaveChangesAsync(); 
}