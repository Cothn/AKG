using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DModelViewer
{
    internal static class PointsCrossing
    {
        public static bool ArePointsCrossing(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)//проверка пересечения
        {
            float v1 = VectorMultiplication(p4.X - p3.X, p4.Y - p3.Y, p1.X - p3.X, p1.Y - p3.Y);
            float v2 = VectorMultiplication(p4.X - p3.X, p4.Y - p3.Y, p2.X - p3.X, p2.Y - p3.Y);
            float v3 = VectorMultiplication(p2.X - p1.X, p2.Y - p1.Y, p3.X - p1.X, p3.Y - p1.Y);
            float v4 = VectorMultiplication(p2.X - p1.X, p2.Y - p1.Y, p4.X - p1.X, p4.Y - p1.Y);
            if ((v1 * v2) < 0 && (v3 * v4) < 0)
                return true;
            return false;
        }

        public static Vector2 CrossingPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float a1, b1, c1, a2, b2, c2;

            (a1, b1, c1) = LineEquation(p1, p2);
            (a2, b2, c2) = LineEquation(p3, p4);

            return CrossingPoint(a1, b1, c1, a2, b2, c2);
        }

        //поиск точки пересечения
        public static Vector2 CrossingPoint(float a1, float b1, float c1, float a2, float b2, float c2)
        {
            Vector2 pt = new Vector2();
            float d = a1 * b2 - b1 * a2;
            float dx = -c1 * b2 + b1 * c2;
            float dy = -a1 * c2 + c1 * a2;
            pt.X = dx / d;
            pt.Y = dy / d;
            return pt;
        }

        //построение уравнения прямой
        public static (float A, float B, float C) LineEquation(Vector2 p1, Vector2 p2)
        {
            var A = p2.Y - p1.Y;
            var B = p1.X - p2.X;
            var C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);

            return (A, B, C);
        }

        private static float VectorMultiplication(float ax, float ay, float bx, float by) //векторное произведение
        {
            return ax * by - bx * ay;
        }
    }
}
