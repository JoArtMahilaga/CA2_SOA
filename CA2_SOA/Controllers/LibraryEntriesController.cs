using System.Security.Claims;
using CA2SOA.DTOS;
using CA2SOA.Entities;
using CA2SOA.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/libraryentries")]
public sealed class LibraryEntriesController : ControllerBase
{
    private readonly ILibraryEntryService _entries;

    public LibraryEntriesController(ILibraryEntryService entries)
    {
        _entries = entries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<LibraryEntryDto>>> GetMine()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var items = await _entries.GetForUserAsync(userId.Value);
        return Ok(items.Select(e =>
            new LibraryEntryDto(
                e.Id,
                e.GameId,
                e.Game?.Title ?? "",
                e.Game?.Platform ?? "",
                e.Status,
                e.CreatedUtc
            )));
    }

    [HttpPost("mine")]
    public async Task<ActionResult<LibraryEntryDto>> AddMine([FromBody] CreateLibraryEntryRequest req)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (await _entries.ExistsForUserGameAsync(userId.Value, req.GameId))
            return Conflict(new { message = "Game already in your library." });

        var entry = new LibraryEntry
        {
            UserId = userId.Value,
            GameId = req.GameId,
            Status = req.Status,
            CreatedUtc = DateTime.UtcNow
        };

        await _entries.AddAsync(entry);

        var created = await _entries.GetByIdAsync(entry.Id);
        return Ok(new LibraryEntryDto(
            created!.Id,
            created.GameId,
            created.Game?.Title ?? "",
            created.Game?.Platform ?? "",
            created.Status,
            created.CreatedUtc
        ));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLibraryEntryRequest req)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _entries.GetByIdAsync(id);

        if (entry is null)
        {
            var mine = await _entries.GetForUserAsync(userId.Value);
            entry = mine.FirstOrDefault(e => e.GameId == id);
            if (entry is null) return NotFound();
        }
        else
        {
            if (entry.UserId != userId.Value) return Forbid();
        }

        entry.Status = req.Status;
        await _entries.UpdateAsync(entry);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _entries.GetByIdAsync(id);
        if (entry is null) return NotFound();
        if (entry.UserId != userId.Value) return Forbid();

        await _entries.DeleteAsync(entry);
        return NoContent();
    }

    private int? GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : null;
    }
}
