using CA2SOA.DTOS;
using CA2SOA.Entities;
using CA2SOA.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/genres")]
public sealed class GenresController : ControllerBase
{
    private readonly IGenreService _genres;

    public GenresController(IGenreService genres)
    {
        _genres = genres;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAll()
    {
        var items = await _genres.GetAllAsync();
        return Ok(items.Select(g => new GenreDto(g.Id, g.Name)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenreDto>> GetById(int id)
    {
        var g = await _genres.GetByIdAsync(id);
        if (g is null) return NotFound();
        return Ok(new GenreDto(g.Id, g.Name));
    }

    [HttpPost]
    public async Task<ActionResult<GenreDto>> Create(CreateGenreRequest req)
    {
        var name = (req.Name ?? "").Trim();
        if (name.Length < 2) return BadRequest(new { message = "Name too short." });

        var genre = new Genre { Name = name };
        await _genres.AddAsync(genre);

        return CreatedAtAction(nameof(GetById), new { id = genre.Id }, new GenreDto(genre.Id, genre.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGenreRequest req)
    {
        var genre = await _genres.GetByIdAsync(id);
        if (genre is null) return NotFound();

        var name = (req.Name ?? "").Trim();
        if (name.Length < 2) return BadRequest(new { message = "Name too short." });

        genre.Name = name;
        await _genres.UpdateAsync(genre);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _genres.GetByIdAsync(id);
        if (genre is null) return NotFound();

        await _genres.DeleteAsync(genre);
        return NoContent();
    }
}