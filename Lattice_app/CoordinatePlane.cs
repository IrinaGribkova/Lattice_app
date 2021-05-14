using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lattice_app
{
    public class CoordinatePlane
    {
        public List<Line> horizontal_lines = new List<Line>();
        public List<Line> vertical_lines = new List<Line>();
        public List<Ellipse> points_on_plane = new List<Ellipse>();
        public List<Path> coordinate_vectors = new List<Path>();
        public Canvas g;
        public double dist;
        public double thickness_line;
        List<TextBlock> digits = new List<TextBlock>();
        public Dictionary<double, Point> points_X = new Dictionary<double, Point>();
        public Dictionary<double, Point> points_Y = new Dictionary<double, Point>();
        

        public CoordinatePlane(Canvas c, double d, double thick)
        {
            g = c;
            dist = d;
            thickness_line = thick;
            for (double i = dist; i <= g.Width - dist; i += dist) // Create points on coodrinate plane
            {
                for (double j = dist; j <= g.Height - dist; j += dist)
                {
                    points_on_plane.Add(Create_point(i, j, Brushes.Black));
                }
            }
            for (double i = 0; i <= g.Width; i += dist)
            {
                vertical_lines.Add(Create_line(i, 0, i, g.Height, thickness_line));
                if (i <= g.Height - dist)
                {
                    horizontal_lines.Add(Create_line(0, i, g.Width, i, thickness_line));
                }
            }
            double k = dist;
            for (double i = (int)((g.Height / dist - 1) / 2); i >= -(int)((g.Height / dist - 1) / 2); i--)
            {
                points_Y.Add(i, new Point(g.Width / 2, k));
                k += dist;
            }
            k = dist;
            for (double i = -(int)((g.Width / dist - 1) / 2); i <= (int)((g.Width / dist - 1) / 2); i++)
            {
                points_X.Add(i, new Point(k, g.Height / 2));
                k += dist;
            }
            Add_vector(g.Width / 2, g.Height, g.Width / 2, 0, ref g, Brushes.Blue, false);
            Add_vector(0, g.Height / 2, g.Width, g.Height / 2, ref g, Brushes.Blue, false);
        }

        private void CreateCoordinateVectors()
        {
            foreach (var v in coordinate_vectors)
            {
                g.Children.Add(v);
            }
        }
        private void HideCoordinateVectors()
        {
            foreach (var v in coordinate_vectors)
            {
                v.Visibility = Visibility.Hidden;
            }
        }
        private void VisibleCoordinateVectors()
        {
            foreach (var v in coordinate_vectors)
            {
                v.Visibility = Visibility.Visible;
            }
        }

        private void Add_vector(double startX, double startY, double endX, double endY, ref Canvas gr, Brush brushes, bool add)
        {
            Path vector = LineWithArrow(startX, startY, endX, endY, arrowType: ArrowType.End);
            vector.Stroke = brushes;
            coordinate_vectors.Add(vector);
        }

        private void CreateDigits(Dictionary<double, Point> pnts)
        {
            TextBlock textBlock;
            foreach (var p in pnts)
            {
                textBlock = new TextBlock();
                textBlock.Text = p.Key.ToString();
                Canvas.SetLeft(textBlock, p.Value.X + 2);
                Canvas.SetTop(textBlock, p.Value.Y + 2);
                digits.Add(textBlock);
            }
        }

        private void Add_all_digits()
        {
            CreateDigits(points_X);
            CreateDigits(points_Y);
            foreach (var d in digits)
            {
                g.Children.Add(d);
            }
        }

        private void AddPoints()
        {
            foreach (var p in points_on_plane)
            {
                g.Children.Add(p);
            }
        }

        private void AddLattice()
        {
            foreach (var p in horizontal_lines)
            {
                g.Children.Add(p);
            }
            foreach (var p in vertical_lines)
            {
                g.Children.Add(p);
            }
        }

        public void CreateLattice()
        {
            AddPoints();
            AddLattice();
            HideLattice();
            Add_all_digits();
            CreateCoordinateVectors();
        }
        public void HideDigits()
        {
            foreach (var v in digits)
            {
                v.Visibility = Visibility.Hidden;
            }
        }
        public void VisibleDigits()
        {
            foreach (var v in digits)
            {
                v.Visibility = Visibility.Visible;
            }
        }

        public void HidePoints()
        {
            foreach (var p in points_on_plane)
            {
                p.Visibility = Visibility.Hidden;
            }
        }
        public void VisiblePoints()
        {
            foreach (var p in points_on_plane)
            {
                p.Visibility = Visibility.Visible;
            }
        }
        public void HideLattice()
        {
            foreach (var p in horizontal_lines)
            {
                p.Visibility = Visibility.Hidden;
            }
            foreach (var p in vertical_lines)
            {
                p.Visibility = Visibility.Hidden;
            }
        }
        public void ClearAll()
        {
            horizontal_lines.Clear();
            vertical_lines.Clear();
            points_on_plane.Clear();
            digits.Clear();
            coordinate_vectors.Clear();
        }
        public void HideCoordinatePlane()
        {
            HideLattice();
            HidePoints();
            HideDigits();
            HideCoordinateVectors();
        }

        public void VisibleLattice()
        {
            foreach (var p in horizontal_lines)
            {
                p.Visibility = Visibility.Visible;
            }
            foreach (var p in vertical_lines)
            {
                p.Visibility = Visibility.Visible;
            }
        }
        public void VisibleCoordinatePlane()
        {
            VisibleLattice();
            VisiblePoints();
            VisibleDigits();
            VisibleCoordinateVectors();
        }
        enum ArrowType
        {
            None, Start, End, Both
        }

        private static Path LineWithArrow(double startX, double startY, double endX, double endY, double len = 25, double width = 5, ArrowType arrowType = ArrowType.End)
        {
            var result = new Path();
            var v = new Vector(endX - startX, endY - startY);
            v.Normalize();
            var gg = new GeometryGroup();
            gg.Children.Add(new LineGeometry(new Point(startX, startY), new Point(endX, endY)));
            switch (arrowType)
            {
                case ArrowType.None:
                    break;
                case ArrowType.Start:
                    gg.Children.Add(Arrow(endX, endY, startX, startY, len, width));
                    break;
                case ArrowType.End:
                    gg.Children.Add(Arrow(startX, startY, endX, endY, len, width));
                    break;
                case ArrowType.Both:
                    gg.Children.Add(Arrow(endX, endY, startX, startY, len, width));
                    gg.Children.Add(Arrow(startX, startY, endX, endY, len, width));
                    break;
            }
            result.Data = gg;
            return result;
        }

        private static Geometry Arrow(double startX, double startY, double endX, double endY, double len = 25, double width = 5)
        {
            var v = new Vector(endX - startX, endY - startY);
            v.Normalize();
            var v1 = new Vector(endX - v.X * len, endY - v.Y * len);
            var n1 = new Vector(-v.Y * width / 2 + v1.X, v.X * width / 2 + v1.Y);
            var n2 = new Vector(v.Y * width / 2 + v1.X, -v.X * width / 2 + v1.Y);
            var gg = new GeometryGroup();
            gg.Children.Add(new LineGeometry(new Point(n1.X, n1.Y), new Point(endX, endY)));
            gg.Children.Add(new LineGeometry(new Point(n2.X, n2.Y), new Point(endX, endY)));
            return gg;
        }
        static Line Create_line(double x1, double y1, double x2, double y2, double thickness)
        {
            Line L = new Line();
            L.X1 = x1;
            L.Y1 = y1;
            L.X2 = x2;
            L.Y2 = y2;
            L.Stroke = Brushes.Blue;
            L.StrokeThickness = thickness;

            return L;
        }
        static Ellipse Create_point(double x, double y, Brush color)
        {
            Point point = new Point();
            Ellipse elipse = new Ellipse();
            elipse.StrokeThickness = 2;
            elipse.Stroke = color;
            point.X = x;
            point.Y = y;
            elipse.Width = 4;
            elipse.Height = 4;
            elipse.StrokeThickness = 2;
            elipse.Stroke = color;
            elipse.Margin = new Thickness(point.X - 2, point.Y - 2, 0, 0);

            return elipse;
        }
    }
}
