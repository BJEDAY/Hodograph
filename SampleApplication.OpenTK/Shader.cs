using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace SampleApplication.OpenTK
{
    public class Shader
    {
        int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            //uchwyty na shadery
            int VertexShader = 0, FragmentShader = 0;
            //zapisanie shadera do stringa
            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);
            //stworzenie shaderów i przypisanie do uchwytów + uzupełnienie zawartością
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            //kompilacja shaderów i test czy działa
            GL.CompileShader(VertexShader);
            int success = 0;
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            //stworzenie programu
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            //he individual vertex and fragment shaders are useless now that they've been linked; 
            //the compiled data is copied to the shader program when you link it. 
            //You also don't need to have those individual shaders attached to the program; 
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }


        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);

            GL.UniformMatrix4(location, true, ref matrix);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //Garbage Collector
        }
    }
}
