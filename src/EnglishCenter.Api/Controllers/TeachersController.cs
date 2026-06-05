using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff")]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;
    private readonly ISalaryService _salary;

    public TeachersController(ITeacherService service, ISalaryService salary)
    {
        _service = service;
        _salary = salary;
    }

    [HttpGet]
    public async Task<ActionResult> GetList(CancellationToken ct) =>
        Ok(await _service.GetPagedAsync(GetPagedQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeacherDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeacherDto>> Create([FromBody] CreateTeacherRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeacherDto>> Update(Guid id, [FromBody] UpdateTeacherRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, request, ct));

    [HttpGet("{id:guid}/lesson-rates")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult> GetLessonRates(Guid id, CancellationToken ct) =>
        Ok(await _service.GetLessonRatesAsync(id, ct));

    [HttpPost("{id:guid}/lesson-rates")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<TeacherLessonRateDto>> AddLessonRate(Guid id, [FromBody] CreateTeacherLessonRateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateLessonRateAsync(id, request, ct);
        return Created($"/api/v1/teachers/{id}/lesson-rates", created);
    }

    [HttpGet("{id:guid}/lesson-pay-records")]
    [Authorize(Roles = "Admin,Accountant,Teacher")]
    public async Task<ActionResult> GetLessonPayRecords(Guid id, CancellationToken ct) =>
        Ok(await _salary.GetLessonPayRecordsByTeacherAsync(id, GetPagedQuery(), ct));
}
