using CA2SOA.Data;
using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Repositories;

public sealed class GameService : IGameService
{
    private readonly GameShelfDbContext _db;

    public GameService(GameShelfDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _db.Games
            .AsNoTracking()
            .Include(g => g.Genre)
            .OrderBy(g => g.Id)
            .ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _db.Games
            .AsNoTracking()
            .Include(g => g.Genre)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task AddAsync(Game game)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        _db.Games.Update(game);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Game game)
    {
        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
    }
}