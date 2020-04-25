using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    class Line : Shape
    {
        bool aliasing;
        public Line(List<int> _points, int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            aliasing = _aliasing;
            DrawShape(ref pixels);   
        }

        public override void Edit(List<int> newPoints, int? newThickness, List<int> newColor)
        {
            points = newPoints == null ? points : newPoints;
            thickness = newThickness == null ? thickness : (int)newThickness;
            if (newThickness != null) brush = new Brush(thickness);
            color = newColor == null ? color : new List<int>(newColor);
        }

        public void changeAlias(bool ifAlias)
        {
            aliasing = ifAlias;
        }

        public override Shape CheckClick(int x, int y)
        {
            if(editMode == true)
            {
                editMode = false;
                return this;
            }
            else if(points[0]>=x-5 && points[0] <= x+5 && points[1] >= y-5 && points[1] <= y+5)
            {
                editMode = true;
                return this;
            }
            else if (points[2] >= x - 5 && points[2] <= x + 5 && points[3] >= y - 5 && points[3] <= y + 5)
            {
                editMode = true;
                return this;
            }
            return null;
        }

        public override void DrawShape(ref byte[] pixels)
        {
            if (aliasing)
                GuptaSproull(ref pixels);
            else
                LineDDA(ref pixels);
        }

        private void LineDDA(ref byte[] pixels)
        {
            float dy = points[3]-points[1];
            float dx = points[2]-points[0];
            float step;

            if (Math.Abs(dx) >= Math.Abs(dy))
                step = Math.Abs(dx);
            else
                step = Math.Abs(dy);


            dx = dx / step;
            dy = dy / step;
            float y = points[1];
            float x = points[0];
            int i = 0;


            while (i < step)
            {
                if (x - thickness + 1 >= 0 && x + thickness - 1 < stride / 3 && y - thickness + 1 >= 0 && y + thickness - 1 < (pixels.Length / stride))
                {
                    SetPixel((int)Math.Round(x), (int)Math.Round(y), ref pixels);
                }
                y += dy;
                x += dx;
                i++;

            }
        }

        private static float Cov(float d, float r)
        {
            if (d <= r)
            {
                float term1 = (float)((1 / Math.PI) * (Math.Acos(1.0 * d / r)));
                float term2 = (float)((d / Math.PI * r * r) * Math.Sqrt(r * r - d * d));
                return term1 - term2;
            }
            else
                return 0;
        }

        private static float Coverage(float thickness, float distance, float r)
        {
            if (thickness / 2 <= distance)
                return Cov(distance - thickness / 2, r);
            else
                return 1 - Cov(thickness / 2 - distance, r);
        }

        List<int> getColorCoverage(List<int> bg, List<int> fg, float cov)
        {
            List<int> color = new List<int>();
            color.Add((int)((1 - cov) * bg[0] + cov * fg[0]));
            color.Add((int)((1 - cov) * bg[1] + cov * fg[1]));
            color.Add((int)((1 - cov) * bg[2] + cov * fg[2]));
            return color;
        }

        private float IntensifyPixel(int x, int y, int thickness, float distance, ref byte[] pixels)
        {
            float r = 0.5f;
            float cov = Coverage(thickness, distance, r);
            if (cov > 0)
            {
                if (x - thickness + 1 >= 0 && x + thickness - 1 < stride / 3 && y - thickness + 1 >= 0 && y + thickness - 1 < (pixels.Length / stride))
                {
                    int location = y * stride + x * 3;
                    List<int> aliasColor = getColorCoverage(new List<int> { 255, 255, 255 }, color, cov);
                    pixels[location] = (byte)aliasColor[0];
                    pixels[location + 1] = (byte)aliasColor[1];
                    pixels[location + 2] = (byte)aliasColor[2];
                }
            }
            return cov;
        }

        private void GuptaSproull(ref byte[] pixels)
        {
            int x0 = points[0], y0 = points[1], x1 = points[2], y1 = points[3];

            if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
            {
                if (x0 > x1)
                    plotLineLow(x1, y1, x0, y0, ref pixels);
                else
                    plotLineLow(x0, y0, x1, y1, ref pixels);
            }
            else
            {
                if (y0 > y1)
                    plotLineHigh(x1, y1, x0, y0, ref pixels);
                else
                    plotLineHigh(x0, y0, x1, y1, ref pixels);
            }
        }

        private void plotLineLow(int x0, int y0, int x1, int y1, ref byte[] pixels)
        {
            int dx = x1 - x0;
            int dy = y1 - y0;
            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }
            int d = 2 * dy - dx;
            int y = y0;
            int x = x0;

            int twoVDx = 0;
            float invDenom = (float)(1 / (2 * Math.Sqrt(dx * dx + dy + dy)));
            float twoDxInvDenom = 2 * dx * invDenom;

            while (x < x1)
            {
                IntensifyPixel(x, y, thickness, twoVDx * invDenom, ref pixels);
                for (int i = 1; IntensifyPixel(x, y + i, thickness, i * twoDxInvDenom - yi*twoVDx * invDenom, ref pixels) > 0; ++i) ;
                for (int i = 1; IntensifyPixel(x, y - i, thickness, i * twoDxInvDenom + yi* twoVDx * invDenom, ref pixels) > 0; ++i) ;

                if (d > 0)
                {
                    twoVDx = d - dx;
                    y = y + yi;
                    d = d - 2 * dx;
                }
                else
                { 
                    twoVDx = d + dx; 
                }
                d = d + 2 * dy;
                x += 1;
            }
        }

        private void plotLineHigh(int x0, int y0, int x1, int y1, ref byte[] pixels)
        {
            int dx = x1 - x0;
            int dy = y1 - y0;
            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            int d = 2 * dx - dy;
            int y = y0;
            int x = x0;

            int twoVDx = 0;
            float invDenom = (float)(1 / (2 * Math.Sqrt(dy * dy + dx + dx)));
            float twoDxInvDenom = 2 * dy * invDenom;

            while (y < y1)
            {
                IntensifyPixel(x, y, thickness, twoVDx * invDenom, ref pixels);
                for (int i = 1; IntensifyPixel(x+i, y, thickness, i * twoDxInvDenom - xi* twoVDx * invDenom, ref pixels) > 0; ++i) ;
                for (int i = 1; IntensifyPixel(x-i, y, thickness, i * twoDxInvDenom + xi* twoVDx * invDenom, ref pixels) > 0; ++i) ;

                if (d > 0)
                {
                    twoVDx = d - dy;
                    x = x + xi;
                    d = d - 2 * dy;
                }
                else
                {
                    twoVDx = d + dy;
                }
                d = d + 2 * dx;
                y += 1;
            }
        }


    }
}
