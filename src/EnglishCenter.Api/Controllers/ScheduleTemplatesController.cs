using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
[Route("api/v1/schedule-templates")]
public class ScheduleTemplatesController : ApiControllerBase
{
    private readonly IScheduleService _service;

    public ScheduleTemplatesController(IScheduleService service) => _service = service;

    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<ActiveScheduleTemplateDto>>> GetActive(CancellationToken ct) =>
        Ok(await _service.GetActiveTemplatesAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WeeklyScheduleTemplateDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetTemplateByIdAsync(id, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<WeeklyScheduleTemplateDto>> Update(Guid id, [FromBody] UpdateWeeklyScheduleTemplateRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateTemplateAsync(id, request, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<WeeklyScheduleTemplateDto>> Patch(Guid id, [FromBody] PatchWeeklyScheduleTemplateRequest request, CancellationToken ct) =>
        Ok(await _service.PatchTemplateAsync(id, request, ct));
}
