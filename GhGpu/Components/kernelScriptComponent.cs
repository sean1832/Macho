using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;

namespace GhGpu.Components
{
    public class KernelScriptComponent : KernelScriptComponentBase
    {
        public KernelScriptComponent() : base("ILGPU Kernel", "Kernel", "Custom ILGPU compute kernel")
        {
            KernelCode = @"
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Runtime;

public static class KernelInstance
{
    static void Kernel(Index1D i, ArrayView<int> data, ArrayView<int> output)
    {
        output[i] = data[i % data.Length];
    }
}";
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override void CreateAttributes()
        {
            Attributes = new KernelComponentAttributes(this, OpenEditor);
        }
    }

    class KernelComponentAttributes(KernelScriptComponent owner, Action openEditor) : GH_ComponentAttributes(owner)
    {
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            openEditor();
            return GH_ObjectResponse.Handled;
        }
    }
}
