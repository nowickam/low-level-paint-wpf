using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    abstract class Shape
    {
        public List<int> points;

        public int thickness;
        public List<int> color;

        public int stride;

        public Shape(List<int> _points, int _thickness, List<int> _color, int _stride)
        {
            points = _points;

            thickness = _thickness;

            stride = _stride;

            color = new List<int>();
            for(int i = 0; i < _color.Count; i++)
            {
                color.Add(_color[i]);
            }

        }

    }
}
