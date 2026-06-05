using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
[Route("api/v1/assessments")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentsController(IAssessmentService service) => _service = service;

    [HttpGet("{id:guid}/grades")]
    public async Task<ActionResult> GetGrades(Guid id, CancellationToken ct) =>
        Ok(await _service.GetGradesAsync(id, ct));

    [HttpPut("{id:guid}/grades")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult> ReplaceGrades(Guid id, [FromBody] ReplaceGradesRequest request, CancellationToken ct) =>
        Ok(await _service.ReplaceGradesAsync(id, request, ct));
}
