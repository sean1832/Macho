using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime;

namespace Macho.Params
{
    public class AcceleratorGoo : GH_Goo<Accelerator>
    {
        public AcceleratorGoo()
        {
            Value = null;
        }

        public AcceleratorGoo(Accelerator accelerator)
        {
            Value = accelerator;
        }

        public override IGH_Goo Duplicate()
        {
            return new AcceleratorGoo(Value);
        }

        public override string ToString()
        {
            return $"Accelerator [Name:{Value.Device.Name}, Size:{Value.Device.MemorySize}]";
        }

        public override bool IsValid
        {
            get
            {
                if (Value == null) return false;
                if (string.IsNullOrEmpty(Value.Name)) return false;
                return true;
            }
        }

        public override string TypeName => "Accelerator";

        public override string TypeDescription => "Accelerator";
    }
}
