using System;
using Grasshopper.Kernel;
using Macho.Components;
using ILGPU;
using ILGPU.Runtime;
using System.Collections.Generic;

namespace CustomKernelComponent;

public class BasicKernelComponent() 
    : KernelScriptBase("GPU Kernel Component", "GPU Benchmark", "Your custom GPU Kernel component")
{
    // Instance-level cached kernel and associated accelerator
    private Action<Index1D, double, ArrayView<double>> _cachedKernel; // <- remember to update the signature
    private Accelerator _cachedKernelAccelerator;

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        base.RegisterInputParams(pManager);
        pManager.AddNumberParameter("Seed", "S", "Input seed value", GH_ParamAccess.item);
        pManager.AddIntegerParameter("Total Iterations", "I", "Total Iteration", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Result", "R", "Accumulated result", GH_ParamAccess.item);
    }

    // Kernel method
    private static void Kernel(Index1D i, double seed, ArrayView<double> results)
    {
        double val = Math.Sin(seed) + Math.Cos(seed) + (i % 10);
        Atomic.Add(ref results[0], val);
    }

    protected override void ExecuteKernel(IGH_DataAccess DA)
    {
        double seed = 0.0;
        int totalThreads = 0;
        if (!DA.GetData(1, ref seed)) return;
        if (!DA.GetData(2, ref totalThreads)) return;

        // Allocate buffer for the result
        using var bufferResult = Accelerator.Allocate1D<double>(1);

        // If the kernel hasn't been compiled or if the accelerator has changed, compile a new kernel
        if (_cachedKernel == null || _cachedKernelAccelerator != Accelerator)
        {
            _cachedKernel = Accelerator.
                LoadAutoGroupedStreamKernel<Index1D, double, ArrayView<double>>(Kernel); // <- remember to update the signature
            _cachedKernelAccelerator = Accelerator;
        }

        // Launch the kernel.
        _cachedKernel(totalThreads, seed, bufferResult.View);
        Accelerator.Synchronize();

        // Retrieve the result from the GPU.
        double[] result = bufferResult.GetAsArray1D();
        DA.SetData(0, result[0]);
    }
}
