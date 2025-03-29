using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using GhGpu.Gpu;
using GhGpu.Params;

namespace GhGpu.Components
{
    public class GetAcceleratorComponent : GH_Component
    {
        public GetAcceleratorComponent()
            : base("Get Accelerator", "Accelerator",
                "Defines accelerator context",
                Config.Category, Config.SubCat.Operation)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("bf1cf9ff-82e2-47ac-8f0b-f88a1d92e7cd");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Device Index", "I", "Device Index to select", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Devices Info", "D", "All Accelerator devices available on this computer",
                GH_ParamAccess.list);
            pManager.AddParameter(new AcceleratorParam(), "Accelerator", "A", "Current Accelerator device", GH_ParamAccess.item);
        }

        #endregion

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int deviceIndex = 0;
            if (!DA.GetData(0, ref deviceIndex)) return;
            if (deviceIndex < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Device index must be greater than 0");
                DA.AbortComponentSolution();
                return;
            }

            try
            {
                using var deviceContext = new GpuContext(deviceIndex);
                var devices = deviceContext.GetDeviceNames();
                string device = null;
                string[] devicesName = new string[devices.Length];

                for (int i = 0; i < devices.Length; i++)
                {
                    devicesName[i] = devices[i];
                    if (i == deviceIndex)
                    {
                        device = devices[i];
                    }
                }

                Message = devicesName[deviceIndex];

                DA.SetDataList(0, devicesName);
                DA.SetData(1, new AcceleratorGoo(new AcceleratorIndex(deviceIndex, device)));
            }
            catch (IndexOutOfRangeException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error: Invalid device index. Please ensure you select a valid index based on the available devices.");
                DA.AbortComponentSolution();
            }
        }
    }
}