// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.Analyzers.MinimalActions;

public partial class MinimalActionAnalyzer : DiagnosticAnalyzer
{
    private static void DetectMisplacedLambdaAttribute(
        in OperationAnalysisContext context,
        IInvocationOperation invocation,
        IAnonymousFunctionOperation method)
    {
        var syntaxTree = context.Compilation.SyntaxTrees.FirstOrDefault();

        if (syntaxTree is null)
        {
            return;
        }

        var invocations = invocation.Syntax.DescendantNodes().OfType<InvocationExpressionSyntax>();

        if (!invocations.Any())
        {
            return;
        }

        foreach (var invoke in invocations)
        {
            var identifier = ((IdentifierNameSyntax)invoke.Expression).Identifier;
            var identifierName = identifier.ValueText;

            var members = syntaxTree.GetRoot().DescendantNodes().OfType<LocalFunctionStatementSyntax>();
            var item = members.Where(member => member.Identifier.ValueText == identifierName).SingleOrDefault();
            var location = item.GetLocation();
            if (item is LocalFunctionStatementSyntax)
            {
                    foreach (var attributeList in item.AttributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.DetectMisplacedLambdaAttribute,
                            location,
                            attribute.Name,
                            identifierName));
                        }

                    }
            }
        }

    }
}