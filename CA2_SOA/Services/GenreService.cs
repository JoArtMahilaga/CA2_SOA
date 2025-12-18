using CA2SOA.Data;
using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Repositories;

public sealed class GenreService : IGenreService
{
    private readonly GameShelfDbContext _db;

    public GenreService(GameShelfDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Genre>> GetAllAsync()
    {
        return await _db.Genres
            .AsNoTracking()
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    public async Task<Genre?> GetByIdAsync(int id)
    {
        return await _db.Genres
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task AddAsync(Genre genre)
    {
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Genre genre)
    {
        _db.Genres.Update(genre);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Genre genre)
    {
        _db.Genres.Remove(genre);
        await _db.SaveChangesAsync();
    }
}