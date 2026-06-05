using EnglishCenter.Application.Common;
using EnglishCenter.Application.Dtos;

namespace EnglishCenter.Application.Abstractions;

public interface ITuitionService
{
    Task<PagedResult<StudentTuitionMonthDto>> GetTuitionMonthsPagedAsync(
        PagedQuery query,
        StudentTuitionMonthListFilter filter,
        CancellationToken cancellationToken = default);

    Task<StudentTuitionMonthDto> GetTuitionMonthByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<TuitionPaymentDto>> GetPaymentsPagedAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<TuitionPaymentDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TuitionPaymentDto> RecordPaymentAsync(Guid studentId, CreateTuitionPaymentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassStudentTuitionBillingDto>> GetClassTuitionBillingAsync(
        Guid classId,
        int year,
        int month,
        CancellationToken cancellationToken = default);
}
