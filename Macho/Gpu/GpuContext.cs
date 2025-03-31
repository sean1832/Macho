using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using ILGPU;

namespace Macho.Gpu
{
    /// <summary>
    /// Provides a shared context and accelerator management.
    /// </summary>
    public static class GpuContext
    {
        // The shared context instance.
        private static Context _context;
        // Cache for accelerators per device index.
        private static readonly Dictionary<int, Accelerator> Accelerators = new Dictionary<int, Accelerator>();
        // Lock for thread safety.
        private static readonly object Lock = new object();

        // Flag to ensure that the shared context is disposed only once.
        private static bool _isDisposed;

        /// <summary>
        /// Gets or creates the shared context.
        /// </summary>
        public static Context SharedContext
        {
            get
            {
                if (_context != null) return _context;
                lock (Lock)
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(nameof(GpuContext), "The GPU context has been disposed.");
                    if (_context == null)
                    {
                        _context = Context.Create(b => b.Default().EnableAlgorithms());
                    }
                }
                return _context;
            }
        }

        /// <summary>
        /// Returns the accelerator for the specified device index, creating it if necessary.
        /// </summary>
        /// <param name="deviceIndex">Index of device</param>
        /// <returns>Accelerator at device index</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static Accelerator GetAccelerator(int deviceIndex)
        {
            lock (Lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(GpuContext), "The GPU context has been disposed.");
                if (!Accelerators.ContainsKey(deviceIndex) || Accelerators[deviceIndex].IsDisposed)
                {
                    // Validate device index before proceeding
                    if (deviceIndex < 0 || deviceIndex >= SharedContext.Devices.Length)
                        throw new IndexOutOfRangeException($"Device index must be between 0 and {SharedContext.Devices.Length - 1}");

                    Accelerator accelerator = SharedContext.Devices[deviceIndex].CreateAccelerator(SharedContext);
                    Accelerators[deviceIndex] = accelerator;
                }
                return Accelerators[deviceIndex];
            }
        }

        /// <summary>
        /// Get all available device names.
        /// </summary>
        /// <returns>An array of device names.</returns>
        public static string[] GetDeviceNames()
        {
            lock (Lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(GpuContext), "The GPU context has been disposed.");
                return SharedContext.Devices.Select(device => device.Name).ToArray();
            }
        }

        /// <summary>
        /// Dispose of the shared resources. This should be called at a central point (e.g., on document shutdown).
        /// </summary>
        public static void Dispose()
        {
            lock (Lock)
            {
                if (_isDisposed) return;
                foreach (var accelerator in Accelerators.Values)
                {
                    accelerator.Dispose();
                }
                Accelerators.Clear();
                _context?.Dispose();
                _context = null;
                _isDisposed = true;
            }
        }
    }
}
