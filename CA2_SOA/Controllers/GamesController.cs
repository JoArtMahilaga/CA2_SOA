using CA2SOA.DTOS;
using CA2SOA.Entities;
using CA2SOA.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameService _games;

    public GamesController(IGameService games)
    {
        _games = games;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
    {
        var items = await _games.GetAllAsync();
        return Ok(items.Select(g =>
            new GameDto(g.Id, g.Title, g.Platform, g.GenreId, g.Genre?.Name ?? "")));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDto>> GetById(int id)
    {
        var g = await _games.GetByIdAsync(id);
        if (g is null) return NotFound();

        return Ok(new GameDto(g.Id, g.Title, g.Platform, g.GenreId, g.Genre?.Name ?? ""));
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create(CreateGameRequest req)
    {
        var title = (req.Title ?? "").Trim();
        var platform = (req.Platform ?? "").Trim();

        if (title.Length < 2) return BadRequest(new { message = "Title too short." });
        if (platform.Length < 2) return BadRequest(new { message = "Platform too short." });

        var game = new Game
        {
            Title = title,
            Platform = platform,
            GenreId = req.GenreId
        };

        await _games.AddAsync(game);

        var created = await _games.GetByIdAsync(game.Id);
        return CreatedAtAction(nameof(GetById), new { id = game.Id },
            new GameDto(created!.Id, created.Title, created.Platform, created.GenreId, created.Genre?.Name ?? ""));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGameRequest req)
    {
        var game = await _games.GetByIdAsync(id);
        if (game is null) return NotFound();

        var title = (req.Title ?? "").Trim();
        var platform = (req.Platform ?? "").Trim();

        if (title.Length < 2) return BadRequest(new { message = "Title too short." });
        if (platform.Length < 2) return BadRequest(new { message = "Platform too short." });

        game.Title = title;
        game.Platform = platform;
        game.GenreId = req.GenreId;

        await _games.UpdateAsync(game);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _games.GetByIdAsync(id);
        if (game is null) return NotFound();

        await _games.DeleteAsync(game);
        return NoContent();
    }
}
