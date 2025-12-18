using CA2SOA.Data;
using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Repositories;

public sealed class UserService : IUserService
{
    private readonly GameShelfDbContext _db;

    public UserService(GameShelfDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        var name = (userName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return null;

        return await _db.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == name.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var e = (email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(e)) return null;

        return await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == e.ToLower());
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByDisplayNameAsync(string displayName)
    {
        var name = (displayName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return false;

        return await _db.Users.AnyAsync(u => u.UserName.ToLower() == name.ToLower());
    }
}