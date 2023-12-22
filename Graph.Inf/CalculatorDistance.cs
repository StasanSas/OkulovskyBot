using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Inf
{
    public static class CalculatorDistance
    {
        public static double GetDistance(int x1, int y1, int x2, int y2)
        {
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            return distance;
        }

        public static bool IntersectsCircleWithCircle(int xCenter1, int yCenter1, int radius1,
                                               int xCenter2, int yCenter2, int radius2)
        {
            double distance = Math.Sqrt(Math.Pow((xCenter2 - xCenter1), 2) + Math.Pow((yCenter2 - yCenter1), 2));
            return distance <= (radius1 + radius2);
        }

        public static bool IsCircleIntersectSegment(double centerCirculx, double centerCircuy, double radius,
            double x1, double y1, double x2, double y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var b = y1 - (m * x1);

            var A = m; var B = -1; var C = b; var x0 = centerCirculx; var y0 = centerCircuy;

            var distance = Math.Abs(A * x0 + B * y0 + C) / Math.Sqrt(A * A + B * B);

            Console.WriteLine(distance);
            return distance <= radius;
        }

        public static bool IsSegmentIntersectSegment(double x1, double y1, double x2, double y2,
                                                     double x3, double y3, double x4, double y4)
        {
            double a1 = y2 - y1;
            double b1 = x1 - x2;
            double c1 = a1 * x1 + b1 * y1;

            double a2 = y4 - y3;
            double b2 = x3 - x4;
            double c2 = a2 * x3 + b2 * y3;

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                return false; // Отрезки параллельны
            }
            else
            {
                double intersectionX = (b2 * c1 - b1 * c2) / determinant;
                double intersectionY = (a1 * c2 - a2 * c1) / determinant;

                bool onSegment1 = IsPointOnSegment(intersectionX, intersectionY, x1, y1, x2, y2);
                bool onSegment2 = IsPointOnSegment(intersectionX, intersectionY, x3, y3, x4, y4);

                return onSegment1 && onSegment2;
            }
        }

        public static bool IsPointOnSegment(double px, double py, double x1, double y1, double x2, double y2)
        {
            bool betweenX = (px >= x1 && px <= x2) || (px >= x2 && px <= x1);
            bool betweenY = (py >= y1 && py <= y2) || (py >= y2 && py <= y1);
            return betweenX && betweenY;
        }
    }
}
