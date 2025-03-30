using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GhGpu;
using GhGpu.Components;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System.Diagnostics;
using ILGPU.Runtime.Cuda;

public class BasicKernelComponent : KernelScriptBase
{
    public BasicKernelComponent()
        : base("Custom Kernel Component", "Custom", "My custom kernel component")
    {
    }
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        // Register the accelerator input from the base class.
        base.RegisterInputParams(pManager);
        // Add custom input parameters.
        pManager.AddIntegerParameter("Data Size", "N", "Number of elements to process", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Result", "R", "The output", GH_ParamAccess.list);
        pManager.AddTextParameter("Elapsed Time", "T", "Time taken in ms", GH_ParamAccess.item);
    }

    // Static kernel method.
    private static void MyKernel(Index1D i, ArrayView<double> input, ArrayView<double> output)
    {
        output[i] = Math.Sin(input[i % input.Length]) * Math.Sqrt(i);
    }

    protected override void ExecuteKernel(IGH_DataAccess DA)
    {
        // Get the data size (number of elements to process)
        int N = 1000000;
        if (!DA.GetData(1, ref N)) return;

        // Create the input array and initialize it
        double[] input = new double[N];
        for (int i = 0; i < N; i++)
        {
            input[i] = i * 0.001;
        }

        // Start a stopwatch to measure the overall GPU processing time (including memory transfers)
        Stopwatch sw = Stopwatch.StartNew();

        // Allocate and initialize GPU buffers.
        using var bufferInput = Accelerator.Allocate1D(input);
        using var bufferOutput = Accelerator.Allocate1D<double>(N);

        // Compile the kernel.
        var compiledKernel = Accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>>(MyKernel);

        // Execute the kernel.
        compiledKernel(N, bufferInput.View, bufferOutput.View);
        Accelerator.Synchronize();

        sw.Stop();

        // Retrieve the results from the GPU.
        double[] result = bufferOutput.GetAsArray1D();

        // Output the results and the elapsed time.
        DA.SetDataList(0, result.ToList());
        DA.SetData(1, $"{sw.ElapsedMilliseconds} ms");
    }
}