using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Polygon : Shape
    {
        private bool aliasing;
        private List<Line> edges;
        public Polygon(List<int> _points, int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            aliasing = _aliasing;
            DrawShape(ref pixels);
        }

        public override Shape CheckClick(int x, int y)
        {
            
            foreach(Line e in edges)
            {
                Shape check = e.CheckClick(x, y);
                
                if (check != null)
                {
                    check.EditMode = false;
                    editMode = true;                   
                    return this;
                }
            }
            return null;
        }

        public List<int> PrepareNewVertices(int oldX, int oldY, int newX, int newY)
        {
            for(int i = 0; i < points.Count - 1; i+=2)
            {
                if(points[i]==oldX && points[i + 1] == oldY)
                {
                    points[i] = newX;
                    points[i + 1] = newY;
                    break;
                }
            }
            return points;
        }

        public List<int> CheckNeighborClick(int x0, int y0, int x1, int y1)
        {
            List<int> result = new List<int>();
            for(int i=0;i<points.Count-1; i+=2)
            {
                if(points[i] >= x0 - 5 && points[i] <= x0 + 5 && points[i+1] >= y0 - 5 && points[i+1] <= y0 + 5)
                {
                    result.Add(points[i]);
                    result.Add(points[i + 1]);

                    int nextX = (i + 2) % points.Count;
                    int nextY = (i + 3) % points.Count;
                    int prevX = i - 2 < 0 ? points.Count  - 2 : i - 2;
                    int prevY = i - 1 < 0 ? points.Count - 1 : i - 1;
                    // check first neighbor
                    if (points[nextX]-5 <= x1 && points[nextX]+5 >= x1 &&
                        points[nextY]-5 <= y1  && points[nextY]+5 >= y1 )
                    {
                        result.Add(points[nextX]);
                        result.Add(points[nextY]);
                        return result;
                    }
                    //check second neighbor
                    else if (points[prevX]-5 <= x1 && points[prevX]+5 >= x1  &&
                            points[prevY]-5 <= y1 && points[prevY]+5 >= y1 )
                    {
                        result.Add(points[prevX]);
                        result.Add(points[prevY]);
                        return result;
                    }
                    //no neighbor matches to this click
                    else
                        return result;
                }
            }
            return null;
        }


        public override void DrawShape(ref byte[] pixels)
        {
            edges = new List<Line>();
            List<int> tempPoints = new List<int>();
            for(int i = 0; i < points.Count / 2; i++)
            {
                tempPoints.Add(points[2 * i]);
                tempPoints.Add(points[2 * i + 1]);
                tempPoints.Add(points[(2 * i + 2) % points.Count]);
                tempPoints.Add(points[(2 * i + 3) % points.Count]);
                edges.Add(new Line(tempPoints, thickness, color, stride, aliasing, ref pixels));
                tempPoints.Clear();
            }
        }

        public override void Edit(List<int> newPoints, int? newThickness, List<int> newColor)
        {
            points = newPoints == null ? points : newPoints;
            thickness = newThickness == null ? thickness : (int)newThickness;
            if (newThickness != null) brush = new Brush(thickness);
            color = newColor == null ? color : new List<int>(newColor);
        }

    }
}
