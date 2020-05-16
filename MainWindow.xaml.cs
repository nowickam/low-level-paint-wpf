using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap canvas = null;
        private byte[] pixels;
        private int height;
        private int width;
        private int stride;
        private List<Shape> shapes;
        private List<Circle> vertices;
        private List<int> buffer;
        private int tool;
        private int V_SIZE = 5;
        private bool newShape;

        public MainWindow()
        {
            InitializeComponent();

            width = (int)SystemParameters.PrimaryScreenWidth ;
            height = (int)SystemParameters.PrimaryScreenHeight ;
            stride = width * 3;

            canvas = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            pixels = new byte[height * stride];

            canvas.CopyPixels(pixels, stride, 0);
            ResetCanvas();
            Paint();

            canvasContainer.Source = canvas;

            shapes = new List<Shape>();
            vertices = new List<Circle>();
            buffer = new List<int>();

            newShape = true;
            tool = 0;
        }

        private void Redraw()
        {
            ResetCanvas();
            UpdateAllShapes();
            Paint();
        }

        private void ResetCanvas()
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }
        }

        private void Paint()
        {
            canvas.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void UpdateAllShapes()
        {
            foreach (Shape s in shapes)
            {
                s.DrawShape(ref pixels);
            }
        }

        private string ShapeToText()
        {
            StringBuilder sb = new StringBuilder();
            foreach(Shape s in shapes)
            {
                sb.Append(s.GetType().Name+":");
                foreach(int point in s.Points)
                {
                    sb.Append(point.ToString() + ",");
                }
                sb.Remove(sb.Length-1,1);
                sb.Append(":");
                foreach (int c in s.Color)
                {
                    sb.Append(c.ToString() + ",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(":");
                sb.Append(s.Thickness+"\n");
            }
            if(sb.Length>0)sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private List<Shape> TextToShape(string[] text)
        {
            List<Shape> shapes = new List<Shape>();
            foreach(string line in text)
            {
                string[] attr = line.Split(':');

                string[] pts = attr[1].Split(',');
                List<int> points = new List<int>();
                foreach (string pt in pts)
                    points.Add(int.Parse(pt));

                string[] clrs = attr[2].Split(',');
                List<int> color = new List<int>();
                foreach (string cl in clrs)
                    color.Add(int.Parse(cl));

                int thickness = int.Parse(attr[3]);
                Shape s;

                switch (attr[0])
                {
                    case "Line":
                        s = new Line(points, thickness, color, stride, GetAlias(), ref pixels);
                        break;
                    case "Polygon":
                        s = new Polygon(points, thickness, color, stride, GetAlias(), ref pixels);
                        break;
                    case "Circle":
                        s = new Circle(points, thickness, color, stride, ref pixels);
                        break;
                    default:
                        s = null;
                        break;
                }

                if(s!=null) shapes.Add(s);
                Console.WriteLine(s.GetType().Name);
            }
            return shapes;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string shapeText = ShapeToText();
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Paint files(*.paint)|*.paint";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, shapeText);
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Stream fileStream;
            StreamReader fileReader;
            string shapeText=string.Empty;
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Paint files(*.paint)|*.paint";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileStream = dialog.OpenFile();
                fileReader = new StreamReader(fileStream);
                shapeText = fileReader.ReadToEnd();

                if (shapeText != "")
                {
                    string[] shapesText = shapeText.Split('\n');
                    shapes.Clear();
                    shapes = TextToShape(shapesText);
                    newShape = false;
                }
                else
                {
                    shapes.Clear();
                    newShape = true;

                }
                vertices.Clear();
                buffer.Clear();
                Redraw();
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
            Paint();
            shapes.Clear();
            vertices.Clear();
            buffer.Clear();
            Info.Text = "";
            newShape = true;
        }

        private void ColorCheck(TextBox tb)
        {
            Decimal res;
            bool parsed = Decimal.TryParse(tb.Text, out res);
            if (tb.Text == null || !parsed)
            {
                tb.Text = "0";
            }
            else if (res > 255)
            {
                tb.Text = "255";
            }
        }

        private List<int> GetColor()
        {
            List<int> color = new List<int>();

            foreach (UIElement element in ColorContainer.Children)
            {
                if (element is TextBox)
                {
                    ColorCheck(element as TextBox);
                    color.Add(int.Parse((element as TextBox).Text));
                }
            }
            return color;
        }

        private void SetColor(List<int> color)
        {
            int i = 0;
            foreach (UIElement element in ColorContainer.Children)
            {
                if (element is TextBox)
                {
                    ((TextBox)element).Text = color[i].ToString();
                    i++;
                }
            }
        }

        private int GetThickness()
        {
            return (int)ThickSlider.Value;
        }

        private bool GetAlias()
        {
            return (bool)AliasBox.IsChecked;
        }

        private void HandleVertex(int x, int y, int c)
        {
            List<int> tempPoints = new List<int>();
            tempPoints.Add(x);
            tempPoints.Add(y);
            tempPoints.Add(x + V_SIZE);
            tempPoints.Add(y + V_SIZE);

            List<int> mode = new List<int> { 0, 0, 0 };
            mode[c] = 255;

            vertices.Add(new Circle(tempPoints, 1, mode, stride, ref pixels));
            Paint();
        }

        private void NewLineCircle(int x, int y)
        {
            buffer.Add(x);
            buffer.Add(y);

            if (buffer.Count == 4)
            {
                newShape = false;
                

                List<int> color = GetColor();
                if (tool == 0)
                    shapes.Add(new Line(buffer, GetThickness(), color, stride, GetAlias(), ref pixels));
                else
                    shapes.Add(new Circle(buffer, GetThickness(), color, stride, ref pixels));

                buffer.Clear();
                Info.Text = "";
                Redraw();
            }
            else
            {
                newShape = true;
                HandleVertex(x, y, 1);
            }
        }

        private void NewCapsule(int x, int y)
        {
            buffer.Add(x);
            buffer.Add(y);

            if (buffer.Count == 6)
            {
                Redraw();
                newShape = false;
                Capsule c = new Capsule(buffer, GetThickness(), GetColor(), stride, ref pixels);
                shapes.Add(c);
                buffer.Clear();
                Info.Text = "";
                Paint();
            }
            else
            {
                newShape = true;
                HandleVertex(x, y, 1);
            }
        }

        private void NewPolygon(int x, int y)
        {
            if (buffer.Count >= 6 && Math.Abs(x - buffer[0]) <= V_SIZE && Math.Abs(y - buffer[1]) <= V_SIZE)
            {
                newShape = false;
                //delete vertices
                Redraw();

                List<int> color = GetColor();
                shapes.Add(new Polygon(buffer, GetThickness(), color, stride, GetAlias(), ref pixels));

                buffer.Clear();
                Info.Text = "";
                Paint();
            }
            else
            {
                newShape = true;
                buffer.Add(x);
                buffer.Add(y);
                HandleVertex(x, y, 1);
                Info.Text += "\nTo finish shape click near the start";
            }
        }

        private Shape CheckClik(int x, int y)
        {
            Shape existingShape;
            existingShape = CheckEditMode();
            if (existingShape == null)
            {
                foreach (Shape s in shapes)
                {
                    existingShape = s.CheckClick(x, y);
                    if (existingShape != null) return existingShape;
                }
            }
            else return existingShape;
            return null;
        }

        private void SetShapeConfig(Shape s)
        {
            ThickSlider.Value = s.Thickness;
            SetColor(s.Color);
        }

        private void EditLine(Shape existingShape, int x, int y)
        {
            //finish editing
            DeleteBtn.IsEnabled = false;
            ApplyColorBtn.IsEnabled = false;
            if (buffer.Count == 2)
            {
                existingShape.Edit(new List<int> { buffer[0], buffer[1], x, y }, null, null);
                Redraw();
                buffer.Clear();
                Info.Text = "";
            }
            //start editing
            else if(buffer.Count==0)
            {
                Info.Text = "Click somewhere to change the position of the vertex";
                DeleteBtn.IsEnabled = true;
                ApplyColorBtn.IsEnabled = true;
                HandleVertex(x, y, 2);
                if (x <= existingShape.Points[0] + V_SIZE && x >= existingShape.Points[0] - V_SIZE)
                {
                    buffer.Add(existingShape.Points[2]);
                    buffer.Add(existingShape.Points[3]);
                }
                else
                {
                    buffer.Add(existingShape.Points[0]);
                    buffer.Add(existingShape.Points[1]);
                }
            }
        }

        private void EditCircle(Shape existingShape, int x, int y)
        {
            int cx = ((Circle)existingShape).CX, cy = ((Circle)existingShape).CY;
            if (buffer.Count == 0)
            {
                Info.Text = "Click on the circle to change the radius,\n click on the center to move the circle";
                DeleteBtn.IsEnabled = true;
                ApplyColorBtn.IsEnabled = true;
                HandleVertex(cx, cy, 2);
                buffer.Add(cx);
                buffer.Add(cy);
            }
            else if (buffer.Count == 2)
            {
                DeleteBtn.IsEnabled = false;
                ApplyColorBtn.IsEnabled = false;
                //if clicked on the center -> move the circle
                if (x <= cx + 5 && x >= cx - 5 && y <= cy + 5 && y >= cy - 5)
                {
                    Info.Text = "Click somewhere to change the position of the circle";
                    HandleVertex(cx, cy, 0);
                    int r = ((Circle)existingShape).R;
                    existingShape.EditMode = true;
                    buffer.Add(r);
                }
                //if clicked elsewhere -> change the radius
                else
                {
                    existingShape.Edit(new List<int> { buffer[0], buffer[1], x, y }, null, null);
                    Redraw();
                    buffer.Clear();
                    Info.Text = "";
                }
            }
            //new center
            else if (buffer.Count == 3)
            {
                existingShape.Edit(new List<int> { x, y, x, y + buffer[2] }, null, null);
                Redraw();
                buffer.Clear();
                Info.Text = "";
            }
        }

        private void EditPolygon(Shape existingShape, int x, int y)
        {
            if (buffer.Count == 0)
            {
                Info.Text = "Choose one vertex to change the vertex' position,\n choose two neighboring vertices to change the edge's position,\n choose three neighboring vertices to change the position of the polygon";
                DeleteBtn.IsEnabled = true;
                ApplyColorBtn.IsEnabled = true;
                HandleVertex(x, y, 2);
                buffer.Add(x);
                buffer.Add(y);
            }
            else if (buffer.Count == 2)
            {
                DeleteBtn.IsEnabled = false;
                ApplyColorBtn.IsEnabled = false;
                List<int> nb = (existingShape as Polygon).CheckNeighborClick(buffer[0], buffer[1], x, y);
                //move vertex
                if (nb.Count == 2)
                {
                    (existingShape as Polygon).PrepareNewVertices(nb[0], nb[1], x, y);
                    existingShape.Edit(existingShape.Points, null, null);
                    Redraw();
                    buffer.Clear();
                    Info.Text = "";
                }
                //move edge
                else if (nb.Count == 4)
                {
                    Info.Text = "Choose the place to change the position of the edge,\n click on a neighboring vertex to move the polygon";
                    HandleVertex(x, y, 0);
                    HandleVertex(buffer[0], buffer[1], 0);
                    existingShape.EditMode = true;
                    buffer.Add(nb[0]);
                    buffer.Add(nb[1]);
                    buffer.Add(nb[2]);
                    buffer.Add(nb[3]);
                }
            }
            else if (buffer.Count == 6)
            {
                List<int> nb1 = (existingShape as Polygon).CheckNeighborClick(buffer[2], buffer[3], x, y);
                List<int> nb2 = (existingShape as Polygon).CheckNeighborClick(buffer[4], buffer[5], x, y);
                //move edge
                if (nb1.Count == 2 && nb2.Count == 2)
                {
                    int dx = buffer[4] + (buffer[2] - buffer[4]) / 2;
                    int dy = buffer[5] + (buffer[3] - buffer[5]) / 2;
                    dx = x - dx;
                    dy = y - dy;
                    (existingShape as Polygon).PrepareNewVertices(buffer[2], buffer[3], buffer[2] + dx, buffer[3] + dy);
                    (existingShape as Polygon).PrepareNewVertices(buffer[4], buffer[5], buffer[4] + dx, buffer[5] + dy);
                    existingShape.Edit(existingShape.Points, null, null);
                    Redraw();
                    buffer.Clear();
                    Info.Text = "";
                }
                //move entire polygon
                else
                {
                    Info.Text = "Click somewhere to change the position of the polygon";
                    HandleVertex(x, y, 0);

                    int x1 = Math.Max(Math.Max(x, buffer[2]), buffer[4]);
                    int x2 = Math.Min(Math.Min(x, buffer[2]), buffer[4]);

                    int y1 = Math.Max(Math.Max(y, buffer[3]), buffer[5]);
                    int y2 = Math.Min(Math.Min(y, buffer[3]), buffer[5]);

                    int dx = (x1 - x2) / 2 + x2;
                    int dy = (y1 - y2) / 2 + y2;

                    buffer.Add(dx);
                    buffer.Add(dy);
                    HandleVertex(dx, dy, 2);
                    existingShape.EditMode = true;
                }
            }
            else
            {
                int dx = x - buffer[6];
                int dy = y - buffer[7];
                for (int i = 0; i < existingShape.Points.Count - 1; i += 2)
                {
                    (existingShape as Polygon).PrepareNewVertices(existingShape.Points[i], existingShape.Points[i + 1], existingShape.Points[i] + dx, existingShape.Points[i + 1] + dy);
                }
                existingShape.Edit(existingShape.Points, null, null);
                Redraw();
                buffer.Clear();
                Info.Text = "";
            }
        }

        private void MouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition((Image)sender).X;
            int y = (int)e.GetPosition((Image)sender).Y;

            Shape existingShape = CheckClik(x, y);

            if (existingShape != null && !newShape)
            {
                SetShapeConfig(existingShape);
                if (existingShape is Line)
                {
                    EditLine(existingShape, x, y);
                }
                else if (existingShape is Circle)
                {
                    EditCircle(existingShape, x, y);
                }
                else if (existingShape is Polygon)
                {
                    EditPolygon(existingShape, x, y);                    
                }
            }
            else
            {
                Info.Text = "Click to place new vertex";
                //new line or circle
                if (tool < 2)
                {
                    NewLineCircle(x, y);
                }
                //new polygon
                if (tool == 2)
                {
                    NewPolygon(x, y);
                }
                if(tool == 3)
                {
                    NewCapsule(x, y);
                }
            }
        }


        private void LineBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null)
            {
                buffer.Clear();
                Redraw();
            }            
            tool = 0;
        }

        private void CircleBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null)
            {
                buffer.Clear();
                Redraw();
            }
            tool = 1;
        }

        private void PolyBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null)
            {
                buffer.Clear();
                Redraw();
            }
            tool = 2;
        }

        private void AliasBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)(sender as CheckBox).IsChecked;
            foreach (Shape s in shapes)
            {
                if (s is Line)
                {
                    (s as Line).changeAlias(isChecked);
                }
            }
            Redraw();

        }

        private Shape CheckEditMode()
        {
            if (shapes != null)
            {
                foreach (Shape s in shapes)
                {
                    if (s.EditMode)
                    {
                        s.EditMode = false;
                        return s;
                    }
                }
            }
            return null;
        }

        private void ThickSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Shape edited = CheckEditMode();
            int value = (int)((Slider)sender).Value;
            
            if (edited != null)
            {
                edited.EditMode = false;
                buffer.Clear();
                Info.Text = "";
                edited.Edit(null, value, null);
                Redraw();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Shape edited = CheckEditMode();
            DeleteBtn.IsEnabled = false;
            ApplyColorBtn.IsEnabled = false;
            
            if (edited != null)
            {
                buffer.Clear();
                Info.Text = "";
                shapes.Remove(edited);
                Redraw();
            }
        }

        private void ApplyColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Shape edited = CheckEditMode();
            List<int> color = GetColor();
            DeleteBtn.IsEnabled = false;
            ApplyColorBtn.IsEnabled = false;
            
            if (edited != null)
            {
                edited.EditMode = false;
                buffer.Clear();
                Info.Text = "";
                edited.Edit(null, null, color);
                Redraw();
            }
        }

        private void CapsuleBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null)
            {
                buffer.Clear();
                Redraw();
            }
            tool = 3;
        }
    }
}
