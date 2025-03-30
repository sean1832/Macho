/*
 * This file is adapted from HotLoader by camnewnham.
 * Source: https://github.com/camnewnham/HotLoader/blob/main/Plugin/HotComponentAttributes.cs
 * Licensed under the MIT License.
 * Original copyright (c) 2023 camnewnham.
 * Modifications made for GhGpu.CodeEditor under the Apache-2.0 License.
 */

using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;

namespace GhGpu.CodeEditor
{
    internal class CodeEditorAttributes : GH_ComponentAttributes
    {
        public CodeEditorAttributes(IGH_Component component) : base(component)
        {
            if (component is not CodeEditorBase)
            {
                throw new InvalidOperationException($"Can not create {nameof(CodeEditorAttributes)} for {component?.GetType().FullName}");
            }
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!ContentBox.Contains(e.CanvasLocation)) return base.RespondToMouseDoubleClick(sender, e);
            if (Owner is not CodeEditorBase component) return base.RespondToMouseDoubleClick(sender, e);

            component.EditSourceProject();
            return GH_ObjectResponse.Handled;
        }
    }
}
