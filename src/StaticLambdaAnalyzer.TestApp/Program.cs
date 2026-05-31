Console.WriteLine("Hello, World!");

int fortyTwo = 42;
MyAnalyzedClass.assertStatic(true, staticFunction); // no diagnostic
MyAnalyzedClass.assertStatic(false, nonStaticFunction); // STLA0001
MyAnalyzedClass.assertStatic(false, () => fortyTwo); // STLA0001
MyAnalyzedClass.assertStatic(true, () => 42); // no diagnostic
MyAnalyzedClass.assertStatic(true, static () => 42); // no diagnostic
return;

static int staticFunction() => 42;
int nonStaticFunction() => fortyTwo;
