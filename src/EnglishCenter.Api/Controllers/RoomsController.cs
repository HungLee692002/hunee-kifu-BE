using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff")]
public class RoomsController : ApiControllerBase
{
    private readonly IRoomService _service;

    public RoomsController(IRoomService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult> GetList(CancellationToken ct) =>
        Ok(await _service.GetPagedAsync(GetPagedQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomDto>> Update(Guid id, [FromBody] UpdateRoomRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomDto>> Patch(Guid id, [FromBody] PatchRoomRequest request, CancellationToken ct) =>
        Ok(await _service.PatchAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
