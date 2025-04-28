using Fatou.Graphics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Fatou
{
    internal class Programme : IDisposable
    {

        private readonly IWindow _window;

        private GL? _gl;
        private IInputContext? _inputContext;

        private float _aspectRatio;

        private VAO vao;
        private VBO<float> vbo;
        private ShaderProgramme shader;

        private Vector4 bounds = new(-2.0f, 2.0f, 2.0f, -2.0f); // RTLB
        private Vector2 movement;

        Matrix4x4 viewMatrix;

        // ImGUI
        private nint _context;
        private ImGuiController _imGuiController;

        private Programme(IWindow window)
        {
            _window = window;
        }

        private void Load()
        {
            _gl = GL.GetApi(_window);
            _inputContext = _window.CreateInput();

            foreach (IKeyboard keyboard in _inputContext.Keyboards)
            {
                keyboard.KeyDown += (_, k, _) =>
                {
                    switch (k)
                    {
                        case Key.Up:
                        case Key.W:
                            movement.Y = -1.0f;
                            break;
                        case Key.Down:
                        case Key.S:
                            movement.Y = 1.0f;
                            break;
                        case Key.Left:
                        case Key.A:
                            movement.X = -1.0f;
                            break;
                        case Key.Right:
                        case Key.D:
                            movement.X = 1.0f;
                            break;

                        case Key.KeypadAdd:
                            Scale(2.0f);
                            break;
                        case Key.Minus:
                        case Key.KeypadSubtract:
                            Scale(1 / 2f);
                            break;
                    }
                };
                keyboard.KeyUp += (_, k, _) =>
                {
                    switch (k)
                    {
                        case Key.Up:
                        case Key.W:
                        case Key.Down:
                        case Key.S:
                            movement.Y = 0.0f;
                            break;
                        case Key.Left:
                        case Key.A:
                        case Key.Right:
                        case Key.D:
                            movement.X = 0.0f;
                            break;

                        case Key.Escape:
                            _window.Close();
                            break;
                    };
                };
            }

            Resize(_window.Size); // Initialise size stuff

            vbo = new VBO<float>(BufferTargetARB.ArrayBuffer, _gl);
            vbo.Store([
                1.0f,  1.0f, 1.0f,
                1.0f, -1.0f, 1.0f,
               -1.0f, -1.0f, 1.0f,

                1.0f,  1.0f, 1.0f,
               -1.0f, -1.0f, 1.0f,
               -1.0f,  1.0f, 1.0f
            ]);
            vbo.Bind();
            vao = new VAO(_gl);
            vao.BindBuffer(vbo);
            vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0);

            shader = new ShaderProgramme("./vertexShader.vert", "./fragmentShader.frag", _gl);

            // ImGUI initialisation
            _imGuiController = new ImGuiController(_gl, _window, _inputContext);
        }

        private void Scale(float scalingFactor)
        {
            Vector2 size = new(bounds.Z - bounds.X, bounds.W - bounds.Y);
            Vector2 center = new Vector2(bounds.X, bounds.Y) + size / 2;

            size /= (2.0f * scalingFactor); // mult. with 2 b.c. size is subtracted as half (as it's a size)

            bounds = new Vector4(center.X - size.X, center.Y - size.Y, center.X + size.X, center.Y + size.Y);
        }

        private void Resize(Vector2D<int> newSize)
        {
            _gl!.Viewport(newSize);
            _aspectRatio = (float)newSize.Y / (float)newSize.X;
        }

        private void Update(double delta)
        {
            float d = (float)delta;

            Vector2 size = Vector2.Abs(new Vector2(bounds.Z - bounds.X, bounds.W - bounds.Y)); // scale movement speed by scaling. Otherwise we get terribly fast when zoomed in.

            bounds += new Vector4(movement.X, movement.Y, movement.X, movement.Y) * d * MathF.Min(size.X, size.Y) * 0.5f; // change speed here

            viewMatrix = Matrix4x4.CreateOrthographic(1.0f, 1.0f * _aspectRatio, 0.1f, 1000.0f);

            // Update ImGUI
            _imGuiController.Update(d);
        }

        private void Render(double _)
        {
            ImGui.ShowDemoWindow();

            // OpenGL New Frame
            _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _gl!.ClearColor(0f, 0f, 0f, 1f);

            vao.Bind();

            shader.Start();
            shader.LoadMatrix("viewMatrix", viewMatrix);
            shader.LoadVector("renderBounds", bounds);

            _gl!.EnableVertexAttribArray(0);

            _gl!.DrawArrays(PrimitiveType.Triangles, 0, 6);

            _gl!.DisableVertexAttribArray(0);

            vao.Unbind();
            shader.Stop();

            // Render ImGUI
            _imGuiController.Render();
        }

        private void Close()
        {
            vao.Dispose();
            vbo.Dispose();

            shader.Dispose();

            _imGuiController.Dispose();
        }

        public void Dispose()
        {

        }

        static void Main()
        {
            var options = WindowOptions.Default;
            options.VSync = true;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "Fatou - Fractal Explorer";

            var window = Window.Create(options);
            var programme = new Programme(window);

            window.Load += programme.Load;
            window.FramebufferResize += programme.Resize;
            window.Update += programme.Update;
            window.Render += programme.Render;
            window.Closing += programme.Close;

            window.Run();

            programme.Dispose();
            window.Dispose();
        }
    }
}
