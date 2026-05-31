namespace StaticLambdaAnalyzer.Tests;

using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Analyzer.AvoidNonstaticLabmdasAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public class AvoidNonstaticLabmdasAnalyzerTests
{

    [Test]
    public async Task Test1()
    {
        var test = """
        using System;   
        public class C
        {
            public void  M()
            {
                Console.WriteLine("Hello, World!");

                int fortyTwo = 42;
                MyAnalyzedClass.assertStatic(true, staticFunction); //works (trivial)
                MyAnalyzedClass.assertStatic(false, nonStaticFunction); //works (trivial)
                MyAnalyzedClass.assertStatic(false, () => fortyTwo); //works (happy path)
                MyAnalyzedClass.assertStatic(true, () => 42); //fails
                MyAnalyzedClass.assertStatic(true, static () => 42); //fails too
                return;

                static int staticFunction() => 42;
                int nonStaticFunction() => fortyTwo;
            }
        }

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
        """;
        await Verify.VerifyAnalyzerAsync(test);
    }
}
