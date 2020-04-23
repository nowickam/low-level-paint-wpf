using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Circle :Shape
    {
        int R;
        int cX;
        int cY;
        public Circle(List<int> _points, int _thickness, List<int> _color, int _stride, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            int dx = Math.Abs(points[0]) - Math.Abs(points[2]);
            int dy = Math.Abs(points[1]) - Math.Abs(points[3]);
            R = (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            cX = points[0];
            cY = points[1];
            midpointCircle(R, ref pixels); ;
        }

        void setColor(int x, int y, ref byte[] pixels)
        {
            if (cX + x >= 0 && cX + x < stride/3 && cY + y >= 0 && cY + y < (pixels.Length / stride))
            {
                int location = (cX + x) * 3 + (cY + y) * stride;
                pixels[location] = (byte)color[0];
                pixels[location + 1] = (byte)color[1];
                pixels[location + 2] = (byte)color[2];
            }
        }

        void midpointCircle(int R, ref byte[] pixels)
        {
            int d = 1 - R;
            int x = 0;
            int y = R;
            do
            {
                if (d < 0) //move to E
                    d += 2 * x + 3;
                else //move to SE
                {
                    d += 2 * x - 2 * y + 5;
                    y--;
                }
                x++;
                setColor(x, y, ref pixels);
                setColor(-x, y, ref pixels);
                setColor(x, -y, ref pixels);
                setColor(-x, -y, ref pixels);
                setColor(y, x, ref pixels);
                setColor(-y, x, ref pixels);
                setColor(y, -x, ref pixels);
                setColor(-y, -x, ref pixels);
            } while (y > x);
        }
    }
}
