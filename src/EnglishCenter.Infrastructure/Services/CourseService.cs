using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;
using EnglishCenter.Domain.Entities;
using EnglishCenter.Domain.Exceptions;
using EnglishCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Services;

public sealed class CourseService : ICourseService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public CourseService(AppDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<CourseDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _db.Courses.AsQueryable().ToPagedAsync(query, cancellationToken);
        return PagedResult<CourseDto>.Create(
            paged.Items.Select(e => e.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<CourseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy khóa học.");
        return entity.ToDto();
    }

    public async Task<CourseDto> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        if (await _db.Courses.AnyAsync(c => c.Code == request.Code, cancellationToken))
            throw new ConflictException($"Mã khóa '{request.Code}' đã tồn tại.");

        var entity = new Course
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
        };
        entity.SetCreated(_currentUser.UserId, _clock.UtcNow);
        _db.Courses.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<CourseDto> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy khóa học.");

        if (entity.Code != request.Code &&
            await _db.Courses.AnyAsync(c => c.Code == request.Code && c.Id != id, cancellationToken))
            throw new ConflictException($"Mã khóa '{request.Code}' đã tồn tại.");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<CourseDto> PatchAsync(Guid id, PatchCourseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy khóa học.");

        if (request.Code is not null)
        {
            if (await _db.Courses.AnyAsync(c => c.Code == request.Code && c.Id != id, cancellationToken))
                throw new ConflictException($"Mã khóa '{request.Code}' đã tồn tại.");
            entity.Code = request.Code;
        }
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        entity.SetUpdated(_currentUser.UserId, _clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
