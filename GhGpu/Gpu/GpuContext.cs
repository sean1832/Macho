using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;

namespace GhGpu.Gpu
{
    public static class GpuContext
    {
        // The shared context instance.
        private static Context _context;
        // Cache for accelerators per device index.
        private static readonly Dictionary<int, Accelerator> Accelerators = new Dictionary<int, Accelerator>();
        // Lock for thread safety.
        private static readonly object Lock = new object();

        // Gets or creates the shared context.
        public static Context SharedContext
        {
            get
            {
                if (_context != null) return _context;
                lock (Lock)
                {
                    if (_context == null)
                    {
                        _context = Context.Create(builder => builder.AllAccelerators());
                    }
                }
                return _context;
            }
        }

        // Returns the accelerator for the specified device index, creating it if necessary.
        public static Accelerator GetAccelerator(int deviceIndex)
        {
            lock (Lock)
            {
                if (!Accelerators.ContainsKey(deviceIndex))
                {
                    // Validate the device index against the available devices.
                    if (deviceIndex < 0 || deviceIndex >= SharedContext.Devices.Length)
                    {
                        throw new IndexOutOfRangeException($"Device index must be between 0 and {SharedContext.Devices.Length - 1}");
                    }

                    // Create and cache the accelerator.
                    Accelerator accelerator = SharedContext.Devices[deviceIndex].CreateAccelerator(SharedContext);
                    Accelerators[deviceIndex] = accelerator;
                }
                return Accelerators[deviceIndex];
            }
        }

        // Returns an array of device names.
        public static string[] GetDeviceNames()
        {
            lock (Lock)
            {
                return SharedContext.Devices.Select(device => device.Name).ToArray();
            }
        }

        // Dispose of the shared resources. This should be called at a central point (e.g., on document shutdown).
        public static void Dispose()
        {
            lock (Lock)
            {
                foreach (var accelerator in Accelerators.Values)
                {
                    accelerator.Dispose();
                }
                Accelerators.Clear();
                _context?.Dispose();
                _context = null;
            }
        }
    }
}
