using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Macho.Gpu;
using Macho.Params;
using ILGPU;
using ILGPU.Runtime;

namespace Macho.Components
{
    public class AcceleratorManagerComponent : GH_Component
    {
        public AcceleratorManagerComponent()
            : base("Accelerator Manager", "Accelerator",
                "Creates and manages a shared GPU context; " +
                "accelerator is initialized once and reused until removed.",
                Config.Category, Config.SubCat.Operation)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => [];
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
            // If multiple components rely on GpuManager, ensure that Dispose is called only once at the proper time.
            GpuContext.Dispose();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int deviceIndex = 0;
            if (!DA.GetData(0, ref deviceIndex)) return;
            try
            {
                // Retrieve all device names from the shared context.
                string[] devicesName = GpuContext.GetDeviceNames();
                if (deviceIndex < 0 || deviceIndex >= devicesName.Length)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"Device index must be between 0 and {devicesName.Length - 1}");
                    DA.AbortComponentSolution();
                    return;
                }

                // Retrieve the accelerator for the selected device.
                Accelerator accelerator = GpuContext.GetAccelerator(deviceIndex);
                Message = $"{deviceIndex}: {devicesName[deviceIndex]}";

                DA.SetDataList(0, devicesName);
                DA.SetData(1, new AcceleratorGoo(accelerator));
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error: {e.Message}");
                DA.AbortComponentSolution();
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            // Unsubscribe from document events when the component is removed.
            Instances.DocumentServer.DocumentRemoved -= OnDocumentClose;
            base.RemovedFromDocument(document);
        }
    }
}