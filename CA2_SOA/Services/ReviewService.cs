using CA2SOA.Data;
using CA2SOA.DTOS;
using CA2SOA.Entities;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Services;

public sealed class ReviewService : IReviewService
{
    private readonly GameShelfDbContext _db;

    public ReviewService(GameShelfDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Review>> GetAllAsync(int? gameId = null, int? userId = null)
    {
        var q = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .AsQueryable();

        if (gameId is not null) q = q.Where(r => r.GameId == gameId.Value);
        if (userId is not null) q = q.Where(r => r.UserId == userId.Value);

        return await q.OrderByDescending(r => r.CreatedUtc).ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<(bool Ok, string? Error, Review? Created)> CreateAsync(int userId, CreateReviewRequest req)
    {
        if (req.GameId <= 0) return (false, "Invalid GameId.", null);
        if (req.Rating < 1 || req.Rating > 10) return (false, "Rating must be between 1 and 10.", null);

        var comment = (req.Comment ?? "").Trim();
        if (comment.Length > 1000) return (false, "Comment too long (max 1000).", null);

        var gameExists = await _db.Games.AnyAsync(g => g.Id == req.GameId);
        if (!gameExists) return (false, "Game not found.", null);

        var already = await _db.Reviews.AnyAsync(r => r.UserId == userId && r.GameId == req.GameId);
        if (already) return (false, "You already reviewed this game.", null);

        var review = new Review
        {
            UserId = userId,
            GameId = req.GameId,
            Rating = req.Rating,
            Comment = comment,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var created = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        return (true, null, created);
    }

    public async Task<(bool Ok, string? Error)> UpdateAsync(int id, int userId, UpdateReviewRequest req)
    {
        if (req.Rating < 1 || req.Rating > 10) return (false, "Rating must be between 1 and 10.");

        var comment = (req.Comment ?? "").Trim();
        if (comment.Length > 1000) return (false, "Comment too long (max 1000).");

        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null) return (false, "NotFound");

        if (review.UserId != userId) return (false, "Forbidden");

        review.Rating = req.Rating;
        review.Comment = comment;
        review.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(int id, int userId)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null) return (false, "NotFound");

        if (review.UserId != userId) return (false, "Forbidden");

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        return (true, null);
    }
}
