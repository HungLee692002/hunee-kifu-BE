using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class ClassAssignmentService : IClassAssignmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ClassAssignmentService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ClassAssignmentDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, cancellationToken))
            throw new NotFoundException("Không tìm thấy lớp học.");

        var items = await _db.ClassAssignments
            .Where(a => a.ClassId == classId)
            .OrderByDescending(a => a.AssignedFrom)
            .ToListAsync(cancellationToken);

        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<ClassAssignmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ClassAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phân công giáo viên.");
        return entity.ToDto();
    }

    public async Task<ClassAssignmentDto> CreateAsync(
        Guid classId,
        CreateClassAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.Id == classId, cancellationToken))
            throw new NotFoundException("Không tìm thấy lớp học.");
        if (!await _db.Teachers.AnyAsync(t => t.Id == request.TeacherId, cancellationToken))
            throw new NotFoundException("Không tìm thấy giáo viên.");

        var entity = new ClassAssignment
        {
            ClassId = classId,
            TeacherId = request.TeacherId,
            Role = EnumNames.ParseClassAssignmentRole(request.Role),
            AssignedFrom = request.AssignedFrom,
            AssignedTo = request.AssignedTo,
            IsActive = true,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.ClassAssignments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassAssignmentDto> UpdateAsync(
        Guid id,
        UpdateClassAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.ClassAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phân công giáo viên.");

        if (!await _db.Teachers.AnyAsync(t => t.Id == request.TeacherId, cancellationToken))
            throw new NotFoundException("Không tìm thấy giáo viên.");

        entity.TeacherId = request.TeacherId;
        entity.Role = EnumNames.ParseClassAssignmentRole(request.Role);
        entity.AssignedFrom = request.AssignedFrom;
        entity.AssignedTo = request.AssignedTo;
        entity.IsActive = request.IsActive;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassAssignmentDto> PatchAsync(
        Guid id,
        PatchClassAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.ClassAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phân công giáo viên.");

        if (request.TeacherId.HasValue)
        {
            if (!await _db.Teachers.AnyAsync(t => t.Id == request.TeacherId, cancellationToken))
                throw new NotFoundException("Không tìm thấy giáo viên.");
            entity.TeacherId = request.TeacherId.Value;
        }
        if (request.Role is not null) entity.Role = EnumNames.ParseClassAssignmentRole(request.Role);
        if (request.AssignedFrom.HasValue) entity.AssignedFrom = request.AssignedFrom.Value;
        if (request.AssignedTo.HasValue) entity.AssignedTo = request.AssignedTo;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
