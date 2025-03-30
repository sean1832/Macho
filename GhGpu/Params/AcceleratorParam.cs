using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime;

namespace GhGpu.Params
{
    public class AcceleratorParam() : GH_PersistentParam<AcceleratorGoo>(
        "Accelerator Index", "AcceleratorIdx",
        "Index of accelerator device",
        Config.Category, Config.SubCat.Param)
    {
        public override Guid ComponentGuid => new Guid("036b8a31-a021-479c-815a-cae9871afa38");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override GH_GetterResult Prompt_Plural(ref List<AcceleratorGoo> values)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Singular(ref AcceleratorGoo value)
        {
            return GH_GetterResult.cancel;
        }
    }
}