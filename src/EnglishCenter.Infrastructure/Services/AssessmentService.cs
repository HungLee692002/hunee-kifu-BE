using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class AssessmentService : IAssessmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public AssessmentService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<AssessmentDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, cancellationToken))
            throw new NotFoundException("Không tìm thấy lớp học.");

        var items = await _db.Assessments
            .Where(a => a.ClassId == classId)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync(cancellationToken);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssessmentDto> CreateAsync(
        Guid classId,
        CreateAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var cls = await _db.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");

        if (!cls.GradingEnabled)
            throw new BusinessException("Lớp học chưa bật chấm điểm.");

        var entity = new Assessment
        {
            ClassId = classId,
            Title = request.Title,
            AssessmentDate = request.AssessmentDate,
            MaxScore = request.MaxScore,
            Description = request.Description,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.Assessments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<IReadOnlyList<GradeDto>> GetGradesAsync(Guid assessmentId, CancellationToken cancellationToken = default)
    {
        await EnsureAssessmentAsync(assessmentId, cancellationToken);
        var items = await _db.Grades.Where(g => g.AssessmentId == assessmentId).ToListAsync(cancellationToken);
        return items.Select(g => g.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<GradeDto>> ReplaceGradesAsync(
        Guid assessmentId,
        ReplaceGradesRequest request,
        CancellationToken cancellationToken = default)
    {
        var assessment = await EnsureAssessmentAsync(assessmentId, cancellationToken);
        var cls = await _db.Classes.FirstOrDefaultAsync(c => c.Id == assessment.ClassId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy lớp học.");

        if (!cls.GradingEnabled)
            throw new BusinessException("Lớp học chưa bật chấm điểm.");

        var existing = await _db.Grades.Where(g => g.AssessmentId == assessmentId).ToListAsync(cancellationToken);
        foreach (var grade in existing)
            grade.SoftDelete(_currentUser.UserId, _clock.UtcNow);

        foreach (var item in request.Items)
        {
            if (!await _db.Students.AnyAsync(s => s.Id == item.StudentId, cancellationToken))
                throw new NotFoundException($"Không tìm thấy học sinh {item.StudentId}.");

            var grade = new Grade
            {
                AssessmentId = assessmentId,
                StudentId = item.StudentId,
                Score = item.Score,
                Comment = item.Comment,
                GradedAt = _clock.UtcNow,
                GradedBy = _currentUser.UserId,
            };
            grade.SetCreated(_currentUser.UserId, _clock.UtcNow);
            _db.Grades.Add(grade);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await GetGradesAsync(assessmentId, cancellationToken);
    }

    private async Task<Assessment> EnsureAssessmentAsync(Guid assessmentId, CancellationToken ct) =>
        await _db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId, ct)
        ?? throw new NotFoundException("Không tìm thấy bài kiểm tra.");
}
