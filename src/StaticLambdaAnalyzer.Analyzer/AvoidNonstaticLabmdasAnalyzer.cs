using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace StaticLambdaAnalyzer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidNonstaticLabmdasAnalyzer : DiagnosticAnalyzer
    {
        public const string AvoidNonstaticLabmdasDictionaryAnalyzerId = "STLA0001";
        private static readonly DiagnosticDescriptor Rule = new(
        AvoidNonstaticLabmdasDictionaryAnalyzerId,
        title: "Use the lambda parameters instead of using a closure",
        messageFormat: "Use the lambda parameters instead of using a closure (captured variable: {0})",
        "Performance",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "",
        helpLinkUri: "");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(ctx =>
            {
                var analyzerContext = new AnalyzerContext(ctx.Compilation);
                // if (analyzerContext.ConcurrentDictionarySymbol is null)
                //     return;

                ctx.RegisterOperationAction(innerCtx => analyzerContext.AnalyzeInvocation(innerCtx), OperationKind.Invocation);
            });

        }

        private sealed class AnalyzerContext
        {
            public AnalyzerContext(Compilation compilation)
            {

                var symbol = compilation.GetTypesByMetadataName("MyAnalyzedClass");

                AnalyzedType = symbol.FirstOrDefault();
                if (AnalyzedType is null)
                {
                    return;
                }

                // TODO
                AnalyzedMethod = AnalyzedType.GetMembers("assertStatic").OfType<IMethodSymbol>().SingleOrDefault();
            }

            public void AnalyzeInvocation(OperationAnalysisContext context)
            {
                var op = (IInvocationOperation)context.Operation;
                // if (!op.TargetMethod.ContainingSymbol.OriginalDefinition.IsEqualTo(AnalyzedType))
                // {
                //     return;
                // }
                
                if(!op.TargetMethod.OriginalDefinition.IsEqualTo(AnalyzedMethod))
                {
                    return;
                }

                IArgumentOperation argumentOperation = op.Arguments[1];
                bool v = argumentOperation.Value is IDelegateCreationOperation;
                Debug.WriteLine("Found");
            }


            public INamedTypeSymbol AnalyzedType { get; }
            public IMethodSymbol AnalyzedMethod { get; }
        }
    }
}


internal static class SymbolExtensions
{
    public static bool IsEqualTo(this ISymbol symbol, ISymbol expectedType)
    {
        if (symbol is null || expectedType is null)
            return false;

        return SymbolEqualityComparer.Default.Equals(expectedType, symbol);
    }
}