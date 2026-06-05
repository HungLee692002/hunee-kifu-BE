namespace EnglishCenter.Application.Common;

public static class ScheduleConflictHelper
{
    public static bool TimeRangesOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2) =>
        start1 < end2 && start2 < end1;

    public static bool EffectiveRangesOverlap(
        DateOnly? from1,
        DateOnly? to1,
        DateOnly? from2,
        DateOnly? to2)
    {
        var start1 = from1 ?? DateOnly.MinValue;
        var end1 = to1 ?? DateOnly.MaxValue;
        var start2 = from2 ?? DateOnly.MinValue;
        var end2 = to2 ?? DateOnly.MaxValue;
        return start1 <= end2 && start2 <= end1;
    }

    public static bool TemplatesConflict(
        int dayOfWeek1,
        Guid roomId1,
        TimeOnly start1,
        TimeOnly end1,
        DateOnly? effectiveFrom1,
        DateOnly? effectiveTo1,
        int dayOfWeek2,
        Guid roomId2,
        TimeOnly start2,
        TimeOnly end2,
        DateOnly? effectiveFrom2,
        DateOnly? effectiveTo2) =>
        dayOfWeek1 == dayOfWeek2
        && roomId1 == roomId2
        && TimeRangesOverlap(start1, end1, start2, end2)
        && EffectiveRangesOverlap(effectiveFrom1, effectiveTo1, effectiveFrom2, effectiveTo2);
}
