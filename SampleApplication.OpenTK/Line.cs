using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SampleApplication.OpenTK;

namespace SampleApplication.OpenTK
{
    public class Line
    {
        Vector2 start;
        Vector2 end;

        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }

        public Line(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            GenerateVAO();
        }

        public (float[] vertices, int[] indices) MakeMesh()
        {
            List<float> verts = new List<float>();
            List<int> indices = new List<int>();

            verts.Add(start.X); verts.Add(start.Y); verts.Add(0f);
            verts.Add(end.X); verts.Add(end.Y); verts.Add(0f);
            indices.Add(0); indices.Add(1);

            return (verts.ToArray(), indices.ToArray());
        }

        public void GenerateVAO()
        {
            var (vertices, indices) = MakeMesh();
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void UpdateValue(float len)
        {
            this.start = new Vector2(len, 0);
            this.end = new Vector2(0, 0);
            UpdateVAO();
        }

        public void UpdateEndPos(float alf, float len, float circle_R)
        {
            //var test = Math.Sin(GetRadians(90));
            this.end = new Vector2((float)Math.Sin(GetRadians(alf)), (float)Math.Cos(GetRadians(alf))) * circle_R;
            this.start.X = end.X + (float)Math.Sqrt((double)(len * len - end.Y * end.Y));
            this.start.Y = 0f;

            UpdateVAO();
        }

        public float GetRadians(float angle)
        {
            return angle * (float)Math.PI / (float)180;
        }

        public void UpdateVAO()
        {
            var (vertices, indices) = MakeMesh();
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }
        public void Draw(Shader shader, Matrix4 view, Matrix4 perspective)
        {
            shader.Use();
            shader.SetMatrix4("persp", perspective);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("model", Matrix4.CreateTranslation(-5f, 0f, 0f));
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, 0);
        }
    }
}

