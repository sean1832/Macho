using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhGpu.Params
{
    public class AcceleratorIndex
    {
        private readonly int _index;
        private readonly string _name;

        public AcceleratorIndex() { }
        public AcceleratorIndex(int index, string name)
        {
            _index = index;
            _name = name;
        }

        public (int, string) Get()
        {
            return (_index, _name);
        }
        public int Index => _index;
        public string Name => _name;
    }
}
