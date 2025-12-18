using CA2SOA.Entities;

namespace CA2SOA.Repositories;

public interface ILibraryEntryService
{
    Task<IEnumerable<LibraryEntry>> GetForUserAsync(int userId);
    Task<LibraryEntry?> GetByIdAsync(int id);
    Task AddAsync(LibraryEntry entry);
    Task UpdateAsync(LibraryEntry entry);
    Task DeleteAsync(LibraryEntry entry);
    Task<bool> ExistsForUserGameAsync(int userId, int gameId);
}