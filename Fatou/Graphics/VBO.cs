using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

namespace Fatou.Graphics
{
    internal class VBO<T> : IDisposable
        where T : unmanaged
    {

        private readonly uint _handle;
        private readonly BufferTargetARB _bufferType;

        private readonly GL _gl;

        public VBO(BufferTargetARB bufferType, GL gl)
        {
            _bufferType = bufferType;
            _gl = gl;

            _handle = _gl.GenBuffer();
        }

        public unsafe void Store(Span<T> data, BufferUsageARB usage = BufferUsageARB.StaticDraw)
        {
            Bind();
            fixed (void* ptr = data)
            {
                _gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(T)), ptr, usage);
            }
            Unbind();
        }

        public void Bind()
        {
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void Unbind()
        {
            _gl.BindBuffer(_bufferType, 0);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_handle);
        }

    }
}
