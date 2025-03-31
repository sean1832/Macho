/*
 * This file is adapted from HotLoader by camnewnham.
 * Source: https://github.com/camnewnham/HotLoader/blob/main/Plugin/HotComponentPlaceholder.cs
 * Licensed under the MIT License.
 * Original copyright (c) 2023 camnewnham.
 * Modifications made for Macho.CodeEditor under the Apache-2.0 License.
 */

using Grasshopper.Kernel;
using System.IO;
using System.Windows.Forms;
using Macho.Params;

namespace Macho.CodeEditor
{
    public class CodeEditorPlaceholder()
        : CodeEditorBase("Custom GPU Kernel", "Kernel", "A custom component written in C#")
    {
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "This is a placeholder. Double click to edit the source code with your native C# editor.");
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcceleratorParam(), "Accelerator", "A", "Accelerator to execute on",
                GH_ParamAccess.item);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendItem(menu, "Link to Existing Project", (ev, arg) =>
            {
                Rhino.UI.OpenFileDialog openFileDialog = new Rhino.UI.OpenFileDialog
                {
                    Filter = "C# Project Files (*.csproj)|*.csproj",
                    Title = "Select a C# project file to link to this component"
                };
                if (openFileDialog.ShowOpenDialog())
                {
                    ReplaceComponentWithPlaceholder(Path.GetDirectoryName(openFileDialog.FileName));
                }
            });
        }
    }
}
