using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public class Brush
    {
        private int thickness;
        private int stride;
        private byte[] pattern;

        public Brush(int _thickness)
        {
            thickness = _thickness;
            stride = 3 * thickness;
            CreateBrush();
        }

        private void CreateBrush()
        {
            pattern = new byte[stride * thickness];
            if (thickness == 1)
            {
                pattern[0] = 1;
            }
            else
            {
                List<int> tempPoints = new List<int> { thickness / 2, thickness / 2, thickness - 1, thickness / 2 };
                List<int> color = new List<int> { 1, 1, 1 };
                new Circle(tempPoints, 1, color, stride, ref pattern);
            }
        }

        public int Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public byte[] Pattern
        {
            get { return pattern; }
        }

        public int Stride
        {
            get { return stride; }
        }
    }
}
