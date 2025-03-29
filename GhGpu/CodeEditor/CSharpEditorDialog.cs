using Eto.Forms;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Ed.Eto;
using Eto.Drawing;
using Microsoft.CodeAnalysis.FlowAnalysis;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using Grasshopper.GUI.Script;

namespace GhGpu.CodeEditor
{
    class CSharpEditorDialog : Form
    {
        CodeEditorControl _editor;
        ListBox _errorList;
        private readonly string _originalCode;

        public string Code { get; private set; }
        public bool Canceled { get; private set; }

        class SimpleCommand : Command
        {
            public SimpleCommand(string text, Action action)
            {
                MenuText = text;
                Executed += (s, e) => action();
            }
        }

        public CSharpEditorDialog(string initialCode)
        {
            _originalCode = initialCode;
            InitializeComponents(800, 600);
        }

        void InitializeComponents(int sizeX, int sizeY)
        {
            Title = "Kernel Script";
            Size = new Eto.Drawing.Size(sizeX, sizeY);
            Resizable = true;
            Topmost = true; // Ensure window stays on top
            

            _editor = new CodeEditorControl();
            // Subscribe to the TextLoaded event
            _editor.TextLoaded += (s, e) => CheckCode();
            _ = _editor.SetTextAsync(_originalCode);  // Fire and forget


            _errorList = new ListBox { Height = 100 };

            var saveButton = new Button { Text = "OK" };
            saveButton.Click += async (s, e) => await SafeClose();

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += async (s, e) =>
            {
                Canceled = true;
                await SafeClose();
            };

            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 5,
                Items =
        {
            new StackLayoutItem(_editor, HorizontalAlignment.Stretch, true),
            new StackLayoutItem(_errorList, HorizontalAlignment.Stretch, false),
            new StackLayoutItem(new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items = { null, saveButton, cancelButton },
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            })
        }
            };

            Menu = new MenuBar
            {
                Items =
        {
            new ButtonMenuItem
            {
                Text = "&Edit",
                Items =
                {
                    new SimpleCommand("&Undo", () => _editor.GoBack()),
                    new SimpleCommand("&Redo", () => _editor.GoForward()),
                    new SeparatorMenuItem(),
                    new SimpleCommand("&Format Code", FormatCode),
                    new SimpleCommand("&Check Code", CheckCode)  // Manual check command
                }
            },
            new ButtonMenuItem
            {
                Text = "&Templates",
                Items =
                {
                    new SimpleCommand("ILGPU Kernel Template", InsertKernelTemplate),
                    new SimpleCommand("Add Using Statements", InsertCommonUsings)
                }
            }
        }
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CheckCode();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            CheckCode();
        }
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            CheckCode();
        }

        private async Task SafeClose()
        {
            try
            {
                await Application.Instance.InvokeAsync(Close);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Close error:", ex.Message);
            }
        }

        public void ShowErrors(IEnumerable<Diagnostic> diagnostics)
        {
            Application.Instance.Invoke(() =>
            {
                _errorList.Items.Clear();
                foreach (var diagnostic in diagnostics.OrderBy(d => d.Location.GetLineSpan().StartLinePosition.Line))
                {
                    _errorList.Items.Add($"{diagnostic.Severity}: {diagnostic.GetMessage()} (line:{diagnostic.Location.GetLineSpan().EndLinePosition.Line+1})");
                }
            });
        }

        public async void FormatCode()
        {
            try
            {
                var code = await _editor.GetTextAsync();
                var formattedCode = CodeFormatter.Format(code);
                await _editor.SetTextAsync(formattedCode);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Formatting Error", ex.Message);
            }
        }

        public async void CheckCode()
        {
            try
            {
                var code = await _editor.GetTextAsync();
                var (success, errors, diagnostics, _) = KernelCompiler.Compile(code);

                if (!success)
                {
                    ShowErrors(diagnostics);
                }
                else
                {
                    if (_errorList != null)
                        _errorList.Items.Clear();
                    //await ShowMessageAsync("Compilation", "No syntax or compilation errors detected.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Check Error", ex.Message);
            }
        }

        async void InsertKernelTemplate()
        {
            const string template = @"
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Runtime;

public static class KernelInstance
{
    static void Kernel(Index1D i, ArrayView<int> data, ArrayView<int> output)
    {
        output[i] = data[i % data.Length];
    }
}";
            try
            {
                await _editor.InsertTextAtCursorPositionAsync(template);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Insert Error", ex.Message);
            }
        }

        async void InsertCommonUsings()
        {
            const string usings = "using System;\nusing System.Linq;\nusing ILGPU;\nusing ILGPU.Runtime;\n";
            try
            {
                await _editor.InsertTextAtCursorPositionAsync(usings);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Insert Error", ex.Message);
            }
        }

        protected override async void OnClosed(EventArgs e)
        {
            try
            {
                Code = Canceled ? _originalCode : await _editor.GetTextAsync();
            }
            catch (Exception ex)
            {
                Code = _originalCode;
                await ShowMessageAsync("Save Error", ex.Message);
            }
            base.OnClosed(e);
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            await Application.Instance.InvokeAsync(() =>
                MessageBox.Show(this, message, title)
            );
        }
    }

    class CodeEditorControl() : Ed.Eto.Ed("csharp")
    {
        public event EventHandler TextLoaded;

        public new async Task SetTextAsync(string text)
        {
            await base.SetTextAsync(text);
            TextLoaded?.Invoke(this, EventArgs.Empty);
        }

        public new async Task<string> GetTextAsync() =>
            await base.GetTextAsync() ?? string.Empty;

        public new async Task InsertTextAtCursorPositionAsync(string text)
        {
            var currentText = await GetTextAsync();
            await base.InsertTextAsync(text, 0, currentText.Length);
        }
    }

    static class CodeFormatter
    {
        public static string Format(string code)
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot().NormalizeWhitespace();
                return root.ToFullString();
            }
            catch
            {
                return code; // Return original if formatting fails
            }
        }
    }
}
