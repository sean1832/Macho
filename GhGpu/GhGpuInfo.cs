using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace GhGpu
{
    public class GhGpuInfo : GH_AssemblyInfo
    {
        public override string Name => "GhGpu";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("cb0da0d6-ef5a-4e1d-8132-9ed5517497f3");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion
        {
            get
            {
                Version version = GetType().Assembly.GetName().Version;
                if (version != null)
                    return version.ToString();
                return null;
            }
        }
    }
}