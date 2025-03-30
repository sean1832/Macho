using System.Drawing;
using Macho.CodeEditor;
using Macho.Params;
using Grasshopper.Kernel;
using ILGPU;
using ILGPU.Runtime;

namespace Macho.Components
{
    public abstract class KernelScriptBase(string name, string nickname, string description)
        : CodeEditorBase(name, nickname, description)
    {
        /// <summary>
        /// Accelerator to execute on
        /// </summary>
        protected Accelerator Accelerator { get; private set; }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcceleratorParam(), "Accelerator", "A", "Accelerator to execute on",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AcceleratorGoo acceleratorGoo = null;
            if (!DA.GetData(0, ref acceleratorGoo))
                return;

            Accelerator = acceleratorGoo.Value;

            ExecuteKernel(DA);
        }

        /// <summary>
        /// Derived classes must implement this to compile and execute their custom kernel.
        /// </summary>
        /// <param name="DA">Data access interface</param>
        protected abstract void ExecuteKernel(IGH_DataAccess DA);
    }
}
