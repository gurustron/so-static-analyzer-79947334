using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace StaticLambdaAnalyzer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidNonstaticLabmdasAnalyzer : DiagnosticAnalyzer
    {
        public const string AvoidNonstaticLabmdasAnalyzerId = "STLA0001";
        private static readonly DiagnosticDescriptor Rule = new(
        AvoidNonstaticLabmdasAnalyzerId,
        title: "Avoid Nonstatic Labmdas",
        messageFormat: "Use static methods/lamdas instead of {0}",
        "Performance",
        DiagnosticSeverity.Warning,
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

                ctx.RegisterOperationAction(analyzerContext.AnalyzeInvocation, OperationKind.Invocation);
            });
        }

        private sealed class AnalyzerContext
        {
            public INamedTypeSymbol AnalyzedType { get; }
            public IMethodSymbol AnalyzedMethod { get; }

            public AnalyzerContext(Compilation compilation)
            {
                var symbol = compilation.GetTypesByMetadataName("MyAnalyzedClass");

                AnalyzedType = symbol.FirstOrDefault();
                if (AnalyzedType is null)
                {
                    return;
                }

                // TODO: change to correct name and add validations
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

                // TODO: change + validations
                IArgumentOperation argumentOperation = op.Arguments[1];

                if (argumentOperation.Value is IDelegateCreationOperation delegateCreationOperation)
                {
                    var delegateTarget = delegateCreationOperation.Target;
                    if (delegateTarget is IMethodReferenceOperation methodReferenceOperation)
                    {
                        if (methodReferenceOperation.Method.IsStatic)
                        {
                            return;
                        }
                        
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                descriptor: Rule, 
                                location: methodReferenceOperation.Syntax.GetLocation(), 
                                messageArgs: methodReferenceOperation.Method.Name));
                    }
                    else if (delegateCreationOperation.Target is IAnonymousFunctionOperation anonymousFunctionOperation)
                    {
                        // check that lambda marked as static
                        if (anonymousFunctionOperation.Symbol.IsStatic)
                        {
                            return;
                        }

                        var syntax = GetDataFlowArgument(anonymousFunctionOperation.Body.Syntax);
                        var semanticModel = context.Operation.SemanticModel!;
                        DataFlowAnalysis dataFlow = semanticModel.AnalyzeDataFlow(syntax);
                        if (dataFlow.CapturedInside.Length > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                descriptor: Rule,
                                location: anonymousFunctionOperation.Syntax.GetLocation(),
                                messageArgs: string.Join(", ", dataFlow.Captured.Select(symbol => symbol.Name))));
                        }
                    }
                }

                Debug.WriteLine("Found");
            }
            
            static IEnumerable<ISymbol> GetParameters(IOperation operation)
            {
                if (operation is IAnonymousFunctionOperation func)
                {
                    return func.Symbol.Parameters;
                }

                if (operation is IDelegateCreationOperation delegateCreation)
                {
                    return GetParameters(delegateCreation.Target);
                }

                return [];
            }
        
            private static SyntaxNode GetDataFlowArgument(SyntaxNode node)
            {
                if (node is null)
                    return null;

                if (node is ArrowExpressionClauseSyntax expression)
                {
                    return expression.Expression;
                }

                return node;
            }
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