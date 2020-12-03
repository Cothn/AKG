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
        readonly Vector3 ambientLightColor = new Vector3(0.1F, 0.1F, 0.1F);
        readonly Vector3 defaultColor = new Vector3(255, 255, 255);
        readonly float shiness = 30;

        private List<Vector4> vertices;
        private List<Vector4> originalVertices;
        private List<Vector3> normalVertices;
        private List<Vector3> originalNormalVertices;
        private List<Vector3> textureVertices;
        private List<Vector3> originalTextureVertices;

        private List<int[][]> originalPoligons;
        private List<int[][]> poligons;

        private float[][] zBuffer;

        Point lastPoint = Point.Empty;
        bool isMouseDown = false;
        bool neadDrow = true;

        Vector3 viewPoint = new Vector3(0, 0, 4);
        //Vector3 viewPoint = new Vector3(0, 5, 15);
        float delta = 0.1f;
        float aDelta = 1f;

        Vector4 lightPoint = new Vector4(10, 11, 2F, 0);
        private List<Vector4> worldVertices;
        private List<Vector4> viewerVertices;
        private List<Vector4> projectionVertices;

        float xRotation = 0;
        float yRotation = 0;
        float zRotation = 0;

        private int skippedPixelsDraw = 0;

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
            //parser.Parse(@"D:\Github projects\AKG\Head\Model.obj");
            //= parser.Parse(@"D:\RepositHub\AKG\Shovel Knight\Model.obj");
            Transform();
        }

        public void Transform()
        {
            vertices = new List<Vector4>(originalVertices);
            normalVertices = new List<Vector3>(originalNormalVertices);
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

            //TransformVectors(mainMatrix);

            TransformVectors(viewerMatrix);
            TransformNormals(viewerMatrix);

            viewerVertices = new List<Vector4>(vertices);

            TransformVectors(projectionMatrix);
            //TransformNormals(projectionMatrix);

            // Чтобы завершить преобразование, нужно разделить каждую компоненту век-тора на компонент 
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] /= vertices[i].W;
            }

            projectionVertices = new List<Vector4>(vertices);

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
                                               0, 0, 1, 0,
                                               m03, m13, 0, 1);
            
            return viewerMatrix;
        }
        
/*
        public void DrawLine(float x1, float y1, float z1, float x2, float y2, float z2, Graphics bm, float colorScale = -1, int pointWidth = 1, int pointHeight = 1)
        {
            float x = x1;
            float y = y1;
            float z = z1;
            float dx = Math.Abs(x2 - x1);
            float dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / length;
            var stepy = (y2 - y1) / length;
            var stepz = (z2 - z1) / length;

            Brush brush;

            if (colorScale < 0)
            {
                brush = Brushes.White;
            }
            else
            {
                brush = new SolidBrush(Color.FromArgb((int)(255 * colorScale), (int)(255 * colorScale), (int)(255 * colorScale)));
            }

            //Brush bBrush = Brushes.Black;

            for (int i = 1; i <= (int)length; i++)
            {
                if (y > 0 && y < pictureBoxPaintArea.Height && x > 0 && x < pictureBoxPaintArea.Width && zBuffer[(int)y][(int)x] > z)
                {
                    zBuffer[(int)y][(int)x] = z;

                    bm.FillRectangle(brush, x, y, pointWidth, pointHeight);
                }
                else
                {
                    skippedPixelsDraw++;
                    //bm.FillRectangle(bBrush, x, y, pointWidth, pointHeight);
                }

                x += stepx;
                y += stepy;
                z += stepz;
            }
        }
*/
        //public void DrawTriangle(Vector4 A, Vector4 B, Vector4 C, Graphics bm, float colorScale = -1, int pointWidth = 1, int pointHeight = 1)
        public void DrawTriangle(Vector4 A, Vector4 B, Vector4 C, Graphics bm, Vector3 color, int pointWidth = 1, int pointHeight = 1)
        {
            Brush brush;
            var startVector = A;

            //if (colorScale < 0)
            //{
            //    brush = Brushes.White;
            //}
            //else
            //{
            //    brush = new SolidBrush(Color.FromArgb((int)(255 * colorScale), (int)(255 * colorScale), (int)(255 * colorScale)));
            //}

            brush = new SolidBrush(Color.FromArgb((int)Math.Min(color.X, 255) , (int)Math.Min(color.Y, 255), (int)Math.Min(color.Z, 255)));


            for (int y = (int) Math.Floor(startVector.Y); y >= (int) Math.Ceiling(C.Y); y--)
            {
                if (y > 0 && y < pictureBoxPaintArea.Height)

                {
                    float startX;
                    float startZ;
                    if (y >= B.Y)
                    {
                        var BAK = (y - B.Y) / (startVector.Y - y);
                        startX = (B.X + startVector.X * BAK) / (BAK + 1);
                        startZ = (B.Z + startVector.Z * BAK) / (BAK + 1);
                    }
                    else
                    {
                        var CBK = (y - C.Y) / (B.Y - y);
                        startX = (C.X + B.X * CBK) / (CBK + 1);
                        startZ = (C.Z + B.Z * CBK) / (CBK + 1);
                    }

                    
                    var CAK = (y - C.Y) / (startVector.Y - y);
    
                    var endX = (C.X + startVector.X * CAK) / (CAK + 1);
                    var endZ = (C.Z + startVector.Z * CAK) / (CAK + 1);
                    if (startX > endX)
                    {
                        Swap(ref startX, ref endX);
                        Swap(ref startZ, ref endZ);
                    }

                    for (int x = (int) Math.Ceiling(startX); x <= (int) Math.Floor(endX); x++)
                    {
                        if (x > 0 && x < pictureBoxPaintArea.Width)
                        {
                            
                            var K = (x - startX) / (endX - x);
                            var z = (startZ + endZ *K) / (K + 1);
                            if (zBuffer[(int) y][(int) x] > z)
                            {
                                zBuffer[(int) y][(int) x] = z;

                                bm.FillRectangle(brush, x, y, pointWidth, pointHeight);
                            }
                            else
                            {
                                skippedPixelsDraw++;
                            }
                        }
                    }

                }
            }
        }

        //отрисовка модели
        public void Draw()
        {
            InitializeZBuffer();

            SortPoligonsByMinZ();

            skippedPixelsDraw = 0;

            var minX = vertices.Min(x => x.X);
            var maxX = vertices.Max(x => x.X);

            var width = pictureBoxPaintArea.Width+1;
            var height = pictureBoxPaintArea.Height+1;

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
                    int indexMin = -1;
                    float poligonColorScale;
                    Vector3 poligonColor;

                    //poligonColorScale = FindPoligonLambertComponent(poligon);
                    poligonColor = FindPoligonFongoColor(poligon);

                    for (int i = 0; i < poligon.Length; i++)
                    {
                        var k = poligon[i][0] - 1;
                        var j = poligon[(i + 1) % poligon.Length][0] - 1;

                        var X1 = (vertices[k].X);
                        var X2 = (vertices[j].X);
                        var Y1 = (vertices[k].Y);
                        var Y2 = (vertices[j].Y);
                        var Z1 = (vertices[k].Z);
                        var Z2 = (vertices[j].Z);
                    }

                    int[][] sortedPoligonVertices = new int[poligon.Length][];


                    Array.Copy(poligon, sortedPoligonVertices, poligon.Length);

                    if (vertices[sortedPoligonVertices[0][0] - 1].Y > vertices[sortedPoligonVertices[1][0] - 1].Y)
                    {
                        Swap(ref sortedPoligonVertices[0], ref sortedPoligonVertices[1]);
                    }

                    if (vertices[sortedPoligonVertices[0][0] - 1].Y > vertices[sortedPoligonVertices[2][0] - 1].Y)
                    {
                        Swap(ref sortedPoligonVertices[0], ref sortedPoligonVertices[2]);
                    }

                    if (vertices[sortedPoligonVertices[1][0] - 1].Y > vertices[sortedPoligonVertices[2][0] - 1].Y)
                    {
                        Swap(ref sortedPoligonVertices[1], ref sortedPoligonVertices[2]);
                    }

                    var t0 = vertices[sortedPoligonVertices[0][0] - 1];
                    var t1 = vertices[sortedPoligonVertices[1][0] - 1];
                    var t2 = vertices[sortedPoligonVertices[2][0] - 1];
                    
                    //DrawTriangle(t2, t1, t0,  gr, poligonColorScale);
                    DrawTriangle(t2, t1, t0, gr, poligonColor);

                };
                
            }

            LskippedPixelsDraw.Text = skippedPixelsDraw.ToString();

            pictureBoxPaintArea.Image = bm;
        }
        

        
        
        public void TransformVectors(Matrix4x4 transformMatrix)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = Vector4.Transform(vertices[i], transformMatrix);
            }
        }

        public void TransformNormals(Matrix4x4 transformMatrix)
        {
            for (int i = 0; i < normalVertices.Count; i++)
            {
                normalVertices[i] = Vector3.Transform(normalVertices[i], transformMatrix);
            }
        }

        private void InitializeZBuffer()
        {
            var width = pictureBoxPaintArea.Width + 1;
            var height = pictureBoxPaintArea.Height + 1;

            var maxZ = float.MaxValue;

            zBuffer = new float[height][];

            for (int i = 0; i < height; i++)
            {
                var arr = new float[width];

                for (int j = 0; j < width; j++)
                {
                    arr[j] = maxZ;
                }

                zBuffer[i] = arr;
            }
        }

        private void SortPoligonsByMinZ()
        {
            var poligonsZ = new List<(int num, float[] val)>(poligons.Count);

            for (int i = 0; i < poligons.Count; i++)
            {
                var arr = new float[3];

                for (var j = 0; j < poligons[i].Length; j++)
                {
                    arr[j] = vertices[poligons[i][j][0] - 1].Z;
                }

                poligonsZ.Add((i, arr));
            }

            poligonsZ = (List<(int num, float[] val)>)poligonsZ.OrderBy(x => x.val.Min()).ToList();

            var newPoligonsList = new List<int[][]>();

            for (int i = 0; i < poligons.Count; i++)
            {
                newPoligonsList.Add(poligons[poligonsZ[i].num]);
            }

            poligons = newPoligonsList;
        }

        private float FindPoligonLambertComponent(int[][] poligon)
        {
            float averageLambertComponent = 0;

            for (int i = 0; i < poligon.Length; i++)
            {
                averageLambertComponent +=
                    VertexColorByLambert(viewerVertices[poligon[i][0] - 1], normalVertices[poligon[i][2] - 1]);
            }

            averageLambertComponent /= poligon.Length;

            return averageLambertComponent;
        }

        private float VertexColorByLambert(Vector4 vertexPosition, Vector3 vertexNormal)
        {
            Vector4 lightDirection = lightPoint - vertexPosition;
            //Vector3 lightDirection = new Vector3(lightPoint.X, lightPoint.Y, lightPoint.Z) - new Vector3(vertexPosition.X, vertexPosition.Y, vertexPosition.Z);
            Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));

            Vector3 N = Vector3.Normalize(vertexNormal);

            float lambertComponent = Math.Max(Vector3.Dot(N, -L), 0);

            return lambertComponent;
        }

        private Vector3 FindPoligonFongoColor(int[][] poligon)
        {
            Vector3 averageFongoComponent = VertexColorByFongo(viewerVertices[poligon[0][0] - 1], normalVertices[poligon[0][2] - 1], defaultColor); ;

            for (int i = 1; i < poligon.Length; i++)
            {
                averageFongoComponent +=
                    VertexColorByFongo(viewerVertices[poligon[i][0] - 1], normalVertices[poligon[i][2] - 1], defaultColor);
            }

            averageFongoComponent /= poligon.Length;

            return averageFongoComponent;
        }

        private Vector3 VertexColorByFongo(Vector4 vertexPosition, Vector3 vertexNormal, Vector3 color)
        {
            Vector4 lightDirection = lightPoint - vertexPosition;

            Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));
            Vector3 N = Vector3.Normalize(vertexNormal);

            float lambertComponent = Math.Max(Vector3.Dot(N, -L), 0);
            Vector3 diffuseLight = color * lambertComponent;

            Vector3 eyeVector = new Vector3(-vertexPosition.X, -vertexPosition.Y, -vertexPosition.Z);

            Vector3 R = Vector3.Normalize(eyeVector);
            Vector3 E = Vector3.Reflect(L, N);

            float specular = (float)Math.Pow(Math.Max(Vector3.Dot(E, R), 0), shiness);
            Vector3 specularLight = color * specular;

            Vector3 sumColor = ambientLightColor + diffuseLight + specularLight;

            return sumColor;
        }

        private void Swap(ref int[] first, ref int[] second)
        {
            var temp = first;
            first = second;
            second = temp;
        }
        
        private void Swap(ref float first, ref float second)
        {
            var temp = first;
            first = second;
            second = temp;
        }

        private void Swap(ref Vector4 first, ref Vector4 second)
        {
            var temp = first;
            first = second;
            second = temp;
        }

        private void _MouseWheel(object sender, MouseEventArgs e)
        {

            if ((viewPoint.Z + (e.Delta / 700.0f)) < 0)
            {
                return;
            }

            viewPoint.Z += (e.Delta / 700.0f);
            Transform();
            neadDrow = true;
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
                    neadDrow = true;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if (Keyboard.IsKeyDown(Keys.W))
            {
                xRotation -= aDelta;
                neadDrow = true;
            };
            if (Keyboard.IsKeyDown(Keys.S))
            {
                xRotation += aDelta;
                neadDrow = true;
            };
            if (Keyboard.IsKeyDown(Keys.D))
            {
                yRotation -= aDelta;
                neadDrow = true;
            };
            if (Keyboard.IsKeyDown(Keys.A))
            {
                yRotation += aDelta;
                neadDrow = true;
            };
            if (Keyboard.IsKeyDown(Keys.Z))
            {
                viewPoint.Z += delta;
                neadDrow = true;
            };
            if (Keyboard.IsKeyDown(Keys.X))
            {
                if (viewPoint.Z - delta < 0)
                    return;

                viewPoint.Z -= delta;
                neadDrow = true;
            };

            if (neadDrow)
            {
                Transform();
                Draw();
                neadDrow = false;
            }
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
