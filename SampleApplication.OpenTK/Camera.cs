using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SampleApplication.OpenTK
{
    public class Camera
    {
        float PositionZ = 5;

        public Matrix4 viewMatrix { get; set; }
        public Matrix4 projectionMatrix { get; set; }
        //Vector3 Direction;
        public Vector3 Target { get; set; }
        Vector3 Right;
        Vector3 Up;
        Vector3 Pos;
        Vector3 NewPos;
        float rotX = 0;
        float rotY = 0;
        public Camera()
        {
            //Camera pos
            var Position = new Vector3(0.0f, 0.0f, PositionZ);
            NewPos = Position;
            Pos = Position;
            //Camera direction
            Target = Vector3.Zero;
            var Direction = Vector3.Normalize(Position - Target);
            //Right axis
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
            //Up axis
            Up = Vector3.Cross(Direction, Right);
            UpdateViewMatrix();
            projectionMatrix = Matrix4.Identity;
        }

        public void UpdateViewMatrix()
        {
            //Get actual position including rotation
            //var actual_pos = Matrix4.CreateRotationX(rotX) * Matrix4.CreateRotationY(rotY)* new Vector4(Position, 1);
            //var actual_pos = new Vector4(new Vector3(0.0f, 0.0f, PositionZ), 1) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotX)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotY));
            var actual_pos = new Vector4(NewPos, 1) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotX)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotY));

            //NEW STUFF
            Pos = new Vector3(actual_pos.X, actual_pos.Y, actual_pos.Z);

            viewMatrix = ViewMatrix(Pos);
            //return Matrix4.LookAt(new Vector3(actual_pos.X, actual_pos.Y, actual_pos.Z), Vector3.Zero, Up);
        }

        public void UpdateProjectionMatrix(float width, float height, float fov, float n, float f)
        {
            //width, height of OpenTK window, fov - viewing angle in degrees, n - distance from closer clipping wall, f - distance to the far face of clipping
            float alfa = MathHelper.DegreesToRadians(fov) / 2.0f;
            if (Math.Sin(alfa) == 0 || f == n) return;
            float ctg = (float)(1 / Math.Tan(alfa));
            float aspect = width / height;
            float val1 = (float)(ctg / aspect);
            float val2 = ctg;
            float val3 = (float)((f + n) / (f - n));
            float val4 = (float)((-2 * f * n) / (f - n));
            Vector4 Row1 = new Vector4(val1, 0, 0, 0);
            Vector4 Row2 = new Vector4(0, val2, 0, 0);
            Vector4 Row3 = new Vector4(0, 0, val3, val4);
            Vector4 Row4 = new Vector4(0, 0, 1, 0);
            projectionMatrix = new Matrix4(Row1, Row2, Row3, Row4);
        }

        public void ChangeDistance(float z)
        {
            //PositionZ += z;
            NewPos.Z += z;
            UpdateViewMatrix();
        }

        public void MoveCamera(char Key)
        {
            float speed = 0.05f;
            switch (Key)
            {
                case 'W':
                    NewPos += Up * speed;           //Up
                    break;
                case 'S':
                    NewPos -= Up * speed;           //Down
                    break;
                case 'A':
                    NewPos -= Right * speed; //Left
                    break;
                case 'D':
                    NewPos += Right * speed;
                    break;
            }
            UpdateViewMatrix();
        }

        public void UpdateRotation(float x, float y)
        {

            if (rotX > 89.0f)
            {
                rotX = 89.0f;
            }
            else if (rotX < -89.0f)
            {
                rotX = -89.0f;
            }
            else
            {
                rotX -= x;
            }


            rotY -= y;
            UpdateViewMatrix();
        }


        //NEW STUFF
        public Matrix4 GetStereoView(int eye, float sep)
        {
            Vector3 pos = Pos + (Right * (sep * 0.5f * eye));
            //return Matrix4.LookAt(pos, Target, Up);
            return ViewMatrix(pos);
        }

        public Matrix4 GetStereoPerspective(int eye, float sep, float foc, float near, float far, float fov, float width, float height)
        {
            fov = MathHelper.DegreesToRadians(fov);
            float frustumshift = eye * (sep * 0.5f) * (near / foc);   //offset 
            float top = near * (float)Math.Tan(fov * 0.5f);
            float aspect = width / height;
            float right = aspect * top;

            //Option 1
            //var matrix = Matrix4d.Frustum(-right - frustumshift, right - frustumshift, -top, top, near, far);
            //var result = new Matrix4();
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 4; j++)
            //    {
            //        result[i, j] = (float)matrix[i, j];
            //    }
            //}
            //return result;

            //Option 2
            return StereoPerspectiveMatrix(-right - frustumshift, right - frustumshift, -top, top, near, far);
        }

        public Matrix4 ViewMatrix(Vector3 P)
        {
            //Update direction including 
            //Target = Vector3.Zero;
            var Direction = Vector3.Normalize(new Vector3(P.X, P.Y, P.Z) - Target);
            //Update local axis
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
            Up = Vector3.Cross(Direction, Right);

            //Pozycja
            // Vector4 pos = new Vector4(-actual_pos.X,-actual_pos.Y,-actual_pos.Z, 1);
            //Matrix4 MatrixR = Matrix4.CreateTranslation(-new Vector3(0.0f, 0.0f, PositionZ));
            Matrix4 MatrixR = Matrix4.CreateTranslation(-NewPos);

            //Rotacja
            Vector4 R = new Vector4(Right, 0);
            Vector4 U = new Vector4(Up, 0);
            Vector4 D = new Vector4(Direction, 0);
            Vector4 LastRow = new Vector4(0, 0, 0, 1);
            Matrix4 MatrixL = new Matrix4(R, U, D, LastRow);

            return MatrixL * MatrixR;
        }

        public Matrix4 StereoPerspectiveMatrix(float l, float r, float b, float t, float n, float f)
        // n - near, f - far, l - left side, r - right side, t - top, b - bottom
        {
            //float r1c1 = (float)((2 * n) / (r - l));
            //float r1c3 = (float)((r + l) / (r - l));
            //float r2c2 = (float)(2*n/(t-b));
            //float r2c3 = (float)((t+b)/(t-b));
            //float r3c3 = (float)((f+n)/(f-n));
            //float r3c4 = (float)((-2*f*n)/(f-n));
            //Vector4 Col1 = new Vector4(r1c1, 0, 0, 0);
            //Vector4 Col2 = new Vector4(0, r2c2, 0, 0);
            //Vector4 Col3 = new Vector4(r1c3, r2c3, r3c3, 1);
            //Vector4 Col4 = new Vector4(0, 0, r3c4, 0);
            //return new Matrix4(Col1, Col2, Col3, Col4);

            return new Matrix4(new Vector4(2.0f * n * 1.0f / (r - l), 0.0f, 0.0f, 0.0f), new Vector4(0.0f, 2.0f * n * 1.0f / (t - b), 0.0f, 0.0f), new Vector4((r + l) * 1.0f / (r - l), (t + b) * 1.0f / (t - b), (0.0f - (f + n)) * 1.0f / (f - n), -1.0f), new Vector4(0.0f, 0.0f, -2.0f * f * n * 1.0f / (f - n), 0.0f));
        }
    }
}
