using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SampleApplication.OpenTK;


namespace SampleApplication.OpenTK
{
    public class Circle
    {
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }

        int IndicesCount;
        public float radius { get; set; }
        Vector3[] points;

        public Circle(float R)
        {
            radius = R;
            points = new Vector3[2];
            IndicesCount = 300;
            GenerateVAO();
        }

        public (float[] vertices, int[] indices) MakeMesh()
        {
            List<float> verts = new List<float>();
            List<int> indices = new List<int>();
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < 360; i++)
            {
                float alf = ((float)(i) / 360) * MathF.PI * 2;
                var current_pos = new Vector3(MathF.Sin(alf), MathF.Cos(alf), 1f) * radius;
                positions.Add(current_pos);
                verts.Add(current_pos.X); verts.Add(current_pos.Y); verts.Add(0f); // rysowanie odbywa sie na plaszczyznie xz 
                if (i == 359)
                {
                    indices.Add(i);
                    indices.Add(0);
                }
                else
                {
                    indices.Add(i);
                    indices.Add(i + 1);
                }
            }
            IndicesCount = indices.Count;
            return (verts.ToArray(), indices.ToArray());
        }

        public float GetRadians(float angle)
        {
            return angle * (float)Math.PI / (float)180;
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

        public void UpdateValue(float R)
        {
            radius = R;
            UpdateVAO();
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
            GL.DrawElements(PrimitiveType.Lines, IndicesCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}

