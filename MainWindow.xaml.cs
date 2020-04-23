using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();

            width = (int)canvasContainer.MinWidth;
            height = (int)canvasContainer.MinHeight;
            stride = width * 3;

            canvas = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            pixels = new byte[height * stride];

            canvas.CopyPixels(pixels, stride, 0);
            resetCanvas();
            
            canvasContainer.Source = canvas;

            shapes = new List<Shape>();
            vertices = new List<Circle>();
            buffer = new List<int>();

            tool = 0;
        }

        private void resetCanvas()
        {
            for(int i=0; i < pixels.Length; i++)
            {
                pixels[i] = 250;
            }
            pixels[10 * stride + 10] = 0;
            pixels[10 * stride + 11] = 0;
            pixels[10 * stride + 12] = 0;

            updateCanvas();
        }

        private void updateCanvas()
        {
            canvas.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            resetCanvas();
            shapes.Clear();
            vertices.Clear();
            buffer.Clear();
        }

        private void colorCheck(TextBox tb)
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

        private List<int> getColor()
        {
            List<int> color = new List<int>();

            foreach(UIElement element in ColorContainer.Children)
            {
                if(element is TextBox)
                {
                    colorCheck(element as TextBox);
                    color.Add(int.Parse((element as TextBox).Text));
                }
            }
            return color;
        }

        private void handleVertex(int x, int y, bool ifAdd)
        {
            List<int> tempPoints = new List<int>();
            tempPoints.Add(x);
            tempPoints.Add(y);
            tempPoints.Add(x + 5);
            tempPoints.Add(y + 5);

            if (ifAdd)
            {
                vertices.Add(new Circle(tempPoints, 1, new List<int> { 255, 0, 0 }, stride, ref pixels));
                updateCanvas();
            }
            else
            {
                vertices.Add(new Circle(tempPoints, 1, new List<int> { 255, 255, 255 }, stride, ref pixels));
            }
        }

        private void mouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition((Image)sender).X;
            int y = (int)e.GetPosition((Image)sender).Y;

            handleVertex(x, y, true);

            buffer.Add(x);
            buffer.Add(y);

            if (tool != 2)
            {
                if (buffer.Count == 4)
                {
                    List<int> color = getColor();
                    if (tool == 0)
                        shapes.Add(new Line(buffer, 1, color, stride, ref pixels));
                    else
                        shapes.Add(new Circle(buffer, 1, color, stride, ref pixels));
                    handleVertex(buffer[0], buffer[1], false);
                    handleVertex(buffer[2], buffer[3], false);
                    buffer.Clear();
                    updateCanvas();
                }
            }
            if (tool == 2)
            {
                if (buffer.Count >= 6 && Math.Abs(x-buffer[0])<=5 && Math.Abs(y - buffer[1]) <= 5)
                {
                    List<int> color = getColor();
                    shapes.Add(new Polygon(buffer, 1, color, stride, ref pixels));
                    for (int i = 0; i < buffer.Count - 1; i++)
                    {
                        handleVertex(buffer[i], buffer[i + 1], false);
                    }
                    buffer.Clear();
                    updateCanvas();
                }
            }
        }


        private void LineBtn_Checked(object sender, RoutedEventArgs e)
        {
            if(buffer != null) buffer.Clear();
            tool = 0;
        }

        private void CircleBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null) buffer.Clear();
            tool = 1;
        }

        private void PolyBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (buffer != null) buffer.Clear();
            tool = 2;
        }

        private void ThickSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void AliasBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
