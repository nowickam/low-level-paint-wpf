using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Circle : Shape
    {
        private int r;
        private int cX;
        private int cY;

        public int CX
        {
            get { return cX; }
            set { cX = value; }
        }

        public int CY
        {
            get { return cY; }
            set { cY = value; }
        }

        public int R
        {
            get { return r; }
            set { r = value; }
        }
        public Circle(List<int> _points, int _thickness, List<int> _color, int _stride, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            int dx = Math.Abs(points[0]) - Math.Abs(points[2]);
            int dy = Math.Abs(points[1]) - Math.Abs(points[3]);
            r = (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            cX = points[0];
            cY = points[1];
            DrawShape(ref pixels);
        }

        public override void Edit(List<int> newPoints, int? newThickness, List<int> newColor)
        {
            points = newPoints == null ? points : newPoints;
            thickness = newThickness == null ? thickness : (int)newThickness;
            if (newThickness != null) brush = new Brush(thickness);
            color = newColor == null ? color : new List<int>(newColor);

            int dx = Math.Abs(points[0]) - Math.Abs(points[2]);
            int dy = Math.Abs(points[1]) - Math.Abs(points[3]);
            r = (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            cX = points[0];
            cY = points[1];
        }

        public override Shape CheckClick(int x, int y)
        {
            int dx = cX - x;
            int dy = cY - y;
            if (Math.Sqrt(dx * dx + dy * dy) >= r - 5 && Math.Sqrt(dx * dx + dy * dy) <= r + 5)
            {
                editMode = true;
                return this;
            }
            return null;         
        }

        public override void DrawShape(ref byte[] pixels)
        {
            MidpointCircle(r, ref pixels);
        }

        private void SetColor(int x, int y, ref byte[] pixels)
        {
            if (cX + x - thickness + 1  >= 0 && cX + x + thickness - 1 < stride/3 && cY + y - thickness + 1 >= 0 && cY + y + thickness - 1 < (pixels.Length / stride))
            {
                SetPixel(cX + x, cY + y, ref pixels);
            }
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

        void MidpointCircle(int R, ref byte[] pixels)
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

        }


    }
}
