using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Polygon : Shape
    {
        private List<Line> edges;
        public Polygon(List<int> _points, int _thickness, List<int> _color, int _stride, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            drawPolygon(points, ref pixels);
        }

        private void drawPolygon(List<int> points, ref byte[] pixels)
        {
            edges = new List<Line>();
            List<int> tempPoints = new List<int>();
            for(int i = 0; i < points.Count / 2; i++)
            {
                tempPoints.Add(points[2 * i]);
                tempPoints.Add(points[2 * i + 1]);
                tempPoints.Add(points[(2 * i + 2) % points.Count]);
                tempPoints.Add(points[(2 * i + 3) % points.Count]);
                edges.Add(new Line(tempPoints, 1, color, stride, ref pixels));
                tempPoints.Clear();
            }
        }
    }
}
