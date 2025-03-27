﻿using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime;

namespace GhGpu.Params
{
    public class AcceleratorGoo : GH_Goo<AcceleratorIndex>, IDisposable
    {
        public AcceleratorGoo()
        {
            Value = null;
        }

        public AcceleratorGoo(AcceleratorIndex acceleratorIndex)
        {
            Value = acceleratorIndex;
        }

        public override IGH_Goo Duplicate()
        {
            return new AcceleratorGoo(Value);
        }

        public override string ToString()
        {
            return $"Accelerator [{Value.Index}:{Value.Name}]";
        }

        public override bool IsValid
        {
            get
            {
                if (Value == null) return false;
                if (Value.Index < 0) return false;
                if (string.IsNullOrEmpty(Value.Name)) return false;
                return true;
            }
        }

        public override string TypeName => "AcceleratorIndex";

        public override string TypeDescription => "AcceleratorIndex";

        public void Dispose()
        {
            Value = null;
        }
    }
}
