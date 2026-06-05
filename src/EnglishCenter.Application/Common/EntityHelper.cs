namespace EnglishCenter.Application.Common;

public static class EntityHelper
{
    public static string NewGuidCode() => Guid.NewGuid().ToString();
}
