using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize(Roles = "Admin,AcademicStaff,Receptionist")]
public class EnrollmentsController : ControllerBase
{
    private readonly IClassService _classService;

    public EnrollmentsController(IClassService classService) => _classService = classService;

    [HttpGet("{enrollmentId:guid}")]
    public async Task<ActionResult<EnrollmentDto>> GetById(Guid enrollmentId, CancellationToken ct) =>
        Ok(await _classService.GetEnrollmentByIdAsync(enrollmentId, ct));

    [HttpPatch("{enrollmentId:guid}")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<EnrollmentDto>> Patch(Guid enrollmentId, [FromBody] PatchEnrollmentRequest request, CancellationToken ct) =>
        Ok(await _classService.PatchEnrollmentAsync(enrollmentId, request, ct));
}
