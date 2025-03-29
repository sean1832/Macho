using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhGpu.CodeEditor;
using GhGpu.Params;
using Grasshopper.Kernel;

namespace GhGpu.Components
{
    public abstract class KernelScriptBase(string name, string nickname, string description)
        : CodeEditorBase(name, nickname, description), IDisposable
    {
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcceleratorParam(), "Accelerator", "Ac", "Accelerator to execute on",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            
        }
        public abstract void Dispose();
    }
}
