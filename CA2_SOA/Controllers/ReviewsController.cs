using System.Security.Claims;
using CA2SOA.DTOS;
using CA2SOA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAll([FromQuery] int? gameId, [FromQuery] int? userId)
    {
        var items = await _reviews.GetAllAsync(gameId, userId);
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        var r = await _reviews.GetByIdAsync(id);
        if (r is null) return NotFound();
        return Ok(ToDto(r));
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest req)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var (ok, error, created) = await _reviews.CreateAsync(userId.Value, req);
        if (!ok)
        {
            if (error == "Game not found.") return NotFound(new { message = error });
            if (error == "You already reviewed this game.") return Conflict(new { message = error });
            return BadRequest(new { message = error });
        }

        return CreatedAtAction(nameof(GetById), new { id = created!.Id }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReviewRequest req)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var (ok, error) = await _reviews.UpdateAsync(id, userId.Value, req);
        if (!ok)
        {
            if (error == "NotFound") return NotFound();
            if (error == "Forbidden") return Forbid();
            return BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var (ok, error) = await _reviews.DeleteAsync(id, userId.Value);
        if (!ok)
        {
            if (error == "NotFound") return NotFound();
            if (error == "Forbidden") return Forbid();
            return BadRequest(new { message = error });
        }

        return NoContent();
    }

    private int? GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : null;
    }

    private static ReviewDto ToDto(CA2SOA.Entities.Review r)
    {
        return new ReviewDto(
            r.Id,
            r.GameId,
            r.Game?.Title ?? "",
            r.UserId,
            r.User?.UserName ?? "",
            r.Rating,
            r.Comment,
            r.CreatedUtc,
            r.UpdatedUtc
        );
    }
}
