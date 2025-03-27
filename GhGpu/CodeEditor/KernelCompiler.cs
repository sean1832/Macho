using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GhGpu.CodeEditor
{
    internal class KernelCompiler
    {
        public static (bool Success, List<string> Errors, List<Diagnostic> Diagnostics, Assembly CompiledAssembly) Compile(string code)
        {
            var errors = new List<string>();
            var diagnostics = new List<Diagnostic>();
            Assembly compiledAssembly = null;

            try
            {
                // Parse the code using C# 9.0
                var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
                var syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);

                // Collect any syntax errors first.
                var syntaxErrors = syntaxTree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
                if (syntaxErrors.Any())
                {
                    diagnostics.AddRange(syntaxErrors);
                    errors.AddRange(syntaxErrors.Select(d => $"{d.Id}: {d.GetMessage()}"));
                }

                // Setup required references
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib / System.Private.CoreLib
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location), // System.Collections
                    MetadataReference.CreateFromFile(typeof(Memory<byte>).Assembly.Location), // System.Memory
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location), // Microsoft.CSharp
                    MetadataReference.CreateFromFile(typeof(ILGPU.Runtime.Accelerator).Assembly.Location), // ILGPU.Runtime
                    MetadataReference.CreateFromFile(typeof(ILGPU.Runtime.CPU.CPUAccelerator).Assembly.Location) // ILGPU.Runtime.CPU
                };

                // Use options that treat warnings as errors.
                var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true,
                    generalDiagnosticOption: ReportDiagnostic.Error);

                var compilation = CSharpCompilation.Create(
                    "DynamicKernel",
                    new[] { syntaxTree },
                    references,
                    compilationOptions);

                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);
                    if (!result.Success)
                    {
                        // Add any errors from the emit phase.
                        var emitErrors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                        diagnostics.AddRange(emitErrors);
                        errors.AddRange(emitErrors.Select(d => $"{d.Id}: {d.GetMessage()}"));
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        compiledAssembly = Assembly.Load(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Compilation failed: {ex.Message}");
            }

            bool success = errors.Count == 0;
            return (success, errors, diagnostics, compiledAssembly);
        }
    }
}

