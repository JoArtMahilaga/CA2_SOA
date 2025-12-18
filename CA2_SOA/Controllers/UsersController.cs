using CA2SOA.DTOS;
using CA2SOA.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var items = await _users.GetAllAsync();
        return Ok(items.Select(u => new UserDto(u.Id, u.UserName, u.Email, u.CreatedUtc)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var u = await _users.GetByIdAsync(id);
        if (u is null) return NotFound();
        return Ok(new UserDto(u.Id, u.UserName, u.Email, u.CreatedUtc));
    }
}