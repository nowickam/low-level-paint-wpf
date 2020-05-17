using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Clipping : Rectangle
    {
        List<Line> lines;
        List<Polygon> polygons;
        List<Shape> shapes;
        bool aliasing;

        public Clipping(List<int> _points, int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels, ref List<Shape> _shapes) : base(_points, _thickness, _color, _stride, _aliasing, ref pixels)
        {
            lines = new List<Line>();
            aliasing = _aliasing;

            polygons = new List<Polygon>();
            shapes = _shapes;

            DrawShape(ref pixels);
        }
        private void PolygonsFromShapes(ref List<Shape> shapes)
        {
            polygons.Clear();
            foreach (Shape s in shapes)
            {
                if (s is Polygon && !(s is Clipping))
                    polygons.Add(s as Polygon);
            }
        }

        //from lecture
        bool Clip(float denom, float numer, ref float tE, ref float tL)
        {
            if (denom == 0)
            { //Paralel line
                if (numer > 0)
                    return false; // outside - discard
                return true; //skip to next edge
            }
            float t = numer / denom;
            if (denom > 0)
            { //PE
                if (t > tL) //tE > tL - discard
                    return false;
                if (t > tE)
                    tE = t;
            }
            else
            { //denom < 0 - PL
                if (t < tE) //tL < tE - discard
                    return false;
            if (t < tL)
                    tL = t;
            }
            return true;
        }

        private void LiangBarsky(int p1x, int p1y, int p2x, int p2y, ref byte[] pixels)
        {
            float dx = p2x - p1x, dy = p2y - p1y;
            float tE = 0, tL = 1;

            float left = Math.Min(points[0], Math.Min(points[2],points[4]));
            float right = Math.Max(points[0], Math.Max(points[2], points[4]));

            float top = Math.Min(points[1], Math.Min(points[3], points[5]));
            float bottom = Math.Max(points[1], Math.Max(points[3], points[5]));

            if (Clip(-dx, p1x - right, ref tE, ref tL))
                if (Clip(dx, left - p1x, ref tE, ref tL))
                    if (Clip(-dy, p1y - bottom, ref tE, ref tL))
                        if (Clip(dy, top - p1y, ref tE, ref tL))
                        {
                            if (tL < 1) { 
                                p2x = (int)(p1x + dx * tL); 
                                p2y = (int)(p1y + dy * tL); 
                            }
                            if (tE > 0) { 
                                p1x += (int)(dx * tE);
                                p1y += (int)(dy * tE); 
                            }
                            lines.Add(new Line(new List<int>() { p1x, p1y, p2x, p2y }, thickness + 2, new List<int>() { 255, 0, 0 }, stride, aliasing, ref pixels));
                        }
        }

        public override void DrawShape(ref byte[] pixels)
        {
            lines.Clear();
            base.DrawShape(ref pixels);
            PolygonsFromShapes(ref shapes);
            ClipPolygons(ref pixels);
            foreach (Line l in lines)
            {
                l.DrawShape(ref pixels);
            }
        }


        private void ClipPolygons(ref byte[] pixels)
        {
            foreach(Polygon p in polygons)
            {
                foreach(Line e in p.Edges)
                {
                    LiangBarsky(e.Points[0], e.Points[1], e.Points[2], e.Points[3], ref pixels);
                }
            }
        }

    }
}
