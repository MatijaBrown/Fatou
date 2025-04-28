using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Fatou
{
    internal class Program : IDisposable
    {

        private readonly IWindow _window;

        private GL? _gl;
        private IInputContext? _inputContext;

        // ImGUI
        private nint _context;
        private ImGuiController _imGuiController;

        private Program(IWindow window)
        {
            _window = window;
        }

        private void Load()
        {
            _gl = GL.GetApi(_window);
            _inputContext = _window.CreateInput();

            // ImGUI initialisation
            _imGuiController = new ImGuiController(_gl, _window, _inputContext);
        }

        private void Update(double delta)
        {
            float d = (float)delta;

            // Update ImGUI
            _imGuiController.Update(d);
        }

        private void Render(double _)
        {
            ImGui.ShowDemoWindow();

            // OpenGL New Frame
            _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render ImGUI
            _imGuiController.Render();
        }

        private void Close()
        {
            _imGuiController.Dispose();
        }

        public void Dispose()
        {

        }

        static void Main()
        {
            var options = WindowOptions.Default;
            options.VSync = true;
            options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
            options.Title = "Fatou - Fractal Explorer";

            var window = Window.Create(options);
            var program = new Program(window);

            window.Load += program.Load;
            window.Update += program.Update;
            window.Render += program.Render;
            window.Closing += program.Close;

            window.Run();

            program.Dispose();
            window.Dispose();
        }
    }
}
