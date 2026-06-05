using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
[Route("api/v1/lesson-sessions/{sessionId:guid}")]
public class LessonSessionAttendancesController : ControllerBase
{
    private readonly IAttendanceService _service;

    public LessonSessionAttendancesController(IAttendanceService service) => _service = service;

    [HttpGet("student-attendances")]
    public async Task<ActionResult> GetStudentAttendances(Guid sessionId, CancellationToken ct) =>
        Ok(await _service.GetStudentAttendancesAsync(sessionId, ct));

    [HttpPut("student-attendances")]
    public async Task<ActionResult> PutStudentAttendances(
        Guid sessionId,
        [FromBody] UpsertStudentAttendancesRequest request,
        CancellationToken ct) =>
        Ok(await _service.UpsertStudentAttendancesAsync(sessionId, request, ct));

    [HttpPut("teacher-attendances")]
    public async Task<ActionResult> PutTeacherAttendances(
        Guid sessionId,
        [FromBody] UpsertTeacherAttendancesRequest request,
        CancellationToken ct) =>
        Ok(await _service.UpsertTeacherAttendancesAsync(sessionId, request, ct));
}
