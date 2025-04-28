using Silk.NET.OpenGL;
using System.Numerics;

namespace Fatou.Graphics
{
    internal class ShaderProgramme : IDisposable
    {

        private Dictionary<string, int> _uniformLocations = [];

        private readonly uint _vertexShader, _fragmentShader;
        private readonly uint _handle;
        private readonly GL _gl;

        public ShaderProgramme(string vertexShaderFile, string fragmentShaderFile, GL gl)
        {
            _gl = gl;

            uint _vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderFile);
            uint _fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderFile);

            _handle = _gl.CreateProgram();

            _gl.AttachShader(_handle, _vertexShader);
            _gl.AttachShader(_handle, _fragmentShader);
            _gl.LinkProgram(_handle);

            _gl.GetProgram(_handle, GLEnum.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: ${_gl.GetProgramInfoLog(_handle)}");
            }
        }

        private uint LoadShader(ShaderType shaderType, string path)
        {
            string src = File.ReadAllText(path);
            uint handle = _gl.CreateShader(shaderType);
            _gl.ShaderSource(handle, src);
            _gl.CompileShader(handle);
            string infoLog = _gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.Error.WriteLine(infoLog);
                throw new Exception($"Error compiling shader of type {shaderType}. Failed with error ${infoLog}");
            }
            return handle;
        }

        public void Start()
        {
            _gl.UseProgram(_handle);
        }

        public void Stop()
        {
            _gl.UseProgram(0);
        }

        private int UniformLocation(string uniformName)
        {
            if (!_uniformLocations.ContainsKey(uniformName))
            {
                _uniformLocations.Add(uniformName, _gl.GetUniformLocation(_handle, uniformName));
            }
            return _uniformLocations[uniformName];
        }

        public unsafe void LoadVector(string uniformName, Vector4 vector)
        {
            int location = UniformLocation(uniformName);
            _gl.Uniform4(location, vector);
        }

        public unsafe void LoadMatrix(string uniformName, Matrix4x4 matrix)
        {
            int location = UniformLocation(uniformName);
            _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }

        public void Dispose()
        {
            _gl.DetachShader(_handle, _vertexShader);
            _gl.DetachShader(_handle, _fragmentShader);

            _gl.DeleteShader(_vertexShader);
            _gl.DeleteShader(_fragmentShader);

            _gl.DeleteProgram(_handle);
        }

    }
}
