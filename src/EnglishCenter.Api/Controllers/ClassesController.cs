using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[Authorize(Roles = "Admin,AcademicStaff,Teacher")]
public class ClassesController : ApiControllerBase
{
    private readonly IClassService _service;
    private readonly IScheduleService _schedule;
    private readonly IClassAssignmentService _assignments;
    private readonly IAssessmentService _assessments;
    private readonly ITuitionService _tuition;
    private readonly ILessonSessionGeneratorService _sessionGenerator;

    public ClassesController(
        IClassService service,
        IScheduleService schedule,
        IClassAssignmentService assignments,
        IAssessmentService assessments,
        ITuitionService tuition,
        ILessonSessionGeneratorService sessionGenerator)
    {
        _service = service;
        _schedule = schedule;
        _assignments = assignments;
        _assessments = assessments;
        _tuition = tuition;
        _sessionGenerator = sessionGenerator;
    }

    [HttpGet]
    public async Task<ActionResult> GetList(CancellationToken ct) =>
        Ok(await _service.GetPagedAsync(GetPagedQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<ClassDto>> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<ClassDto>> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<ClassDto>> Patch(Guid id, [FromBody] PatchClassRequest request, CancellationToken ct) =>
        Ok(await _service.PatchAsync(id, request, ct));

    [HttpGet("{classId:guid}/enrollments")]
    public async Task<ActionResult> GetEnrollments(Guid classId, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _service.GetEnrollmentsByClassAsync(classId, GetPagedQuery(), status, ct));

    [HttpPost("{classId:guid}/enrollments")]
    [Authorize(Roles = "Admin,AcademicStaff,Receptionist")]
    public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(Guid classId, [FromBody] CreateEnrollmentRequest request, CancellationToken ct)
    {
        var created = await _service.CreateEnrollmentAsync(classId, request, ct);
        return Created($"/api/v1/enrollments/{created.Id}", created);
    }

    [HttpGet("{classId:guid}/schedule-templates")]
    public async Task<ActionResult> GetScheduleTemplates(Guid classId, CancellationToken ct) =>
        Ok(await _schedule.GetTemplatesByClassAsync(classId, ct));

    [HttpPost("{classId:guid}/schedule-templates")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<WeeklyScheduleTemplateDto>> CreateScheduleTemplate(
        Guid classId,
        [FromBody] CreateWeeklyScheduleTemplateRequest request,
        CancellationToken ct)
    {
        var created = await _schedule.CreateTemplateAsync(classId, request, ct);
        return Created($"/api/v1/schedule-templates/{created.Id}", created);
    }

    [HttpGet("{classId:guid}/teacher-assignments")]
    public async Task<ActionResult> GetTeacherAssignments(Guid classId, CancellationToken ct) =>
        Ok(await _assignments.GetByClassAsync(classId, ct));

    [HttpPost("{classId:guid}/teacher-assignments")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<ClassAssignmentDto>> CreateTeacherAssignment(
        Guid classId,
        [FromBody] CreateClassAssignmentRequest request,
        CancellationToken ct)
    {
        var created = await _assignments.CreateAsync(classId, request, ct);
        return Created($"/api/v1/teacher-assignments/{created.Id}", created);
    }

    /// <summary>Sinh buổi học 30 ngày tới từ lịch tuần của lớp (chạy ngay sau khi tạo lớp / lịch).</summary>
    [HttpPost("{classId:guid}/generate-lesson-sessions")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult> GenerateLessonSessions(Guid classId, CancellationToken ct)
    {
        var count = await _sessionGenerator.GenerateAsync(classId, ct);
        return Ok(new { sessionsCreated = count });
    }

    [HttpGet("{classId:guid}/tuition-billing")]
    [Authorize(Roles = "Admin,Accountant,Receptionist")]
    public async Task<ActionResult> GetTuitionBilling(
        Guid classId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct) =>
        Ok(await _tuition.GetClassTuitionBillingAsync(classId, year, month, ct));

    [HttpGet("{classId:guid}/assessments")]
    public async Task<ActionResult> GetAssessments(Guid classId, CancellationToken ct) =>
        Ok(await _assessments.GetByClassAsync(classId, ct));

    [HttpPost("{classId:guid}/assessments")]
    [Authorize(Roles = "Admin,AcademicStaff")]
    public async Task<ActionResult<AssessmentDto>> CreateAssessment(
        Guid classId,
        [FromBody] CreateAssessmentRequest request,
        CancellationToken ct)
    {
        var created = await _assessments.CreateAsync(classId, request, ct);
        return Created($"/api/v1/assessments/{created.Id}/grades", created);
    }
}

