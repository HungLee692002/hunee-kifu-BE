using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff")]
[Route("api/v1/teacher-assignments")]
public class TeacherAssignmentsController : ApiControllerBase
{
    private readonly IClassAssignmentService _service;

    public TeacherAssignmentsController(IClassAssignmentService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassAssignmentDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClassAssignmentDto>> Update(Guid id, [FromBody] UpdateClassAssignmentRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ClassAssignmentDto>> Patch(Guid id, [FromBody] PatchClassAssignmentRequest request, CancellationToken ct) =>
        Ok(await _service.PatchAsync(id, request, ct));
}
