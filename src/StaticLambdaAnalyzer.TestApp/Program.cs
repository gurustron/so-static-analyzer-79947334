Console.WriteLine("Hello, World!");

int fortyTwo = 42;
assertStatic(true, staticFunction); //works (trivial)
assertStatic(false, nonStaticFunction); //works (trivial)
assertStatic(false, () => fortyTwo); //works (happy path)
assertStatic(true, () => 42); //fails
assertStatic(true, static () => 42); //fails too
return;

static int staticFunction() => 42;
int nonStaticFunction() => fortyTwo;

static void assertStatic<T>(bool shouldBeStatic, System.Func<T> action)
{
    Console.WriteLine($"Assert: {IsStatic(action) == shouldBeStatic}");
}

static bool IsStatic(System.Delegate @delegate)
{
    bool hasTarget = @delegate.Target != null;
    Console.WriteLine(@delegate.Method.IsStatic == !hasTarget);
    return !hasTarget;
}
