namespace EnglishCenter.Application.Abstractions;

/// <summary>Sinh buổi học từ lịch tuần (rolling 30 ngày tới).</summary>
public interface ILessonSessionGeneratorService
{
    /// <param name="classId">Null = tất cả lớp; có giá trị = chỉ lớp đó (sau khi tạo/sửa lịch).</param>
    /// <returns>Số buổi học mới được tạo.</returns>
    Task<int> GenerateAsync(Guid? classId = null, CancellationToken cancellationToken = default);
}
