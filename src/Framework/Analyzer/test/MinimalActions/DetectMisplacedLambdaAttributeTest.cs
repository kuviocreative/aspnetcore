// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Analyzer.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Analyzers.MinimalActions;

public partial class DetectMisplacedLambdaAttributeTest
{
    private TestDiagnosticAnalyzerRunner Runner { get; } = new(new MinimalActionAnalyzer());

    [Fact]
    public async Task MinimalAction_WithCorrectlyPlacedAttribute_Works()
    {
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/"", [Authorize] () => Hello());
void Hello() { }
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task MinimalAction_WithMisplacedAttribute_ProducesDiagnostics()
    {
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/"", () => Hello());
/*MM*/[Authorize]
void Hello() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        Assert.Equal("Authorize should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task MinimalAction_WithMultipleMisplacedAttributes_ProducesDiagnostics()
    {
        // System.Console.WriteLine($"Waiting for debugger to attach on {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        // System.Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/"", () => Hello());
/*MM*/[Authorize]
[Produces(""application/xml"")]
void Hello() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        Assert.Collection(diagnostics,
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("Authorize should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            },
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("Produces should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            }
        );
    }

    [Fact]
    public async Task MinimalAction_WithMisplacedAttributesOnMultipleMethods_ProducesDiagnostics()
    {
        // System.Console.WriteLine($"Waiting for debugger to attach on {System.Environment.ProcessId}");
        // while (!System.Diagnostics.Debugger.IsAttached)
        // {
        //     Thread.Sleep(1000);
        // }
        // System.Console.WriteLine("Debugger attached");
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/"", () => {
    if (true)
    {
        Hello();
    }
    Bye();
});
/*MM*/[Authorize]
void Hello() { }
[Produces(""application/xml"")]
void Bye() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        Assert.Collection(diagnostics,
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("Authorize should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            },
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("Produces should be placed on the delegate instead of Bye", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            }
        );
    }
}

