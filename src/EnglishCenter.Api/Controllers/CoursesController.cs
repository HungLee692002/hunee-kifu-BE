using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff")]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult> GetList(CancellationToken ct) =>
        Ok(await _service.GetPagedAsync(GetPagedQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> Update(Guid id, [FromBody] UpdateCourseRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> Patch(Guid id, [FromBody] PatchCourseRequest request, CancellationToken ct) =>
        Ok(await _service.PatchAsync(id, request, ct));
}
