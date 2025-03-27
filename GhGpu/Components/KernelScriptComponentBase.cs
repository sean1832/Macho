using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using GH_IO.Serialization;
using Grasshopper.Kernel.Parameters;
using System.Reflection;
using GhGpu.CodeEditor;
using GhGpu.Params;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OptimizationLevel = Microsoft.CodeAnalysis.OptimizationLevel;

namespace GhGpu.Components
{
    public abstract class KernelScriptComponentBase(string name, string nickname, string description)
        : GH_Component(name, nickname, description, "ILGPU", "Kernel"), IGH_VariableParameterComponent
    {
        protected string KernelCode = "";
        protected Assembly CompiledAssembly;
        protected List<string> CompileErrors = new List<string>();
        protected List<Diagnostic> Diagnostics = new List<Diagnostic>();

        public override Guid ComponentGuid => GetType().GUID;
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcceleratorParam(), "Accelerator", "A", "Accelerator device to compute", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Output", "Out", "output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var result = KernelCompiler.Compile(KernelCode);
            CompileErrors = result.Errors;
            Diagnostics = result.Diagnostics;
            if (CompileErrors.Count > 0)
            {
                foreach (var error in CompileErrors)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                return;
            }

            CompiledAssembly = result.CompiledAssembly;

            // TODO: Add ILGPU execution logic here
            // This would involve marshaling data from GH inputs to GPU buffers,
            // executing the kernel, and retrieving results
        }

        //protected bool CompileCode()
        //{
        //    CompileErrors.Clear();
        //    try
        //    {
        //        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9); // C# 9
        //        var syntaxTree = CSharpSyntaxTree.ParseText(KernelCode, parseOptions);

        //        // Get all required framework assemblies
        //        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //        var references = new List<MetadataReference>();

        //        // Add essential framework references
        //        var essentialAssemblies = new[]
        //        {
        //            typeof(object).Assembly, // mscorlib
        //            typeof(System.Linq.Enumerable).Assembly, // System.Linq
        //            Assembly.Load("System.Runtime"), // System.Runtime
        //            typeof(System.Collections.Generic.List<>).Assembly, // System.Collections
        //            typeof(Memory<byte>).Assembly, // System.Memory
        //            typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly, // Microsoft.CSharp
        //            typeof(ILGPU.Runtime.Accelerator).Assembly, // ILGPU.Runtime.Accelerator
        //            typeof(ILGPU.Runtime.CPU.CPUAccelerator).Assembly, // ILGPU.Runtime.CPU.CPUAccelerator
        //            Assembly.GetExecutingAssembly()
        //        };

        //        foreach (var assembly in essentialAssemblies)
        //        {
        //            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        //        }


        //        var compilation = CSharpCompilation.Create(
        //            "DynamicKernel",
        //            [syntaxTree],
        //            references,
        //            new CSharpCompilationOptions(
        //                OutputKind.DynamicallyLinkedLibrary,
        //                optimizationLevel: OptimizationLevel.Release,
        //                allowUnsafe: true)); // ILGPU requires unsafe context

        //        using var ms = new System.IO.MemoryStream();
        //        EmitResult result = compilation.Emit(ms);

        //        if (!result.Success)
        //        {
        //            foreach (Diagnostic diagnostic in result.Diagnostics
        //                .Where(d => d.Severity == DiagnosticSeverity.Error))
        //            {
        //                CompileErrors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");
        //                Diagnostics.Add(diagnostic);
        //            }
        //            return false;
        //        }

        //        ms.Seek(0, System.IO.SeekOrigin.Begin);
        //        CompiledAssembly = Assembly.Load(ms.ToArray());
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        CompileErrors.Add($"Compilation failed: {ex.Message}");
        //        return false;
        //    }
        //}

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("KernelCode", KernelCode);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("KernelCode", ref KernelCode);
            return base.Read(reader);
        }

        // Code editor dialog implementation
        private CSharpEditorDialog _activeEditor;
        protected void OpenEditor()
        {
            if (_activeEditor != null) return;

            string originalCode = KernelCode;
            _activeEditor = new CSharpEditorDialog(KernelCode);
            _activeEditor.Closed += (s, e) =>
            {
                if (!_activeEditor.Canceled)
                {
                    KernelCode = _activeEditor.Code;
                    var result = KernelCompiler.Compile(KernelCode);
                    CompileErrors = result.Errors;
                    Diagnostics = result.Diagnostics;
                    if (CompileErrors.Count > 0)
                    {
                        foreach (var error in CompileErrors)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                    }
                    else
                    {
                        CompiledAssembly = result.CompiledAssembly;
                    }
                    ExpireSolution(true);
                }
                // Always clear the active editor, even if there are errors.
                _activeEditor = null;
            };
            _activeEditor.Show();
        }

        // Variable parameter handling
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                // Use "x", "y", "z" for the first three; then fallback to "In" + index
                string nick = index==0 ? "A" : $"x{index}";
                string name = index == 0 ? "Accelerator" : $"Input {index}";
                return new Param_GenericObject
                {
                    Name = name,
                    NickName = nick,
                    Description = "Kernel input"
                };
            }
            else
            {
                // Use "a", "b", "c" for the first three; then fallback to "Out" + index
                string nick = $"y{index}";
                return new Param_GenericObject
                {
                    Name = $"Output {index}",
                    NickName = nick,
                    Description = "Kernel output"
                };
            }
        }


        // Prevent removal of the first parameter (index 0)
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return index != 0;
        }

        // Allow removal only if index is not 0
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return index != 0;
        }

        // Allow insertion only at the bottom of the list
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
                return index == this.Params.Input.Count;
            else
                return index == this.Params.Output.Count;
        }

        // Maintain unique names and nicknames for all parameters
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            // Update input parameters
            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                var param = this.Params.Input[i];
                string nick = i == 0 ? "A" : $"x{i}";
                param.Name = i == 0 ? "Accelerator" : $"Input {i}";
                param.NickName = nick;
            }
            // Update output parameters
            for (int i = 0; i < this.Params.Output.Count; i++)
            {
                var param = this.Params.Output[i];
                string nick = $"y{i}";
                param.Name = $"Output {i}";
                param.NickName = nick;
            }
        }
    }

}
