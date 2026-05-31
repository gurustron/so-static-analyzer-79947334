public static class MyAnalyzedClass
{
    public static void assertStatic<T>(bool shouldBeStatic, System.Func<T> action)
    {
        Console.WriteLine($"Assert: {IsStatic(action) == shouldBeStatic}");
    }
    public static bool IsStatic(System.Delegate @delegate)
    {
        bool hasTarget = @delegate.Target != null;
        Console.WriteLine(@delegate.Method.IsStatic == !hasTarget);
        return !hasTarget;
    }
}
