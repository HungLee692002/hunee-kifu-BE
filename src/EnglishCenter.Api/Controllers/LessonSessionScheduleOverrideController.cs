using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
[Route("api/v1/lesson-sessions/{sessionId:guid}/schedule-override")]
public class LessonSessionScheduleOverrideController : ControllerBase
{
    private readonly IScheduleService _service;

    public LessonSessionScheduleOverrideController(IScheduleService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<LessonScheduleOverrideDto>> Get(Guid sessionId, CancellationToken ct)
    {
        var result = await _service.GetOverrideAsync(sessionId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<LessonScheduleOverrideDto>> Put(
        Guid sessionId,
        [FromBody] UpsertLessonScheduleOverrideRequest request,
        CancellationToken ct) =>
        Ok(await _service.UpsertOverrideAsync(sessionId, request, ct));

    [HttpDelete]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<IActionResult> Delete(Guid sessionId, CancellationToken ct)
    {
        await _service.DeleteOverrideAsync(sessionId, ct);
        return NoContent();
    }
}
