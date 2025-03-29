using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GhGpu;
using GhGpu.Components;
using ILGPU;

public class MyHotComponent : KernelScriptBase
{
    public MyHotComponent()
        : base("Custom Kernel Component", "Custom", "My custom kernel component")
    {
    }
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        base.RegisterInputParams(pManager); // call the base class to add the accelerator parameter

        // your custom input parameters
        pManager.AddNumberParameter("First number", "X", "The first number to add", GH_ParamAccess.item);
        pManager.AddNumberParameter("Second number", "Y", "The second number to add", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Result", "R", "The output of the addition", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        double a = 0;
        double b = 0;
        if (!DA.GetData(0, ref a))
        {
            return;
        }
        if (!DA.GetData(1, ref b))
        {
            return;
        }
        DA.SetData(0, new GH_Number(a + b));
    }

    private void Kernel(Index1D i, ArrayView<int> data, ArrayView<int> output)
    {
        output[i] = data[i % data.Length];
    }

    public override void Dispose()
    {
        // clean up resources
    }
}