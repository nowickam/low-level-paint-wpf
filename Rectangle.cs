using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public class Rectangle : Polygon
    {
        private List<int> actualPoints;
        public Rectangle(List<int> _points, int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels) : base(_thickness, _color, _stride, _aliasing, ref pixels)
        {
            // remake the pixels
            if (_points.Count == 4)
                actualPoints = RectFromDiagonal(_points);
            else
                actualPoints = new List<int>(_points);
            base.points = new List<int>(actualPoints);
            base.DrawShape(ref pixels);
        }

        public List<int> RectFromDiagonal(List<int> points)
        {
            List<int> newPoints = new List<int>();

            int dx = points[2] - points[0];

            newPoints.Add(points[0]);
            newPoints.Add(points[1]);

            newPoints.Add(points[0]+dx);
            newPoints.Add(points[1]);

            newPoints.Add(points[2]);
            newPoints.Add(points[3]);

            newPoints.Add(points[2]-dx);
            newPoints.Add(points[3]);

            return newPoints;
        }

        public new List<int> PrepareNewVertices(int oldX, int oldY, int newX, int newY)
        {
            Console.WriteLine("RECTANGLE");
            int diagX1 = 0, diagX2 = 0, diagY1 = 0, diagY2 = 0;
            for (int i = 0; i < points.Count - 1; i += 2)
            {
                if (points[i] == oldX && points[i + 1] == oldY)
                {
                    points[i] = newX;
                    points[i + 1] = newY;

                    diagX1 = i;
                    diagY1 = i + 1;

                    diagX2 = i - 4 < 0 ? i + 4 : i - 4;
                    diagY2 = i - 3 < 0 ? i + 5 : i - 3;

                    break;
                }
            }
            List<int> newPoints = RectFromDiagonal(new List<int>() { points[diagX1], points[diagY1], points[diagX2], points[diagY2] });
            points = new List<int>(newPoints);

            return points;
        }
    }
}
