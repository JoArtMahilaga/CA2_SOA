using CA2SOA.Entities;

namespace CA2SOA.Repositories;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);

    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);

    Task<bool> ExistsByDisplayNameAsync(string displayName);

    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByEmailAsync(string email);
}