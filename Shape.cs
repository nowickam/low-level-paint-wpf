﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public abstract class Shape
    {
        protected List<int> points;

        protected int thickness;

        protected Brush brush;

        protected List<int> color;

        protected int stride;

        public Shape(List<int> _points, int _thickness, List<int> _color, int _stride)
        {
            points = _points;

            thickness = _thickness;

            brush = new Brush(thickness);

            stride = _stride;

            color = new List<int>();
            for(int i = 0; i < _color.Count; i++)
            {
                color.Add(_color[i]);
            }

        }

        protected abstract void DrawShape(ref byte[] pixels);

        protected void SetPixel(int x, int y, ref byte[] pixels)
        {
            int anchor;
            int shift = brush.Thickness / 2;
            for (int i = -shift; i <= shift; i++)
            {
                for (int j = -shift; j <= shift; j++)
                {
                    if (brush.Pattern[(i + shift) * brush.Stride + 3*(j + shift)] == 1)
                    {
                        anchor = (y+i) * stride + (x+j)*3 ;
                        pixels[anchor] = (byte)color[0];
                        pixels[anchor + 1] = (byte)color[1];
                        pixels[anchor + 2] = (byte)color[2];
                    }
                }
            } 
        }
    }

     
}
