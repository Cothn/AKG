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
        public static bool ArePointsCrossing(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return ArePointsCrossing(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        public static bool ArePointsCrossing(Vector3 p1, Vector3 p2, Vector2 p3, Vector2 p4)
        {
            return ArePointsCrossing(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        public static bool ArePointsCrossing(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)//проверка пересечения
        {
            return ArePointsCrossing(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        public static bool ArePointsCrossing(float p1X, float p1Y, float p2X, float p2Y, float p3X, float p3Y,
            float p4X, float p4Y)
        {
            float v1 = VectorMultiplication(p4X - p3X, p4Y - p3Y, p1X - p3X, p1Y - p3Y);
            float v2 = VectorMultiplication(p4X - p3X, p4Y - p3Y, p2X - p3X, p2Y - p3Y);
            float v3 = VectorMultiplication(p2X - p1X, p2Y - p1Y, p3X - p1X, p3Y - p1Y);
            float v4 = VectorMultiplication(p2X - p1X, p2Y - p1Y, p4X - p1X, p4Y - p1Y);
            if ((v1 * v2) < 0 && (v3 * v4) < 0)
                return true;
            return false;
        }

        public static Vector2 CrossingPoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            float a1, b1, c1, a2, b2, c2;

            (a1, b1, c1) = LineEquation(p1, p2);
            (a2, b2, c2) = LineEquation(p3, p4);

            return CrossingPoint(a1, b1, c1, a2, b2, c2);
        }

        public static Vector2 CrossingPoint(Vector3 p1, Vector3 p2, Vector2 p3, Vector2 p4)
        {
            float a1, b1, c1, a2, b2, c2;

            (a1, b1, c1) = LineEquation(p1, p2);
            (a2, b2, c2) = LineEquation(p3, p4);

            return CrossingPoint(a1, b1, c1, a2, b2, c2);
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
        public static (float A, float B, float C) LineEquation(Vector3 p1, Vector3 p2)
        {
            return LineEquation(p1.X, p1.Y, p2.X, p2.Y);
        }

        //построение уравнения прямой
        public static (float A, float B, float C) LineEquation(Vector2 p1, Vector2 p2)
        {
            return LineEquation(p1.X, p1.Y, p2.X, p2.Y);
        }

        //построение уравнения прямой
        public static (float A, float B, float C) LineEquation(float p1X, float p1Y, float p2X, float p2Y)
        {
            var A = p2Y - p1Y;
            var B = p1X - p2X;
            var C = -p1X * (p2Y - p1Y) + p1Y * (p2X - p1X);

            return (A, B, C);
        }

        private static float VectorMultiplication(float ax, float ay, float bx, float by) //векторное произведение
        {
            return ax * by - bx * ay;
        }
    }
}
