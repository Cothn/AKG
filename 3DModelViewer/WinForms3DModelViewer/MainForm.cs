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
        private List<Vector3> normalVertices;
        private List<Vector3> originalNormalVertices;
        private List<Vector3> textureVertices;
        private List<Vector3> originalTextureVertices;

        private List<int[][]> originalPoligons;
        private List<int[][]> poligons;

        Point lastPoint = Point.Empty;
        bool isMouseDown = false;

        Vector3 viewPoint = new Vector3(0, 0, 4);
        float delta = 0.1f;
        float aDelta = 1f;

        float xRotation = 0;
        float yRotation = 0;
        float zRotation = 0;
        

        public MainForm()
        {
            InitializeComponent();
            pictureBoxPaintArea.MouseWheel += _MouseWheel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ObjParser parser = new ObjParser();
            (originalVertices, originalPoligons, originalNormalVertices, originalTextureVertices) 
                = parser.Parse(@"D:\RepositHub\AKG\Head\Model.obj");
            Transform();
        }

        public void Transform()
        {
            vertices = new List<Vector4>(originalVertices);
            poligons = new List<int[][]>(originalPoligons);

            var eye = this.viewPoint;
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);

            var k = (float)Math.PI / 180;
            var rm = Matrix4x4.CreateFromYawPitchRoll((yRotation % 360) * k, (xRotation % 360) * k, (zRotation % 360) * k);

            eye = Vector3.Transform(eye, rm);
            up = Vector3.Transform(up, rm);


            //Place and transform model from local to Viewer coordinates
            var viewerMatrix = ToViewerCoordinates(eye, target, up);
            

            var projectionMatrix = ToProjectionCoordinates();

            var viewPortMatrix = ToViewPortCoordinates();

            var mainMatrix = viewerMatrix * projectionMatrix;

            TransformVectors(mainMatrix);
            // Чтобы завершить преобразование, нужно разделить каждую компоненту век-тора на компонент 
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] /= vertices[i].W;
            }


            removePoligons(poligons, this.viewPoint);
            
            TransformVectors(viewPortMatrix);


        }

        public void removePoligons(List<int[][]> poligons, Vector3 eye)
        {
            for (int j = 0; j < poligons.Count; j++)
            {
                var poligon = poligons[j];

                // Remove polygon cut with proj matrix
                if (poligon.Any(i => vertices[i[0] - 1].Z < 0 || vertices[i[0] - 1].Z > 1))
                {
                    //Console.WriteLine("1");
                    poligons.RemoveAt(j);
                    j--;
                }
                else
                {
                    var vector1 = vertices[poligon[1][0] - 1] - vertices[poligon[0][0] - 1];
                    var vector2 = vertices[poligon[2][0] - 1] - vertices[poligon[0][0] - 1];
                    var surfaceNormal = new Vector3((vector1.Y * vector2.Z - vector1.Z * vector2.Y),
                        (vector1.Z * vector2.X - vector1.X * vector2.Z),
                        (vector1.X * vector2.Y - vector1.Y * vector2.X));


                    if (surfaceNormal.X * eye.X + surfaceNormal.Y * eye.Y + surfaceNormal.Z * eye.Z < 0.0)
                    {
                        poligons.RemoveAt(j);
                        j--;
                    }

                }
            }
        }
        

        public Matrix4x4 ToViewerCoordinates(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zAxis = Vector3.Normalize(eye - target);
            var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            var yAxis = Vector3.Normalize(up);  //up;


            var viewerMatrix = new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, eye), -Vector3.Dot(yAxis, eye), -Vector3.Dot(zAxis, eye), 1);
            

            //var viewerMatrix = new Matrix4x4(
            //11, 12, 13, 14,
            //21, 22, 23, 24,
            //31, 32, 33, 34,
            //41, 42, 43, 44);
            
            return viewerMatrix;
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
                                                   0, 0, m22, -1,
                                                   0, 0, m23, 0);
            
            return projectionMatrix;
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
            var m22 = 255/2f;

            var viewerMatrix = new Matrix4x4(m00, 0, 0, 0,
                                               0, m11, 0, 0,
                                               0, 0, m22, 0,
                                               m03, m13, m22, 1);
            
            return viewerMatrix;
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Graphics bm, int pointWidth = 2, int pointHeight = 2)
        {
            float x = x1;
            float y = y1;
            float dx = Math.Abs(x2 - x1);
            float dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / (float)length;
            var stepy = (y2 - y1) / (float)length;

            Brush aBrush = (Brush)Brushes.White;

            for (int i = 1; i <= (int)length; i++)
            {
                bm.FillRectangle(aBrush, x, y, pointWidth, pointHeight);
                x += stepx;
                y += stepy;
            }
        }
        
        //отрисовка модели
        public void Draw()
        {
            var minX = vertices.Min(x => x.X);
            var maxX = vertices.Max(x => x.X);

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
                    float yMax = float.MinValue;
                    int indexMax = -1;
                    float yMin = float.MaxValue;

                    for (int i = 0; i < poligon.Length; i++)
                    {
                        var k = poligon[i][0] - 1;
                        var j = poligon[(i + 1) % poligon.Length][0] - 1;

                        var X1 = (vertices[k].X);
                        var X2 = (vertices[j].X);
                        var Y1 = (vertices[k].Y);
                        var Y2 = (vertices[j].Y);

                        DrawLine(X1, Y1, X2, Y2, gr);

                        if (vertices[k].Y > yMax)
                        {
                            indexMax = i;
                            yMax = vertices[k].Y;
                        }

                        if (vertices[k].Y < yMin)
                        {
                            yMin = vertices[k].Y;
                        }
                    };

                    //var skylineBegin = new Vector2((float)-width/2, yMax );
                    //var skylineEnd = new Vector2((float)width/2, yMax);
/*
                    var skylineBegin = new Vector2(minX - 1, yMax);
                    var skylineEnd = new Vector2(maxX + 1, yMax);
                     
                    Vector2 firstEdgeBegin, firstEdgeEnd;
                    Vector2 secondEdgeBegin, secondEdgeEnd;

                    var indexes = new List<int> {0, 1, 2};
                    indexes.Remove(indexMax);

                    firstEdgeBegin = new Vector2(vertices[poligon[indexes[0]][0] - 1].X, vertices[poligon[indexes[0]][0] - 1].Y);
                    firstEdgeEnd = new Vector2(vertices[poligon[indexMax][0] - 1].X, vertices[poligon[indexMax][0] - 1].Y);

                    secondEdgeBegin = new Vector2(vertices[poligon[indexes[1]][0] - 1].X, vertices[poligon[indexes[1]][0] - 1].Y);
                    secondEdgeEnd = new Vector2(vertices[poligon[indexMax][0] - 1].X, vertices[poligon[indexMax][0] - 1].Y);

                    while (skylineBegin.Y > yMin)
                    {
                        if (skylineBegin.Y < firstEdgeBegin.Y)
                        {
                            firstEdgeEnd = new Vector2(vertices[poligon[indexes[1]][0] - 1].X, vertices[poligon[indexes[1]][0] - 1].Y);
                        }

                        if (skylineBegin.Y < secondEdgeBegin.Y)
                        {
                            secondEdgeEnd = new Vector2(vertices[poligon[indexes[0]][0] - 1].X, vertices[poligon[indexes[0]][0] - 1].Y);
                        }

                        if (PointsCrossing.ArePointsCrossing(firstEdgeBegin, firstEdgeEnd, skylineBegin, skylineEnd) &&
                            PointsCrossing.ArePointsCrossing(secondEdgeBegin, secondEdgeEnd, skylineBegin, skylineEnd))
                        {
                            var firstPoint = PointsCrossing.CrossingPoint(firstEdgeBegin, firstEdgeEnd, skylineBegin, skylineEnd);
                            var secondPoint = PointsCrossing.CrossingPoint(secondEdgeBegin, secondEdgeEnd, skylineBegin, skylineEnd);

                            DrawLine(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y, gr);
                        }

                        skylineBegin.Y--;
                        skylineEnd.Y--;
                    }
                   */ 
                };
                
            }

            pictureBoxPaintArea.Image = bm;
        }
        

        
        
        public void TransformVectors(Matrix4x4 transformMatrix)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = Vector4.Transform(vertices[i], transformMatrix);
            }
        }

        private void _MouseWheel(object sender, MouseEventArgs e)
        {

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

                if (lastPoint != Point.Empty)//if our last point is not null

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
        
        /*
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
            var translationMatrix = new Matrix4x4(1, 0, 0, 0,
                                                  0, 1, 0, 0,
                                                  0, 0, 1, 0,
                                                  vector.X, vector.Y, vector.Z, 1);

            TransformVectors(translationMatrix);
        }

        public void RotateXVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(1, 0, 0, 0,
                                             0, cos, sin, 0,
                                             0, -sin, cos, 0,
                                             0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateYVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(cos, 0, -sin, 0,
                                               0, 1, 0, 0,
                                             sin, 0, cos, 0,
                                               0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateZVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(cos, sin, 0, 0,
                                             -sin, cos, 0, 0,
                                              0, 0, 1, 0,
                                              0, 0, 0, 1);

            TransformVectors(rotateMatrix);
        }
*/

    }
}
