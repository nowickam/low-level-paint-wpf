using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Capsule : Shape
    {
        private int r;
        private int cX1, cX2, cY1, cY2;
        public double dX1, dY1, dX2, dY2;
        public double eX1, eY1, eX2, eY2;

        public int R
        {
            get { return r; }
            set { r = value; }
        }
        public Capsule(List<int> _points, int _thickness, List<int> _color, int _stride, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            cX1 = points[0];
            cY1 = points[1];
            cX2 = points[2];
            cY2 = points[3];
            double dx = (points[4]) - (points[2]);
            double dy = (points[5]) - (points[3]);
            r = (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            dx = cX2 - cX1;
            dy = cY2 - cY1;
            double l = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            double temp = dx;
            dx =  dy / l * r;
            dy = -temp / l * r;

            dX1 = cX1 + dx;
            dY1 = cY1 + dy;
            dX2 = cX2 + dx;
            dY2 = cY2 + dy;

            eX1 = cX1 - dx;
            eY1 = cY1 - dy;
            eX2 = cX2 - dx;
            eY2 = cY2 - dy;

            DrawShape(ref pixels);
        }

        public override void Edit(List<int> newPoints, int? newThickness, List<int> newColor)
        {
            
        }

        private void SetColor(int x, int y, ref byte[] pixels)
        {
            if (Math.Sign((dX1 - cX1) * ( y ) - (dY1 - cY1) * (x)) <= 0)
                SetPixel(cX1 + x, cY1 + y, ref pixels);
            if (Math.Sign((dX2 - cX2) * (y) - (dY2 - cY2) * (x)) >= 0)
                SetPixel(cX2 + x, cY2 + y, ref pixels);
        }

        private void SetPixelAllOctan(int x, int y, ref byte[] pixels)
        {
            SetColor(x, y, ref pixels);
            SetColor(-x, y, ref pixels);
            SetColor(x, -y, ref pixels);
            SetColor(-x, -y, ref pixels);
            SetColor(y, x, ref pixels);
            SetColor(-y, x, ref pixels);
            SetColor(y, -x, ref pixels);
            SetColor(-y, -x, ref pixels);
        }

        public override void DrawShape(ref byte[] pixels)
        {
            int d = 1 - R;
            int x = 0;
            int y = R;
            SetPixelAllOctan(x, y, ref pixels);
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
                SetPixelAllOctan(x, y, ref pixels);
            } while (y > x);
            
            Line l1 = new Line(new List<int> { (int)dX1, (int)dY1, (int)dX2, (int)dY2 }, thickness, color, stride, false, ref pixels);
            Line l2 = new Line(new List<int> { (int)eX1, (int)eY1, (int)eX2, (int)eY2 }, thickness, color, stride, false, ref pixels);

        }

        public override Shape CheckClick(int x, int y)
        {
            return null;
        }
    }
}
