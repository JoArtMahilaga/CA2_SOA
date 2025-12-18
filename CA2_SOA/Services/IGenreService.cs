using CA2SOA.Entities;

namespace CA2SOA.Repositories;

public interface IGenreService
{
    Task<IEnumerable<Genre>> GetAllAsync();
    Task<Genre?> GetByIdAsync(int id);
    Task AddAsync(Genre genre);
    Task UpdateAsync(Genre genre);
    Task DeleteAsync(Genre genre);
}