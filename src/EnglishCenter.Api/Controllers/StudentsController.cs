using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Receptionist")]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ITuitionService _tuitionService;

    public StudentsController(IStudentService studentService, ITuitionService tuitionService)
    {
        _studentService = studentService;
        _tuitionService = tuitionService;
    }

    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] string? search,
        [FromQuery] string? status,
        CancellationToken ct) =>
        Ok(await _studentService.GetPagedAsync(GetPagedQuery(), search, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _studentService.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        var created = await _studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentDto>> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken ct) =>
        Ok(await _studentService.UpdateAsync(id, request, ct));

    [HttpGet("{id:guid}/guardians")]
    public async Task<ActionResult<IReadOnlyList<GuardianDto>>> GetGuardians(Guid id, CancellationToken ct) =>
        Ok(await _studentService.GetGuardiansAsync(id, ct));

    [HttpPost("{id:guid}/guardians")]
    public async Task<ActionResult<GuardianDto>> CreateGuardian(Guid id, [FromBody] CreateGuardianRequest request, CancellationToken ct)
    {
        var created = await _studentService.CreateGuardianAsync(id, request, ct);
        return Created($"/api/v1/students/{id}/guardians", created);
    }

    [HttpGet("{id:guid}/enrollments")]
    public async Task<ActionResult> GetEnrollments(Guid id, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _studentService.GetEnrollmentsByStudentAsync(id, GetPagedQuery(), status, ct));

    [HttpPost("{id:guid}/tuition-payments")]
    [Authorize(Roles = "Admin,Accountant,Receptionist")]
    public async Task<ActionResult<TuitionPaymentDto>> RecordTuitionPayment(Guid id, [FromBody] CreateTuitionPaymentRequest request, CancellationToken ct)
    {
        var created = await _tuitionService.RecordPaymentAsync(id, request, ct);
        return CreatedAtAction(nameof(TuitionPaymentsController.GetById), "TuitionPayments", new { id = created.Id }, created);
    }
}
