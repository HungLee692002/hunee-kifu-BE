using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
[Route("api/v1/lesson-sessions")]
public class LessonSessionsController : ApiControllerBase
{
    private readonly IScheduleService _schedule;

    public LessonSessionsController(IScheduleService schedule) => _schedule = schedule;

    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? teacherId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        CancellationToken ct) =>
        Ok(await _schedule.GetSessionsPagedAsync(GetPagedQuery(), classId, teacherId, fromDate, toDate, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LessonSessionDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _schedule.GetSessionByIdAsync(id, ct));
}
