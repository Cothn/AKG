using _3DModelViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3DModelViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Vector3> vertices;
        private List<int[]> poligons;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ObjParser parser = new ObjParser();
            (vertices, poligons) = parser.Parse(@"D:\Study\University\7 term\AKG\АКГ\Head\Model.obj");

            var matrix = new Matrix4x4(2, 0, 0, 0,
                                       0, 2, 0, 0,
                                       0, 0, 2, 0,
                                       0, 0, 0, 2);

            var degrees = 180;
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(1, 0, 0, 0,
                                             0, cos, -sin, 0,
                                             0, sin, cos, 0,
                                             0, 0, 0, 1);

            //TransformVectors(matrix);
            TransformVectors(rotateMatrix);
            Draw();

        }

        public void TransformVectors(Matrix4x4 transformMatrix)
        {
            for(int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = Vector3.TransformNormal(vertices[i], transformMatrix);
            }
        }

        public void ScaleVectors(float x)
        {
            var scaleMatrix = new Matrix4x4(x, 0, 0, 0,
                                            0, x, 0, 0,
                                            0, 0, x, 0,
                                            0, 0, 0, 1);

            TransformVectors(scaleMatrix);
        }

        public void MoveVectors(Vector3 vector)
        {
            var translationMatrix = new Matrix4x4(1, 0, 0, vector.X,
                                             0, 1, 0, vector.Y,
                                             0, 0, 1, vector.Z,
                                             0, 0, 0,         1);

            TransformVectors(translationMatrix);
        }

        public void RotateXVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(1,   0,    0, 0,
                                             0, cos, -sin, 0,
                                             0, sin,  cos, 0,
                                             0,   0,    0, 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateYVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4( cos, 0, sin, 0,
                                               0 , 1,  0 , 0,
                                             -sin, 0, cos, 0,
                                               0 , 0,  0 , 1);

            TransformVectors(rotateMatrix);
        }

        public void RotateZVectors(float degrees)
        {
            double angle = Math.PI * degrees / 180.0;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var rotateMatrix = new Matrix4x4(cos, -sin, 0, 0,
                                             sin,  cos, 0, 0,
                                              0 ,  0  , 1, 0,
                                              0 ,  0  , 0, 1);

            TransformVectors(rotateMatrix);
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {

        }

        public void Draw()
        {
            var width = DrawField.ActualWidth / 2;
            var height = DrawField.ActualHeight / 2;

            foreach (var poligon in poligons)
            {
                for (int i = 0; i < poligon.Length; i++)
                {
                    var k = poligon[i] - 1;
                    var j = poligon[(i + 1) % poligon.Length] - 1;

                    Line line = new Line()
                    {
                        Stroke = Brushes.Black,
                        X1 = (vertices[k].X + 1) * width,
                        X2 = (vertices[j].X + 1) * width,
                        Y1 = (vertices[k].Y + 1) * height,
                        Y2 = (vertices[j].Y + 1) * height
                    };

                    DrawField.Children.Add(line);
                }
            }
        }

        private void IncreaseScaleButton_Click(object sender, RoutedEventArgs e)
        {
            ScaleVectors(1.01f);
            DrawField.Children.Clear();
            Draw();
        }

        private void DecreaseScaleButton_Click(object sender, RoutedEventArgs e)
        {
            ScaleVectors(0.99f);
            DrawField.Children.Clear();
            Draw();
        }

        private void RotateYLeftButtton_Click(object sender, RoutedEventArgs e)
        {
            RotateYVectors(-1);
            DrawField.Children.Clear();
            Draw();
        }

        private void RotateYRightButtton_Click(object sender, RoutedEventArgs e)
        {
            RotateYVectors(1);
            DrawField.Children.Clear();
            Draw();
        }
    }
}
