using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Paint
{
    public class Polygon : Shape
    {
        private bool aliasing;
        private List<Line> edges;
        private List<int> fillColor;
        private byte[] imgPixels;
        private string imgPath;
        private int imgStride;
        //fill flag -> 0 no fill, 1 color fill, 2 image fill
        private int fill;

        // collection of edge buckets with key = ymin
        private Dictionary<int, List<Edge>> ET;
        private Dictionary<int, List<Edge>> AET;

        //edge struct for filling
        public struct Edge
        {
            public int ymax { get; }
            public float xmin { get; set; }
            public float minv { get; }
            public Edge(int _ymax, float _xmin, float _minv)
            {
                ymax = _ymax;
                xmin = _xmin;
                minv = _minv;
            }
            
        }

        public List<Line> Edges
        {
            get { return edges; }

        }

        public int FillFlag
        {
            get { return fill; }
        }

        public List<int> FillColor
        {
            get { return fillColor; }
        }

        public string ImgPath
        {
            get { return imgPath; }
        }


        //constructor for the rectangle
        public Polygon(int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels) : base(new List<int>(), _thickness, _color, _stride)
        {
            aliasing = _aliasing;
            imgPath = null;
            fill = 0;
        }

        public Polygon(List<int> _points, int _thickness, List<int> _color, int _stride, bool _aliasing, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            aliasing = _aliasing;
            ET = new Dictionary<int, List<Edge>>();
            imgPath = null;
            fill = 0;
            DrawShape(ref pixels);
        }

        public Polygon(List<int> _points, int _thickness, List<int> _color, List<int> _fillColor, string _imgPath, int _stride, bool _aliasing, ref byte[] pixels) : base(_points, _thickness, _color, _stride)
        {
            aliasing = _aliasing;
            ET = new Dictionary<int, List<Edge>>();
            if (_fillColor[0] != -1)
            {
                Edit(_fillColor);
            }
            else if(_imgPath != "empty" && _imgPath != null)
            {
                Edit(_imgPath);
            }
            else 
                fill = 0;
            DrawShape(ref pixels);
        }


        private List<int> ExtractX(Dictionary<int, List<Edge>> aet)
        {
            List<int> x = new List<int>();

            foreach(var bucket in aet)
            {
                foreach(var edge in bucket.Value)
                {
                    x.Add((int)Math.Round(edge.xmin));
                }
            }
            x.Sort();
            return x;
        }

        private void RemoveYMax(ref Dictionary<int, List<Edge>> aet, int y)
        {
            List<Edge> bucket;
            List<int> keys = aet.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                bucket = aet[keys[i]];
                for (int j = 0; j < bucket.Count; j++)
                {
                    if (bucket[j].ymax <= y)
                    {
                        aet[keys[i]].Remove(aet[keys[i]][j]);
                        if (bucket.Count == 0)
                            aet.Remove(keys[i]);
                    }
                }
            }
        }

        private void IncreaseX(ref Dictionary<int, List<Edge>> aet)
        {
            List<int> keys = aet.Keys.ToList();
            int ymax;
            float xmin, minv;

            for (int i = 0; i < keys.Count; i++)
            {
                for (int j = 0; j < aet[keys[i]].Count; j++)
                {
                    ymax = aet[keys[i]][j].ymax;
                    xmin = aet[keys[i]][j].xmin + aet[keys[i]][j].minv;
                    minv = aet[keys[i]][j].minv;
                    aet[keys[i]][j] = new Edge(ymax, xmin, minv);
                }
            }
        }

        public void Fill(ref byte[] pixels)
        {
            int y;
            List<int> keys = ET.Keys.ToList();
            keys.Sort();
            y = keys[0];
            List<int> x;
            AET = new Dictionary<int, List<Edge>>();
            do
            {
                // move bucket ET[y] to AET
                if(ET.ContainsKey(y)) 
                    AET.Add(y, ET[y]);
                //sort AET by x value
                x = ExtractX(AET);
                // fill pixels between pairs of intersections
                for (int i = 0; i < x.Count - 1; i += 2)
                {
                    for(int j = x[i]+thickness/2+1; j < x[i + 1]-thickness/2; j++)
                    {
                        if (fill == 1)
                        {
                            pixels[y * stride + j * 3] = (byte)fillColor[0];
                            pixels[y * stride + j * 3 + 1] = (byte)fillColor[1];
                            pixels[y * stride + j * 3 + 2] = (byte)fillColor[2];
                        }
                        else if (fill == 2)
                        {
                            int index2 = Math.Max(0, y * stride + j * 3);
                            int index = Math.Max(0, y * imgStride + j * 4);
                            pixels[index2 + 2] = (byte)imgPixels[index % imgPixels.Length];
                            pixels[index2 + 1] = (byte)imgPixels[(index + 1) % imgPixels.Length];
                            pixels[index2 ] = (byte)imgPixels[(index + 2) % imgPixels.Length];
                        }
                    }
                }
                y++;
                // remove from AET edges for which ymax = y
                RemoveYMax(ref AET, y);
                // for each edge in AET x += 1 / m
                IncreaseX(ref AET);
            }while(AET.Count != 0);
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
            ET = new Dictionary<int, List<Edge>>();
            List<int> tempPoints = new List<int>();
            int ymin, ymax, xmin;
            float m;
            Edge edge;
            for (int i = 0; i < points.Count / 2; i++)
            {
                tempPoints.Clear();
                tempPoints.Add(points[2 * i]);
                tempPoints.Add(points[2 * i + 1]);
                tempPoints.Add(points[(2 * i + 2) % points.Count]);
                tempPoints.Add(points[(2 * i + 3) % points.Count]);
                edges.Add(new Line(tempPoints, thickness, color, stride, aliasing, ref pixels));

                if(tempPoints[1] < tempPoints[3])
                {
                    ymin = tempPoints[1];
                    ymax = tempPoints[3];
                    xmin = tempPoints[0];
                    m = (float)(ymax-ymin) / (tempPoints[2]-xmin);
                }
                else
                {
                    ymin = tempPoints[3];
                    ymax = tempPoints[1];
                    xmin = tempPoints[2];
                    m = (float)(ymax - ymin) / (tempPoints[0] - xmin);
                }
               
                edge = new Edge(ymax, xmin, 1 / m);

                if (ET.ContainsKey(ymin))
                {                  
                    ET[ymin].Add(edge);
                }
                else
                {
                    ET.Add(ymin, new List<Edge>() { edge });
                }
            }
            if (fill > 0)
                Fill(ref pixels);
        }

        public override void Edit(List<int> newPoints, int? newThickness, List<int> newOutlineColor)
        {
            points = newPoints == null ? points : newPoints;
            thickness = newThickness == null ? thickness : (int)newThickness;
            if (newThickness != null) brush = new Brush(thickness);
            color = newOutlineColor == null ? color : new List<int>(newOutlineColor);
        }

        public void Edit(List<int> newFillColor)
        {
            fillColor = new List<int>(newFillColor);
            if (fillColor.All(x => x == 255))
                fill = 0;
            else
                fill = 1;
        }

        public void Edit(string newFillImgPath)
        {
            imgPath = newFillImgPath;
            Uri newFillImgUri = new Uri(imgPath);
            BitmapImage newFillImg;
            try
            {
                newFillImg = new BitmapImage(newFillImgUri);
            }
            catch(System.IO.FileNotFoundException e)
            {
                Console.Error.WriteLine(e);
                Edit(new List<int>() { 0, 0, 0 });
                fill = 1;
                return;
            }
            imgPixels = new byte[newFillImg.PixelWidth * newFillImg.PixelHeight * 4];
            imgStride = newFillImg.PixelWidth * 4;
            newFillImg.CopyPixels(imgPixels, imgStride, 0);
            fill = 2;
        }

    }
}
