using StaticLambdaAnalyzer.TestApp;

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
