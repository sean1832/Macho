# Macho

**High-performance GPU-accelerated C# scripting for Rhino Grasshopper, powered by [ILGPU](https://www.ilgpu.net/).**  
<img src="assets/macho.png" alt="Macho logo" width="200"/>

## Overview

Macho is a C# scripting component for Rhino Grasshopper that enables users to write and execute GPU-accelerated parallel code directly inside Grasshopper, using [ILGPU](https://www.ilgpu.net/).  
It allows you to write custom GPU kernels, compile them in real-time, and leverage the massive parallel computing power of modern GPUs — all within the Grasshopper environment.

Macho is designed to make GPU programming accessible to designers, developers, and technical artists without the need to build separate `.gha` plugins or precompiled libraries.  
You can customize and experiment with GPU kernels directly in your Grasshopper definitions.

## How it works

Macho builds upon and extends the excellent work of [HotLoader](https://github.com/camnewnham/HotLoader) by [@camnewnham](https://github.com/camnewnham).

HotLoader manages the Visual Studio integration, source code editing, and real-time compilation workflow — allowing users to edit and compile C# code directly inside Grasshopper.  

Macho enhances this workflow by integrating ILGPU support, enabling GPU kernel scripting, compilation, and execution in real-time.

Additionally, Macho adopts HotLoader’s distributable architecture: source code and compiled binaries are bundled into a ZIP file, embedded inside the Grasshopper `.gh` definition. This makes it easy to share and distribute custom GPU scripts — anyone can open your Grasshopper file and the scripts will just work, without any additional setup other than downloading the Macho plugin.

## Key Features

- **GPU Kernel Scripting & Real-time Compilation**  
  Write and execute ILGPU-based parallel code directly in Grasshopper.

- **Customizable GPU Kernels & Memory Management**  
  Fine-tune your GPU logic with direct access to ILGPU APIs.

- **Intuitive API for Parallel Programming**  
  Designed for ease of use without sacrificing performance.

- **Visual Studio Integration**  
  Built-in editing and debugging workflow based on [HotLoader](https://github.com/camnewnham/HotLoader).

- **Distributable Scripts**  
  Source code and binaries are packed into your `.gh` files — no external dependencies required for others to run your scripts other than Macho.

## Prerequisites

- Rhino 7 or later
- Visual Studio 2019 or later

## License

Macho is licensed under the [Apache 2.0](LICENSE).

> **Acknowledgements**  
> This project would not be possible without the foundational work of [HotLoader](https://github.com/camnewnham/HotLoader) by [@camnewnham](https://github.com/camnewnham), which powers the real-time editing and compilation workflow in Macho.