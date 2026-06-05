using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/v1/student-tuition-months")]
[Authorize(Roles = "Admin,Accountant,Receptionist")]
public class StudentTuitionMonthsController : ApiControllerBase
{
    private readonly ITuitionService _service;

    public StudentTuitionMonthsController(ITuitionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? status,
        [FromQuery] Guid? classId,
        CancellationToken ct)
    {
        var filter = new StudentTuitionMonthListFilter(year, month, status, ClassId: classId);
        return Ok(await _service.GetTuitionMonthsPagedAsync(GetPagedQuery(), filter, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentTuitionMonthDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetTuitionMonthByIdAsync(id, ct));
}
