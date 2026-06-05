using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,Accountant")]
[Route("api/v1/salary-periods")]
public class SalaryPeriodsController : ApiControllerBase
{
    private readonly ISalaryService _service;

    public SalaryPeriodsController(ISalaryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct) =>
        Ok(await _service.GetPeriodsPagedAsync(GetPagedQuery(), year, month, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalaryPeriodDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetPeriodByIdAsync(id, ct));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SalaryPeriodDto>> Patch(Guid id, [FromBody] PatchSalaryPeriodRequest request, CancellationToken ct) =>
        Ok(await _service.PatchPeriodAsync(id, request, ct));

    [HttpGet("{id:guid}/lesson-pay-records")]
    public async Task<ActionResult> GetLessonPayRecords(Guid id, CancellationToken ct) =>
        Ok(await _service.GetLessonPayRecordsByPeriodAsync(id, GetPagedQuery(), ct));
}
