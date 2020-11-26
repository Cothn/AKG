using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinForms3DModelViewer.Helper;

namespace WinForms3DModelViewer
{
    public partial class MainForm : Form
    {
        private List<Vector4> vertices;
        private List<Vector4> originalVertices;
        private List<int[]> originalPoligons;
        private List<int[]> poligons;

        Point lastPoint = Point.Empty;//Point.Empty represents null for a Point object
        bool isMouseDown = false;

        Vector3 viewPoint = new Vector3(0, 0, 5);
        Vector3 eye = new Vector3(0, 0, 2);
        float delta = 0.1f;
        float aDelta = 1f;

        float xRotation = 0;
        float yRotation = 0;
        float zRotation = 0;

        Matrix4x4 viewPortMatrix;

        public MainForm()
        {
            InitializeComponent();
            pictureBoxPaintArea.MouseWheel += _MouseWheel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ObjParser parser = new ObjParser();
            (originalVertices, originalPoligons) = parser.Parse(@"D:\RepositHub\АКГ\Head\Model.obj");
            Transform();
        }

        public void Transform()
        {
            vertices = new List<Vector4>(originalVertices);
            poligons = new List<int[]>(originalPoligons);

            var eye = this.viewPoint;
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);

            var k = (float)Math.PI / 180;
            var rm = Matrix4x4.CreateFromYawPitchRoll((yRotation % 360) * k, (xRotation % 360) * k, (zRotation % 360) * k);

            eye = Vector3.Transform(eye, rm);
            up = Vector3.Transform(up, rm);


            //Place and transform model from local to world coordinates
            var viewerMatrix = ToViewerCoordinates(eye, target, up);
            //var viewerMatrix = Matrix4x4.CreateLookAt(eye, target, up);
            //TransformVectors(viewerMatrix);

            //var translation = Matrix4x4.CreateTranslation(-viewPoint);
            //TransformVectors(translation);

            //Create Projection matrix
            var projectionMatrix = ToProjectionCoordinates();

            var viewPortMatrix = ToViewPortCoordinates();

            var mainMatrix = viewerMatrix * projectionMatrix;
            //mainMatrix = projectionMatrix;
            //mainMatrix = translation * viewerMatrix * projectionMatrix;
            //mainMatrix = GetMVP();
            //TransformVectors(mainMatrix);

            for (int i = 0; i < vertices.Count; i++)
            {
                //vertices[i] = Vector4.Transform(vertices[i], transformMatrix);
                vertices[i] = Vector4.Transform(vertices[i], mainMatrix);
                vertices[i] /= vertices[i].W;
            }



            for (int j = 0; j < poligons.Count; j++)
            {
                var poligon = poligons[j];

                //TODO Remove polygon cut with proj matrix

                if (poligon.Any(i => vertices[i - 1].Z < 0
                                     || vertices[i - 1].Z > 1))
                {
                    Console.WriteLine("");
                    poligons.RemoveAt(j);
                    j--;
                }
            }

            TransformVectors(viewPortMatrix);
        }

        public void ToWorldCoordinates()
        {
        }

        public Matrix4x4 ToViewerCoordinates(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zAxis = Vector3.Normalize(eye - target);
            var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            var yAxis = Vector3.Normalize(Vector3.Cross(zAxis, xAxis));  //up;

            var viewerMatrix = new Matrix4x4(xAxis.X, xAxis.Y, xAxis.Z, 0,
                                             yAxis.X, yAxis.Y, yAxis.Z, 0,
                                             zAxis.X, zAxis.Y, zAxis.Z, 0,
                                                   0, 0, 0, 1);

            var translation = Matrix4x4.CreateTranslation(new Vector3(-Vector3.Dot(xAxis, eye), -Vector3.Dot(yAxis, eye), -Vector3.Dot(zAxis, eye)));

            //Matrix4x4

            //return Matrix4x4.CreateLookAt(eye, target, up);

            return viewerMatrix * translation;
        }

        public Matrix4x4 ToProjectionCoordinates()
        {
            var zNear = 0.2f;
            var zFar = 15;
            var aspect = pictureBoxPaintArea.Width / (float)pictureBoxPaintArea.Height;
            var fov = (float)Math.PI * (60) / 180;

            var m00 = 1 / (aspect * (float)Math.Tan(fov / 2));
            var m11 = 1 / (float)Math.Tan(fov / 2);
            var m22 = zFar / (zNear - zFar);
            var m23 = (zNear * zFar) / (zNear - zFar);

            var projectionMatrix = new Matrix4x4(m00, 0, 0, 0,
                                                   0, m11, 0, 0,
                                                   0, 0, m22, m23,
                                                   0, 0, -1, 0);

            return Matrix4x4.Transpose(projectionMatrix);
        }

        public Matrix4x4 ToViewPortCoordinates()
        {
            var width = pictureBoxPaintArea.Width * 3 / 4;
            var height = pictureBoxPaintArea.Height * 3 / 4;

            var xMin = pictureBoxPaintArea.Width / 8;
            var yMin = pictureBoxPaintArea.Height / 8;

            var m00 = width / 2;
            var m11 = -height / 2;
            var m03 = xMin + (width / 2);
            var m13 = yMin + (height / 2);
            var m22 = 255 / 2f;

            var viewerMatrix = new Matrix4x4(m00, 0, 0, m03,
                                               0, m11, 0, m13,
                                               0, 0, m22, m22,
                                               0, 0, 0, 1);

            viewerMatrix.Translation = new Vector3(m03, m13, m22);

            //TransformVectors(viewerMatrix);

            return viewerMatrix;
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Graphics bm)
        {
            float x = x1;
            float y = y1;
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / (float)length;
            var stepy = (y2 - y1) / (float)length;

            Brush aBrush = (Brush)Brushes.White;
            //bm.FillRectangle(aBrush, x, y, 1, 1);
            for (int i = 1; i <= length; i++)
            {
                bm.FillRectangle(aBrush, x, y, 1, 1);
                x += stepx;
                y += stepy;
            }
        }

        public void Draw()
        {

            var width = pictureBoxPaintArea.Width;
            var height = pictureBoxPaintArea.Height;

            if (width == 0 || height == 0)
                return;

            var bm = new Bitmap(width, height);
            using (var gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.Black);

                foreach (var poligon in poligons)
                {

                    //if (poligon.Any(i => vertices[i-1].X <= -1 || vertices[i-1].X >= 1 || vertices[i-1].Y <= -1 || vertices[i-1].Y >= 1 || vertices[i-1].Z <= 0 || vertices[i-1].Z >= 1))
                    //    continue;


                    for (int i = 0; i < poligon.Length; i++)
                    {
                        var k = poligon[i] - 1;
                        var j = poligon[(i + 1) % poligon.Length] - 1;

                        var X1 = (vertices[k].X);
                        var X2 = (vertices[j].X);
                        var Y1 = (vertices[k].Y);
                        var Y2 = (vertices[j].Y);

                        DrawLine((int)X1, (int)Y1, (int)X2, (int)Y2, gr);
                    };
                };
            }

            pictureBoxPaintArea.Image = bm;
        }


        public void TransformVectors(Matrix4x4 transformMatrix)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                //var temp = Vector4.Transform(new Vector4(vertices[i],1), transformMatrix);
                //vertices[i] = Vector4.Transform(vertices[i], transformMatrix);
                vertices[i] = Vector4.Transform(vertices[i], transformMatrix);
            }
        }

        public Matrix4x4 ScaleVectors(float x)
        {
            var scaleMatrix = new Matrix4x4(x, 0, 0, 0,
                                            0, x, 0, 0,
                                            0, 0, x, 0,
                                            0, 0, 0, 1);

            TransformVectors(scaleMatrix);

            return scaleMatrix;
        }

        public void MoveVectors(Vector3 vector)
        {
            var translationMatrix = new Matrix4x4(1, 0, 0, vector.X,
                                                  0, 1, 0, vector.Y,
                                                  0, 0, 1, vector.Z,
                                                  0, 0, 0, 1);
            translationMatrix.Translation = vector;

            TransformVectors(translationMatrix);
        }

        public void RotateXVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(1, 0, 0, 0,
                                             0, cos, -sin, 0,
                                             0, sin, cos, 0,
                                             0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateYVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(cos, 0, sin, 0,
                                               0, 1, 0, 0,
                                             -sin, 0, cos, 0,
                                               0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateZVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(cos, -sin, 0, 0,
                                             sin, cos, 0, 0,
                                              0, 0, 1, 0,
                                              0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        private void _MouseWheel(object sender, MouseEventArgs e)
        {
            //ScaleVectors(1.0f - (e.Delta / 1000.0f));

            if ((viewPoint.Z + (e.Delta / 700.0f)) < 0)
            {
                return;
            }

            viewPoint.Z += (e.Delta / 700.0f);
            Transform();
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            Transform();
            Draw();
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void pictureBoxPaintArea_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            lastPoint = Point.Empty;
        }

        private void pictureBoxPaintArea_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = e.Location;
            isMouseDown = true;

        }

        private void pictureBoxPaintArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown == true)//check to see if the mouse button is down
            {

                if (lastPoint != Point.Empty)//if our last point is not null, which in this case we have assigned above

                {
                    Vector3 v = new Vector3(e.X - lastPoint.X, e.Y - lastPoint.Y, 0);
                    xRotation += v.Y;
                    yRotation += v.X;
                    lastPoint = e.Location;//keep assigning the lastPoint to the current mouse position
                    Transform();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Keys.W))
            {
                xRotation -= aDelta;
            };
            if (Keyboard.IsKeyDown(Keys.S))
            {
                xRotation += aDelta;
            };
            if (Keyboard.IsKeyDown(Keys.D))
            {
                yRotation -= aDelta;
            };
            if (Keyboard.IsKeyDown(Keys.A))
            {
                yRotation += aDelta;
            };
            if (Keyboard.IsKeyDown(Keys.Z))
            {
                viewPoint.Z += delta;
            };
            if (Keyboard.IsKeyDown(Keys.X))
            {
                if (viewPoint.Z - delta < 0)
                    return;

                viewPoint.Z -= delta;
            };

            Transform();
            Draw();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
