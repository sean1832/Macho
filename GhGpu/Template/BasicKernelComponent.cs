using System;
using Grasshopper.Kernel;
using GhGpu.Components;
using ILGPU;
using ILGPU.Runtime;
using System.Collections.Generic;

namespace CustomKernelComponent;

public class BasicKernelComponent()
    : KernelScriptBase("GPU Kernel Component", "GPU Benchmark", "GPU kernel with low per-thread complexity")
{
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        base.RegisterInputParams(pManager);
        pManager.AddNumberParameter("Seed", "D", "Input seed value", GH_ParamAccess.item);
        pManager.AddIntegerParameter("Total Iterations", "I", "Total Iteration", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Result", "R", "Accumulated result", GH_ParamAccess.item);
    }

    private static Action<Index1D, double, ArrayView<double>> _cachedKernel;

    // Kernel method
    private static void Kernel(Index1D i, double seed, ArrayView<double> results)
    {
        // Define your kernel logic here
        double val = Math.Sin(seed) + Math.Cos(seed) + (i % 10);
        Atomic.Add(ref results[0], val);
    }

    protected override void ExecuteKernel(IGH_DataAccess DA)
    {
        double seed = 0.0;
        int totalThreads = 0;
        if (!DA.GetData(1, ref seed)) return;
        if (!DA.GetData(2, ref totalThreads)) return;

        // Allocate a buffer
        using var bufferResult = Accelerator.Allocate1D<double>(1);

        // Compile and cache the kernel
        if (_cachedKernel == null)
        {
            _cachedKernel = Accelerator.LoadAutoGroupedStreamKernel<Index1D, double, ArrayView<double>>(Kernel);
        }

        // Launch the kernel.
        _cachedKernel(totalThreads, seed, bufferResult.View);
        Accelerator.Synchronize();

        // Retrieve the result from the GPU.
        double[] result = bufferResult.GetAsArray1D();

        // Output the accumulated result and the elapsed time metrics.
        DA.SetData(0, result[0]);
    }
}
