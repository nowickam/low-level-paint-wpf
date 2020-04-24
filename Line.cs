using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Line : Shape
    {
        public Line(List<int> _points, int _thickness, List<int> _color, int _stride, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            DrawShape(ref pixels);   
        }

        protected override void DrawShape(ref byte[] pixels)
        {
            lineDDA(points[0], points[1], points[2], points[3], ref pixels);
        }

        private void lineDDA(int x1, int y1, int x2, int y2, ref byte[] pixels)
        {
            float dy = y2 - y1;
            float dx = x2 - x1;
            float step;
            if (Math.Abs(dx) >= Math.Abs(dy))
                step = Math.Abs(dx);
            else
                step = Math.Abs(dy);
            dx = dx / step;
            dy = dy / step;
            float y = y1;
            float x = x1;
            int i = 0;
            while (i < step)
            {
                //int location = (int)Math.Round(y) * stride + (int)Math.Round(x) * 3;
                //pixels[location] = (byte)color[0];
                //pixels[location + 1] = (byte)color[1];
                //pixels[location + 2] = (byte)color[2];
                SetPixel((int)Math.Round(x), (int)Math.Round(y), ref pixels);
                y += dy;
                x += dx;
                i++;
            }
        }
    }
}
