using CA2SOA.Data;
using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Repositories;

public sealed class LibraryEntryService : ILibraryEntryService
{
    private readonly GameShelfDbContext _db;

    public LibraryEntryService(GameShelfDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<LibraryEntry>> GetForUserAsync(int userId)
    {
        return await _db.LibraryEntries
            .AsNoTracking()
            .Include(e => e.Game)
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<LibraryEntry?> GetByIdAsync(int id)
    {
        return await _db.LibraryEntries
            .Include(e => e.Game)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(LibraryEntry entry)
    {
        _db.LibraryEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(LibraryEntry entry)
    {
        _db.LibraryEntries.Update(entry);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(LibraryEntry entry)
    {
        _db.LibraryEntries.Remove(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsForUserGameAsync(int userId, int gameId)
    {
        return await _db.LibraryEntries.AnyAsync(e => e.UserId == userId && e.GameId == gameId);
    }
}