using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Circle : Shape
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
            DrawShape(ref pixels);
        }

        protected override void DrawShape(ref byte[] pixels)
        {
            MidpointCircle(R, ref pixels);
        }

        private void SetColor(int x, int y, ref byte[] pixels)
        {
            if (cX + x - thickness + 1  >= 0 && cX + x + thickness - 1 < stride/3 && cY + y - thickness + 1 >= 0 && cY + y + thickness - 1 < (pixels.Length / stride))
            {
                SetPixel(cX + x, cY + y, ref pixels);
            }
        }

        void MidpointCircle(int R, ref byte[] pixels)
        {
            int d = 1 - R;
            int x = 0;
            int y = R;
            SetColor(x, y, ref pixels);
            SetColor(-x, y, ref pixels);
            SetColor(x, -y, ref pixels);
            SetColor(-x, -y, ref pixels);
            SetColor(y, x, ref pixels);
            SetColor(-y, x, ref pixels);
            SetColor(y, -x, ref pixels);
            SetColor(-y, -x, ref pixels);
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
                SetColor(x, y, ref pixels);
                SetColor(-x, y, ref pixels);
                SetColor(x, -y, ref pixels);
                SetColor(-x, -y, ref pixels);
                SetColor(y, x, ref pixels);
                SetColor(-y, x, ref pixels);
                SetColor(y, -x, ref pixels);
                SetColor(-y, -x, ref pixels);
            } while (y > x);

        }
    }
}
