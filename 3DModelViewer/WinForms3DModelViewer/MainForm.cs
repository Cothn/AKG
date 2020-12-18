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
        private string FilesPath;

        private const string FilesPathM = @"D:\Github projects\AKG\Head\";
        //private const string FilesPathM = @"D:\RepositHub\AKG\Head\";

        private const string FilesPathC = @"D:\Github projects\AKG\Skybox\";
        //private const string FilesPathC = @"D:\RepositHub\AKG\Skybox\";

        private List<string> filesNames;

        private List<string> filesNamesM = new List<string> { "Model.obj", "Albedo Map.png", "Normal Map.png", "Specular Map.png" };
        private List<string> filesNamesC = new List<string> { "Model.obj" };

        readonly Vector3 defaultColor = new Vector3(255, 255, 255);
        readonly float shiness = 30;

        private List<Vector4> vertices;
        private List<Vector4> originalVertices;
        private List<Vector3> normalVertices;
        private List<Vector3> originalNormalVertices;
        private List<Vector3> textureVertices;
        private List<Vector3> originalTextureVertices;

        private List<Vector4> verticesM;
        private List<Vector4> originalVerticesM;
        private List<Vector3> normalVerticesM;
        private List<Vector3> originalNormalVerticesM;
        private List<Vector3> textureVerticesM;
        private List<Vector3> originalTextureVerticesM;

        private List<Vector4> verticesC;
        private List<Vector4> originalVerticesC;
        private List<Vector3> normalVerticesC;
        private List<Vector3> originalNormalVerticesC;
        private List<Vector3> textureVerticesC;
        private List<Vector3> originalTextureVerticesC;

        private List<int[][]> originalPoligons;
        private List<int[][]> poligons;

        private List<int[][]> originalPoligonsM;
        private List<int[][]> poligonsM;

        private List<int[][]> originalPoligonsC;
        private List<int[][]> poligonsC;

        public float[] Wbuf;
        private float[][] zBuffer;

        Point lastPoint = Point.Empty;
        bool isMouseDown = false;
        bool neadDrow = true;

        Vector3 viewPoint = new Vector3(0, 0, 2);
        Vector3 eyePoint;

        float delta = 0.1f;
        float aDelta = 1f;
        float lDelta = 0.02f;
            ///0.0000001f
        Vector4 origlightPoint = new Vector4(-1.08f, -1.26f, 0.96f, 1);
        Vector4 lightPoint;
        private List<Vector4> worldVertices;
        private List<Vector4> viewerVertices;
        private List<Vector4> projectionVertices;

        float xRotation = 0;
        float yRotation = 0;
        float zRotation = 0;

        private int skippedPixelsDraw = 0;

        private Bitmap albedoMap;
        private Bitmap normalMap;
        private Bitmap specularMap;

        public bool isAlbedoMap = false;
        public bool isNormalMap = false;
        public bool isSpecularMap = false;

        public Matrix4x4 toViewerCoord;
        public Matrix4x4 toProjectionCoord;

        public bool IsModelNow;

        public MainForm()
        {
            InitializeComponent();
            pictureBoxPaintArea.MouseWheel += _MouseWheel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ObjParser parser = new ObjParser();
                (originalVerticesM, originalPoligonsM, originalNormalVerticesM, originalTextureVerticesM)
                    //= parser.Parse(@"D:\RepositHub\AKG\Head\Model.obj");
                    //parser.Parse(@"D:\RepositHub\AKG\moon.obj");
                    //parser.Parse(@"D:\Github projects\AKG\Head\Model.obj");
                    = //parser.Parse(@"D:\RepositHub\AKG\Head\Model.obj");
                    parser.Parse(FilesPathM + filesNamesM[0]);
            //= parser.Parse(@"D:\RepositHub\AKG\Shovel Knight\Model.obj");

            (originalVerticesC, originalPoligonsC, originalNormalVerticesC, originalTextureVerticesC) = parser.Parse(FilesPathC + filesNamesC[0]);

            FilesPath = FilesPathM;
            filesNames = filesNamesM;

            if (isAlbedoMap)
            {
                albedoMap = new Bitmap(FilesPath + filesNames[1]);
            }

            if (isNormalMap)
            {
                normalMap = new Bitmap(FilesPath + filesNames[2]);
            }

            if (isSpecularMap)
            {
                specularMap = new Bitmap(FilesPath + filesNames[3]);
            }

            Transform(false);
            Transform();

        }

        public void Transform(bool isModel = true)
        {
            if (isModel)
            {
                originalVertices = originalVerticesM;
                originalNormalVertices = originalNormalVerticesM;
                originalTextureVertices = originalTextureVerticesM;
                originalPoligons = originalPoligonsM;
            }
            else
            {
                originalVertices = originalVerticesC;
                originalNormalVertices = originalNormalVerticesC;
                originalTextureVertices = originalTextureVerticesC;
                originalPoligons = originalPoligonsC;
            }

            vertices = new List<Vector4>(originalVertices);
            normalVertices = new List<Vector3>(originalNormalVertices);
            textureVertices = new List<Vector3>(originalTextureVertices);
            poligons = new List<int[][]>(originalPoligons);

            var eye = this.viewPoint;
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);

            eyePoint = eye;

            var k = (float)Math.PI / 180;
            //var rm = Matrix4x4.CreateFromYawPitchRoll((yRotation % 360) * k, (xRotation % 360) * k, (zRotation % 360) * k);

            Matrix4x4 rm;

            if (isModel)
            {
                rm = Matrix4x4.CreateFromYawPitchRoll((yRotation % 360) * k, (xRotation % 360) * k, (zRotation % 360) * k);
            }
            else
            {
                rm = Matrix4x4.CreateFromYawPitchRoll((yRotation % 360) * k, (xRotation % 360) * k, (zRotation % 360) * k);
                //rm.M41 = 0;
                //rm.M42 = 0;
                //rm.M43 = 0;

                //rm.M14 = 0;
                //rm.M24 = 0;
                //rm.M34 = 0;
            }


            eye = Vector3.Transform(eye, rm);
            up = Vector3.Transform(up, rm);

            //Place and transform model from local to Viewer coordinates
            Matrix4x4 viewerMatrix;

            if (isModel)
            {
                viewerMatrix = ToViewerCoordinates(eye, target, up);
            }
            else
            {
                viewerMatrix = ToViewerCoordinatesCube(eye, target, up);
            }

            toViewerCoord = viewerMatrix;


            Matrix4x4 projectionMatrix;

            if (isModel)
            {
                projectionMatrix = ToProjectionCoordinates();
            }
            else
            {
                projectionMatrix = ToProjectionCoordinates();
            }

            toProjectionCoord = projectionMatrix;


            Matrix4x4 viewPortMatrix;

            if (isModel)
            {
                viewPortMatrix = ToViewPortCoordinates();
            }
            else
            {
                viewPortMatrix = ToViewPortCoordinates();
            }

            var mainMatrix = viewerMatrix * projectionMatrix;

            eye = eyePoint;
            //lightPoint = new Vector4(eye.X+5, eye.Y, eye.Z +1, 1);
            lightPoint = origlightPoint;
            //lightPoint  =  Vector4.Transform(lightPoint , viewerMatrix );
            //lightPoint /= lightPoint.W;
            lightPoint  =  Vector4.Transform(lightPoint , projectionMatrix);
            lightPoint /= lightPoint.W;

            TransformVectors(viewerMatrix);
            TransformNormals(viewerMatrix);


            viewerVertices = new List<Vector4>(vertices);

            TransformVectors(projectionMatrix);
            //TransformNormals(projectionMatrix);
            TransformNormals4(projectionMatrix);


            Wbuf = new float[vertices.Count];
            // Чтобы завершить преобразование, нужно разделить каждую компоненту век-тора на компонент 
            for (int i = 0; i < vertices.Count; i++)
            {
                Wbuf[i] = vertices[i].W;
                vertices[i] /=  vertices[i].W;
            }
            

            projectionVertices = new List<Vector4>(vertices);

            if (isModel)
            {
                removePoligons(poligons, this.viewPoint);
            }

            TransformVectors(viewPortMatrix);
            //TransformNormals(viewPortMatrix);

            if (isModel)
            {
                verticesM = vertices;
                normalVerticesM = normalVertices;
                textureVerticesM = textureVertices;

                poligonsM = poligons;
            }
            else
            {
                verticesC = vertices;
                normalVerticesC = normalVertices;
                textureVerticesC = textureVertices;

                poligonsC = poligons;
            }
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

        public Matrix4x4 ToViewerCoordinatesCube(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zAxis = Vector3.Normalize(eye - target);
            var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            var yAxis = Vector3.Normalize(up);  //up;

            var scale = 5;

            var viewerMatrix = new Matrix4x4(
                xAxis.X * scale, yAxis.X * scale, zAxis.X, 0,
                xAxis.Y * scale, yAxis.Y * scale, zAxis.Y, 0,
                xAxis.Z * scale, yAxis.Z * scale, zAxis.Z, 0,
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

        public Matrix4x4 ToProjectionCoordinatesCube()
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
                0, 0, m22, 0,
                0, 0, 0, 0);

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

        public Matrix4x4 ToViewPortCoordinatesCube()
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

            var viewerMatrix = new Matrix4x4(m00, 0, 0, 0,
                0, m11, 0, 0,
                0, 0, 1, 0,
                1, 1, 0, 1);

            return viewerMatrix;
        }

        public Matrix4x4 FromViewPortCoordinates()
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

            Matrix4x4.Invert(viewerMatrix, out var koefMatrix);
            return koefMatrix;
        }

        public void DrawTriangle(int[][] poligon, Graphics bm, int pointWidth = 1, int pointHeight = 1)
        {
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

            var C = vertices[sortedPoligonVertices[0][0] - 1];
            var B = vertices[sortedPoligonVertices[1][0] - 1];
            var A = vertices[sortedPoligonVertices[2][0] - 1];

            Brush brush;
            var startVector = A;

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
                                //if (IsModelNow)
                                //{
                                    zBuffer[(int) y][(int) x] = z;
                                //}

                                Vector4 pixelVector = new Vector4(x, y, z, 1);

                                var Anormal = normalVertices[sortedPoligonVertices[2][2] - 1];
                                var Bnormal = normalVertices[sortedPoligonVertices[1][2] - 1];
                                var Cnormal = normalVertices[sortedPoligonVertices[0][2] - 1];


                                Vector3 Atexture, Btexture, Ctexture;
                                Vector3 pixelTextureKoef = new Vector3(0, 0, 0);
                                Vector3 pixelTexture = new Vector3(0,0,0);

                                if (isAlbedoMap || isNormalMap || isSpecularMap)
                                {
                                    Atexture = textureVertices[sortedPoligonVertices[2][1] - 1];
                                    Btexture = textureVertices[sortedPoligonVertices[1][1] - 1];
                                    Ctexture = textureVertices[sortedPoligonVertices[0][1] - 1];
                                    A.W = Wbuf[sortedPoligonVertices[2][0] - 1];
                                    B.W = Wbuf[sortedPoligonVertices[1][0] - 1];
                                    C.W = Wbuf[sortedPoligonVertices[0][0] - 1];
                                    
                                    Atexture /= A.W;
                                    Atexture.Z = 1/A.W;
                                    Btexture /= B.W;
                                    Btexture.Z = 1/B.W;
                                    Ctexture /= C.W;
                                    Ctexture.Z = 1/C.W;
                                    
                                    pixelTextureKoef = LinearInterpolation(A, B, C, pixelVector, Atexture, Btexture, Ctexture);
                                    //var baricentrikCoord = CountSystemOfEquations(A, B, C, pixelVector);
                                    pixelTextureKoef /= pixelTextureKoef.Z;

                                    //pixelTextureKoef.X =
                                    //    (Atexture.X / A.Z* baricentrikCoord.X + Btexture.X / B.Z* baricentrikCoord.Y + Ctexture.X / C.Z* baricentrikCoord.Z) 
                                    //    ;
                                    //pixelTextureKoef.Y =
                                    //    (Atexture.Y / A.Z* baricentrikCoord.X + Btexture.Y / B.Z* baricentrikCoord.Y + Ctexture.Y / C.Z* baricentrikCoord.Z);
                                    //pixelTextureKoef.Z =
                                    //    (baricentrikCoord.X / A.Z + baricentrikCoord.Y / B.Z + baricentrikCoord.Z  / C.Z) ;
                                    //pixelTextureKoef.X /= pixelTextureKoef.Z;
                                    //pixelTextureKoef.Y /= pixelTextureKoef.Z;
                                    //pixelTextureKoef = LinearInterpolationT(A, B, C, pixelVector, Atexture, Btexture, Ctexture);
                                }

                                Color albedoColor = Color.Blue;

                                Vector3 diffuseAndAmbientKoef = new Vector3(5, 5, 5);

                                if (isAlbedoMap)
                                {
                                    pixelTexture.X = pixelTextureKoef.X * albedoMap.Width;
                                    pixelTexture.Y = pixelTextureKoef.Y * albedoMap.Height;

                                    pixelTexture.X = pixelTexture.X < 0
                                        ? 0
                                        : (pixelTexture.X >= albedoMap.Width ? albedoMap.Width - 1 : pixelTexture.X);

                                    pixelTexture.Y = pixelTexture.Y < 1
                                        ? 1
                                        : (pixelTexture.Y >= albedoMap.Height ? albedoMap.Height : pixelTexture.Y);


                                    albedoColor = albedoMap.GetPixel((int)pixelTexture.X, albedoMap.Height - (int)pixelTexture.Y);

                                    (diffuseAndAmbientKoef.X, diffuseAndAmbientKoef.Y, diffuseAndAmbientKoef.Z) = (albedoColor.R, albedoColor.G, albedoColor.B);
                                }


                                Vector3 pixelNormal;

                                if (isNormalMap)
                                {
                                    pixelTexture.X = pixelTextureKoef.X * normalMap.Width;
                                    pixelTexture.Y = pixelTextureKoef.Y * normalMap.Height;

                                    pixelTexture.X = pixelTexture.X < 0
                                        ? 0
                                        : (pixelTexture.X >= normalMap.Width ? normalMap.Width - 1 : pixelTexture.X);

                                    pixelTexture.Y = pixelTexture.Y < 1
                                        ? 1
                                        : (pixelTexture.Y >= normalMap.Height ? normalMap.Height : pixelTexture.Y);


                                    Color normalColor = normalMap.GetPixel((int)pixelTexture.X, normalMap.Height - (int)pixelTexture.Y);

                                    //pixelNormal.X = normalColor.R;
                                    //pixelNormal.Y = normalColor.G;
                                    //pixelNormal.Z = normalColor.B;

                                    //pixelNormal.X = normalColor.R * 2 - 1;
                                    //pixelNormal.Y = normalColor.G * 2 - 1;
                                    //pixelNormal.Z = normalColor.B * 2 - 1;

                                    pixelNormal.X = (normalColor.R / 255F) * 2 - 1;
                                    pixelNormal.Y = (normalColor.G / 255F) * 2 - 1;
                                    pixelNormal.Z = (normalColor.B / 255F) * 2 - 1;

                                    pixelNormal = Vector3.Normalize(pixelNormal);

                                    pixelNormal = Vector3.Transform(pixelNormal, toViewerCoord);
                                    
                                    pixelNormal = Vector3.Transform(pixelNormal, toProjectionCoord);

                                    //pixelNormal.X = pixelNormal.X * 2 - 1;
                                    //pixelNormal.Y = pixelNormal.Y * 2 - 1;
                                    //pixelNormal.Z = pixelNormal.Z * 2 - 1;

                                    //pixelNormal = pixelNormal * -1;
                                }
                                else
                                {
                                    pixelNormal = LinearInterpolation(A, B, C, pixelVector, Anormal, Bnormal, Cnormal);
                                }


                                Vector3 specularKoef = new Vector3(1,1,1);

                                if (isSpecularMap)
                                {
                                    pixelTexture.X = pixelTextureKoef.X * specularMap.Width;
                                    pixelTexture.Y = pixelTextureKoef.Y * specularMap.Height;

                                    pixelTexture.X = pixelTexture.X < 0
                                        ? 0
                                        : (pixelTexture.X >= specularMap.Width ? specularMap.Width - 1 : pixelTexture.X);

                                    pixelTexture.Y = pixelTexture.Y < 1
                                        ? 1
                                        : (pixelTexture.Y >= specularMap.Height ? specularMap.Height : pixelTexture.Y);


                                    Color specularColor = specularMap.GetPixel((int)pixelTexture.X, specularMap.Height - (int)pixelTexture.Y);

                                    (specularKoef.X, specularKoef.Y, specularKoef.Z) = (specularColor.R, specularColor.G, specularColor.B);
                                }

                                if (IsModelNow)
                                {
                                    Vector3 color = VertexColorByFongo(pixelNormal, pixelVector, diffuseAndAmbientKoef,
                                        diffuseAndAmbientKoef, specularKoef);

                                    brush = new SolidBrush(Color.FromArgb((int) Math.Min(color.X, 255),
                                        (int) Math.Min(color.Y, 255), (int) Math.Min(color.Z, 255)));
                                }
                                else
                                {
                                    brush = new SolidBrush(albedoColor);
                                }


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
            Bitmap bm;

            var width = pictureBoxPaintArea.Width + 1;
            var height = pictureBoxPaintArea.Height + 1;

            if (width == 0 || height == 0)
                return;

            bm = new Bitmap(width, height);

            IsModelNow = false;


            vertices = verticesC;
            normalVertices = normalVerticesC;
            textureVertices = textureVerticesC;

            poligons = poligonsC;

            InitializeZBuffer();

            SortPoligonsByMinZ();

            skippedPixelsDraw = 0;

            using (var gr = Graphics.FromImage(bm))
            {
                //gr.Clear(Color.WhiteSmoke);
                gr.Clear(Color.Black);

                foreach (var poligon in poligons)
                {
                    float yMax = float.MinValue;
                    int indexMax = -1;
                    float yMin = float.MaxValue;
                    int indexMin = -1;
                    float poligonColorScale;
                    Vector3 poligonColor;

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

                    DrawTriangle(poligon, gr);
                };

                
            

                vertices = verticesM;
                normalVertices = normalVerticesM;
                textureVertices = textureVerticesM;

                poligons = poligonsM;

                IsModelNow = true;

                InitializeZBuffer();

                SortPoligonsByMinZ();

                skippedPixelsDraw = 0;

                foreach (var poligon in poligons)
                {
                    float yMax = float.MinValue;
                    int indexMax = -1;
                    float yMin = float.MaxValue;
                    int indexMin = -1;
                    float poligonColorScale;
                    Vector3 poligonColor;

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

                    DrawTriangle(poligon, gr);
                };

            }

            //LskippedPixelsDraw.Text = skippedPixelsDraw.ToString();

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

        public void TransformNormals4(Matrix4x4 transformMatrix)
        {
            List<Vector4> normalVertices4 = new List<Vector4>();
            for (int i = 0; i < normalVertices.Count; i++)
            {
                normalVertices4.Add(Vector4.Transform(new Vector4(normalVertices[i], 1), transformMatrix));
            }

            for (int i = 0; i < normalVertices4.Count; i++)
            {
                normalVertices4[i] /= normalVertices4[i].W;
                normalVertices[i] = new Vector3(normalVertices4[i].X, normalVertices4[i].Y, normalVertices4[i].Z);
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

            Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));

            Vector3 N = Vector3.Normalize(vertexNormal);

            float lambertComponent = Math.Max(Vector3.Dot(N, L), 0);

            return lambertComponent;
        }

        private Vector3 VertexColorByLambertWithColor(Vector3 vertexPosition, Vector3 vertexNormal, Vector3 color)
        {
            Vector3 lightDirection = new Vector3(lightPoint.X, lightPoint.Y, lightPoint.Z) - vertexPosition;

            Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));

            Vector3 N = Vector3.Normalize(vertexNormal);

            float lambertComponent = Math.Max(Vector3.Dot(N, -L), 0);

            return lambertComponent * color;
        }

        private Vector3 VertexColorByFongo(Vector3 vertexNormal, Vector4 vertexpixel4, Vector3 ambientKoef, Vector3 diffuseKoef, Vector3 specularKoef)
        {
            var matrix = FromViewPortCoordinates();
            var vertexpixel = new Vector3(vertexpixel4.X, vertexpixel4.Y, vertexpixel4.Z);
            vertexpixel =  Vector3.Transform(vertexpixel, matrix);

            var ambientLightColor = new Vector3(3F, 1F, 3F) * (ambientKoef / 20f);
            
            var diffuzeKoefL = 4f;
            if (isAlbedoMap)
            {
                diffuzeKoefL = 0.5f;
            }

            var specularKoefL = 2.2f;
            if (isSpecularMap)
            {
                specularKoefL = 0.12f;
            }
            vertexpixel = new Vector3(0, 0, 0);
            //var diffuseColor = new Vector3(12F, 128F, 1F);

            var diffuseColor = new Vector3(30F, 30F, 25F);
            var specularColor = new Vector3(255F, 255F, 255F);

            //var ambientLightColor = ambientKoef;
            //var diffuseColor = new Vector3(30F, 30F, 30F);
            //var specularColor = new Vector3(120F, 120F, 120F);

            Vector3 lightDirection = new Vector3(lightPoint.X, lightPoint.Y, lightPoint.Z) - vertexpixel;
            // lightDirection = new Vector3(lightPoint.X, lightPoint.Y, lightPoint.Z);

            Vector3 L = Vector3.Normalize(new Vector3(lightDirection.X, lightDirection.Y, lightDirection.Z));
            //if (lightPoint.Z < 0)
            //{
            //    L = -L;
            //}
            Vector3 N = Vector3.Normalize(vertexNormal);

            float lambertComponent = (float)diffuzeKoefL * Math.Max(Vector3.Dot(N, L), 0);

            Vector3 diffuseLight = (diffuseKoef / 10f) * diffuseColor * lambertComponent;

            vertexpixel = new Vector3(vertexpixel4.X, vertexpixel4.Y, vertexpixel4.Z);
            vertexpixel =  Vector3.Transform(vertexpixel, matrix);
            //vertexpixel = new Vector3(0, 0, 0);
            Vector3 eyeDirection = new Vector3(eyePoint.X, eyePoint.Y, eyePoint.Z) - vertexpixel;
            Vector3 eyeVector = Vector3.Normalize(eyeDirection);

            Vector3 E = Vector3.Normalize(eyeVector);
            Vector3 R = Vector3.Reflect(-L, N);

            float specular = specularKoefL * (float)Math.Pow(Math.Max(Vector3.Dot(R, E), 0), shiness);
            Vector3 specularLight = specularColor * specular;
            specularLight *= specularKoef / 10F;

            Vector3 sumColor = ambientLightColor;
            sumColor += diffuseLight;
            sumColor += specularLight;

            sumColor = new Vector3(Math.Min(sumColor.X, 255), Math.Min(sumColor.Y, 255), Math.Min(sumColor.Z, 255));
            return sumColor;
        }
        
        private Vector3 LinearInterpolationT(Vector4 a1, Vector4 a2, Vector4 a3, Vector4 b, Vector3 t1, Vector3 t2, Vector3 t3)
        {
            float startX, startZ;
            //float Sb1, b1E;
            float u1, v1;
            t1.X = t1.X / a1.Z;
            t1.Y = t1.Y / a1.Z;
            t1.Z = 1/ a1.Z;
            t2.X = t2.X / a2.Z;
            t2.Y = t2.Y / a2.Z;
            t2.Z = 1/ a2.Z;
            t3.X = t3.X / a3.Z;
            t3.Y = t3.Y / a3.Z;
            t3.Z = 1/ a3.Z;
            if (b.Y >= a2.Y)
            {
                var BAK = (b.Y - a2.Y) / (a1.Y - b.Y);
                startX = (a2.X + a1.X * BAK) / (BAK + 1);
                var edge2 =  getLineLength(a1, a2);
                var Sb1 = getLineLength(a1, new Vector4(startX, b.Y, 0, 0)) / edge2;
                var b1E = getLineLength(a2, new Vector4(startX, b.Y, 0, 0))/ edge2;
                //var b1E = getLineLength(a1, new Vector4(startX, b.Y, 0, 0)) / edge2;
                //var Sb1 = getLineLength(a2, new Vector4(startX, b.Y, 0, 0))/ edge2;
                
                startZ = (a2.Z + a1.Z * BAK) / (BAK + 1);
                //u1 = (t1.X*b1E + t2.X*Sb1) / (t1.Z*b1E + t2.Z*Sb1);
                //v1 = (t1.Y*b1E + t2.Y*Sb1) / (t1.Z*b1E + t2.Z*Sb1);
                u1 = (t1.X*b1E + t2.X*Sb1) * startZ;
                v1 = (t1.Y*b1E + t2.Y*Sb1) * startZ;

            }
            else
            {
                var CBK = (b.Y - a3.Y) / (a2.Y - b.Y);
                startX = (a3.X + a2.X * CBK) / (CBK + 1);
                var edge2 =  getLineLength(a2, a3);
                var Sb1 = getLineLength(a2, new Vector4(startX, b.Y, 0, 0))/ edge2;
                var b1E = getLineLength(a3, new Vector4(startX, b.Y, 0, 0))/ edge2;
                //var b1E = getLineLength(a2, new Vector4(startX, b.Y, 0, 0))/ edge2;
                //var Sb1 = getLineLength(a3, new Vector4(startX, b.Y, 0, 0))/ edge2;
               // u1 = (t2.X*b1E + t3.X*Sb1) / (t2.Z*b1E + t3.Z*Sb1);
                //v1 = (t2.Y*b1E + t3.Y*Sb1) / (t2.Z*b1E + t3.Z*Sb1);
                startZ = (a3.Z + a2.Z * CBK) / (CBK + 1);
                u1 = (t2.X*b1E + t3.X*Sb1) * startZ;
                v1 = (t2.Y*b1E + t3.Y*Sb1) * startZ;

            }
            
            var CAK = (b.Y - a3.Y) / (a1.Y - b.Y);
            var endX = (a3.X + a1.X * CAK) / (CAK + 1);
            var endZ = (a3.Z + a1.Z * CAK) / (CAK + 1);
            
            
            var edge1 = getLineLength(a1, a3);
            var Sb0 = getLineLength(a1, new Vector4(endX, b.Y, 0, 0))/edge1;
            var b0E = getLineLength(a3, new Vector4(endX, b.Y, 0, 0))/edge1;
            //var b0E = getLineLength(a1, new Vector4(endX, b.Y, 0, 0))/edge1;
            //var Sb0 = getLineLength(a3, new Vector4(endX, b.Y, 0, 0))/edge1;
            var u0 = (t1.X*b0E + t3.X*Sb0) / (t1.Z*b0E + t3.Z*Sb0);
            var v0 = (t1.Y*b0E + t3.Y*Sb0) / (t1.Z*b0E + t3.Z*Sb0);

            var edge0 = getLineLength(new Vector4(startX, b.Y, 0, 0), new Vector4(endX, b.Y, 0, 0));
            var Sb = getLineLength(new Vector4(startX, b.Y, 0, 0), b) / edge0;
            var bE = getLineLength(new Vector4(endX, b.Y, 0, 0), b) / edge0;
            //var bE = getLineLength(new Vector4(startX, b.Y, 0, 0), b) / edge0;
            //var Sb = getLineLength(new Vector4(endX, b.Y, 0, 0), b) / edge0;

           /* u0 = u0 / endZ;
            v0 = v0 / endZ;
            u1 = u1 /startZ;
            v1 = v1 / startZ;
            startZ = 1 / startZ;
            endZ = 1 / endZ;
            */
            var u = (u1*bE + u0*Sb) ;/// (startZ*bE + endZ*Sb);
            var v = (v1*bE + v0*Sb) ;/// (startZ*bE + endZ*Sb);
            

            //var koeffs = CountSystemOfEquations(a1, a2, a3, b);

            //var normal = koeffs.X * n1 + koeffs .Y * n2 + koeffs .Z * n3;
            
            //var matrix = FromViewPortCoordinates();
            //var vertexpixel = normal;
            //normal =  Vector3.Transform(normal, matrix);

            return new Vector3(u, v, 0);
        }

        private float getLineLength(Vector4 a1, Vector4 a2)
        {
            return (float) Math.Sqrt((a2.X - a1.X) * (a2.X - a1.X) + (a2.Y - a1.Y) * (a2.Y - a1.Y));
        }


        private Vector3 LinearInterpolation(Vector4 a1, Vector4 a2, Vector4 a3, Vector4 b, Vector3 n1, Vector3 n2, Vector3 n3)
        {

            
            var koeffs = CountSystemOfEquations(a1, a2, a3, b);

            var normal = koeffs.X * n1 + koeffs .Y * n2 + koeffs .Z * n3;
            
            //var matrix = FromViewPortCoordinates();
            //var vertexpixel = normal;
            //normal =  Vector3.Transform(normal, matrix);

            return normal;
        }
        
        private Vector3 LinearInterpolationD(Vector4 a1, Vector4 a2, Vector4 a3, Vector4 b, Vector3 n1, Vector3 n2, Vector3 n3)
        {

            
            var koeffs = CountSystemOfEquations(a1, a2, a3, b);

            var normal = n1/koeffs.X + n2/koeffs .Y + n3/koeffs .Z;
            
            //var matrix = FromViewPortCoordinates();
            //var vertexpixel = normal;
            //normal =  Vector3.Transform(normal, matrix);

            return normal;
        }

        private Vector3 CountSystemOfEquations(Vector4 a14, Vector4 a24, Vector4 a34, Vector4 b4)
        {
            
            var matrix = FromViewPortCoordinates();
            var a1=  Vector3.Transform(new Vector3(a14.X, a14.Y, a14.Z), matrix);
            var a2=  Vector3.Transform(new Vector3(a24.X, a24.Y, a24.Z), matrix);
            var a3=  Vector3.Transform(new Vector3(a34.X, a34.Y, a34.Z), matrix);
            var b=  Vector3.Transform(new Vector3(b4.X, b4.Y, b4.Z), matrix);

            var nKoefMatrix = new Matrix4x4(a1.X, a2.X, a3.X, 0,
                a1.Y, a2.Y, a3.Y, 0,
                a1.Z, a2.Z, a3.Z, 0,
                0, 0, 0, 1);
            Matrix4x4.Invert(nKoefMatrix, out var koefMatrix);
            var resultVector = new Vector3(
                koefMatrix.M11 * b.X + koefMatrix.M12 * b.Y + koefMatrix.M13 * b.Z,
                koefMatrix.M21 * b.X + koefMatrix.M22 * b.Y + koefMatrix.M23 * b.Z,
                koefMatrix.M31 * b.X + koefMatrix.M32 * b.Y + koefMatrix.M33 * b.Z);

            return resultVector;

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
            Transform(false);
            Transform();
            neadDrow = true;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            Transform(false);
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
                    Transform(false);
                    Transform();
                    neadDrow = true;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Focused)
            {
                if (Keyboard.IsKeyDown(Keys.W))
                {
                    xRotation -= lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.S))
                {
                    xRotation += lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.D))
                {
                    yRotation -= aDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.A))
                {
                    yRotation += aDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.Z))
                {
                    viewPoint.Z += delta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.X))
                {
                    if (viewPoint.Z - delta < 0)
                        return;

                    viewPoint.Z -= delta;
                    neadDrow = true;
                }

                ;

                if (Keyboard.IsKeyDown(Keys.U))
                {
                    origlightPoint.Y -= lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.J))
                {
                    origlightPoint.Y += lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.H))
                {
                    origlightPoint.X -= lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.K))
                {
                    origlightPoint.X += lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.N))
                {
                    origlightPoint.Z += lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.M))
                {
                    if (origlightPoint.Z - lDelta < 0)
                        return;

                    origlightPoint.Z -= lDelta;
                    neadDrow = true;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.O))
                {

                    lDelta += 0.02f;
                }

                ;
                if (Keyboard.IsKeyDown(Keys.L))
                {
                    lDelta -= 0.02f;
                }

                ;
                LskippedPixelsDraw.Text = "d=" + lDelta;
                //LskippedPixelsDraw.Text +=" x="+ origlightPoint.X + " y="+ origlightPoint.Y + " z="+ origlightPoint.Z;
            }

            if (neadDrow)
            {
                Transform(false);
                Transform();
                Draw();
                neadDrow = false;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {

        }
        


    }
}
