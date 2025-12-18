using CA2SOA.Entities;
using CA2SOA.DTOS;

namespace CA2SOA.Services;

public interface IReviewService
{
    Task<IEnumerable<Review>> GetAllAsync(int? gameId = null, int? userId = null);
    Task<Review?> GetByIdAsync(int id);

    Task<(bool Ok, string? Error, Review? Created)> CreateAsync(int userId, CreateReviewRequest req);
    Task<(bool Ok, string? Error)> UpdateAsync(int id, int userId, UpdateReviewRequest req);
    Task<(bool Ok, string? Error)> DeleteAsync(int id, int userId);
}