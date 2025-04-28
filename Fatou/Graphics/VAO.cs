using Silk.NET.OpenGL;

namespace Fatou.Graphics
{
    internal class VAO : IDisposable
    {

        private readonly List<Action> _bindBuffers = [];

        private readonly uint _handle;

        private readonly GL _gl;

        public VAO(GL gl)
        {
            _gl = gl;

            _handle = _gl.GenVertexArray();
        }

        public void BindBuffer<T>(VBO<T> vbo)
            where T : unmanaged
        {
            _bindBuffers.Add(vbo.Bind);
        }

        public unsafe void VertexAttribPointer(uint index, int count, VertexAttribPointerType type, int vertexSize, uint offset = 0)
        {
            Bind();
            _gl.VertexAttribPointer(index, count, type, false, (uint)vertexSize, (void*)null);
            Unbind();
        }

        public void Bind()
        {
            _gl.BindVertexArray(_handle);
            foreach (var bind in _bindBuffers)
            {
                bind();
            }
        }

        public void Unbind()
        {
            _gl.BindVertexArray(0);
        }
        
        public void Dispose()
        {
            _gl.DeleteVertexArray(_handle);
        }
    }
}
