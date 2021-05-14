using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lattice_app
{
    public class CreateLattice
    {
        Dictionary<Vector, List<Point>> basis = new Dictionary<Vector, List<Point>>();
        Canvas g;
        Line line_up;
        Line line_down;
        Line line_left;
        Vector left_vector;
        Line l_up;
        double angle_up;
        double angle_down;
        double angle_between_vectors;

        Point intersect_point;
        Point p_up;
        Point p_down;
        public List<Line> horiz_lines_on_plane = new List<Line>();
        public List<Line> vert_lines_on_plane = new List<Line>();
        List<Line> OrthogonalLines = new List<Line>();
        public bool status;
        public bool time_stop = false;
        Brush brush;
        public Path vector;
        AnimationVector ex;
        public  List<Path> vectors_on_plane = new List<Path>();
        public CreateLattice(Dictionary<Vector, List<Point>> b, List<Path> vect, Canvas canvas, Brush br, bool st)
        {
            basis = b;
            g = canvas;
            vectors_on_plane = vect;
            brush = br;
            status = st;

            line_up = Create_line(0, 0, g.Width, 0, 1);
            line_down = Create_line(0, g.Height, g.Width, g.Height, 1);
            line_left = Create_line(0, 0, 0, g.Height, 1);
            left_vector = new Vector(0, g.Height);
            l_up = Create_line(basis.ElementAt(0).Value[0].X, basis.ElementAt(0).Value[0].Y, basis.ElementAt(0).Value[1].X, basis.ElementAt(0).Value[1].Y, 2);
            angle_up = GetAngle(0);
            angle_down = GetAngle(1);
            angle_between_vectors = GetDegrees(basis.ElementAt(0).Key, basis.ElementAt(1).Key);
            Point important = new Point(basis.ElementAt(1).Value[1].X - (basis.ElementAt(1).Value[0].X - basis.ElementAt(0).Value[0].X), basis.ElementAt(1).Value[1].Y - (basis.ElementAt(1).Value[0].Y - basis.ElementAt(0).Value[0].Y));
            intersect_point = GetForthPoint(basis.ElementAt(0).Value[0], important, basis.ElementAt(0).Value[1]);
            Line new_line = Create_line(basis.ElementAt(0).Value[0].X, basis.ElementAt(0).Value[0].Y, important.X, important.Y, 2);
            Path new_vector = LineWithArrow(basis.ElementAt(0).Value[0].X, basis.ElementAt(0).Value[0].Y, important.X, important.Y, arrowType: ArrowType.End);
            new_vector.Stroke = Brushes.Green;
            List<Point> points_new_vector = new List<Point>() { new Point(basis.ElementAt(0).Value[0].X, basis.ElementAt(0).Value[0].Y), new Point(important.X, important.Y) };
            if (status is true)
            {
                ex = new AnimationVector(new Dictionary<Vector, List<Point>>() { { basis.ElementAt(1).Key, basis.ElementAt(1).Value } }, new Dictionary<Vector, List<Point>>() { { new Vector(important.X - basis.ElementAt(0).Value[0].X, important.Y - basis.ElementAt(0).Value[0].Y), points_new_vector } }, g, false);
                ex.PrepareParameters();
                time_stop = ex.status;
            }
            p_up = Intersect(l_up, line_up);
            p_down = Intersect(new_line, line_down);
            if (basis.ElementAt(0).Value[0].Y != basis.ElementAt(0).Value[1].Y && basis.ElementAt(1).Value[0].Y != basis.ElementAt(1).Value[1].Y)
            {
                horiz_lines_on_plane = StraightLines_down(GetDistUp(l_up, new_line, line_down, intersect_point), p_down.X);
                horiz_lines_on_plane = AngleLines(angle_down, ref horiz_lines_on_plane);
                vert_lines_on_plane = StraightLines_up(GetDistUp(new_line, l_up, line_up, intersect_point), p_up.X);
                vert_lines_on_plane = AngleLines(angle_up, ref vert_lines_on_plane);
            }
            if (basis.ElementAt(0).Value[0].Y == basis.ElementAt(0).Value[1].Y && basis.ElementAt(1).Value[0].Y != basis.ElementAt(1).Value[1].Y)
            {
                vert_lines_on_plane = StraightLines_down(GetDistUp(l_up, new_line, line_down, intersect_point), p_down.X);
                vert_lines_on_plane = AngleLines(angle_down, ref vert_lines_on_plane);
                horiz_lines_on_plane = CreateOrthogonalLines(basis.ElementAt(0).Value[0].Y, Math.Abs(basis.ElementAt(1).Value[0].Y - basis.ElementAt(1).Value[1].Y));
            }
            if (basis.ElementAt(0).Value[0].Y != basis.ElementAt(0).Value[1].Y && basis.ElementAt(1).Value[0].Y == basis.ElementAt(1).Value[1].Y)
            {
                vert_lines_on_plane = StraightLines_up(GetDistUp(new_line, l_up, line_up, intersect_point), p_up.X);
                vert_lines_on_plane = AngleLines(angle_up, ref vert_lines_on_plane);
                horiz_lines_on_plane = CreateOrthogonalLines(basis.ElementAt(0).Value[0].Y, Math.Abs(basis.ElementAt(0).Value[0].Y - basis.ElementAt(0).Value[1].Y));
            }
            vect[1].Visibility = Visibility.Hidden;
            vect.Remove(vectors_on_plane[1]);
            vect.Add(new_vector);
            //basis.Remove(basis.ElementAt(1).Key);
            //basis.Add(new Vector(important.X - basis.ElementAt(0).Value[0].X, important.Y - basis.ElementAt(0).Value[0].Y), points_new_vector);
        }
        public void DrawLattice()
        {
            foreach (var ve in vert_lines_on_plane)
            {
                g.Children.Add(ve);
            }
            foreach (var ve in horiz_lines_on_plane)
            {
                g.Children.Add(ve);
            }
        }
        //public void HideVectors()
        //{
        //    ex.vector.Visibility = Visibility.Hidden;
        //}
        //public void VisibleVectors()
        //{          
        //   ex.vector.Visibility = Visibility.Visible;
        //}
        public void HideLattice()
        {
            foreach (var ve in vert_lines_on_plane)
            {
               ve.Visibility = Visibility.Hidden;
            }
            foreach (var ve in horiz_lines_on_plane)
            {
                ve.Visibility = Visibility.Hidden;
            }
        }
        public void VisibleLattice()
        {
            foreach (var ve in vert_lines_on_plane)
            {
                ve.Visibility = Visibility.Visible;
            }
            foreach (var ve in horiz_lines_on_plane)
            {
                ve.Visibility = Visibility.Visible;
            }
        }
        private Line Create_line(double x1, double y1, double x2, double y2, double thickness)
        {
            Line L = new Line();
            L.X1 = x1;
            L.Y1 = y1;
            L.X2 = x2;
            L.Y2 = y2;
            L.Stroke = brush;
            L.StrokeThickness = thickness;

            return L;
        }
        private double GetAngle(int i)
        {
            double angle = 0;
            if ((basis.ElementAt(i).Value[0].X <= basis.ElementAt(i).Value[1].X && basis.ElementAt(i).Value[0].Y >= basis.ElementAt(i).Value[1].Y) || (basis.ElementAt(i).Value[0].X >= basis.ElementAt(i).Value[1].X && basis.ElementAt(i).Value[0].Y <= basis.ElementAt(i).Value[1].Y))
                angle = GetDegrees(left_vector, basis.ElementAt(i).Key);
            if ((basis.ElementAt(i).Value[0].X > basis.ElementAt(i).Value[1].X && basis.ElementAt(i).Value[0].Y >= basis.ElementAt(i).Value[1].Y) || (basis.ElementAt(i).Value[0].X < basis.ElementAt(i).Value[1].X && basis.ElementAt(i).Value[0].Y < basis.ElementAt(i).Value[1].Y))
                angle = -GetDegrees(left_vector, basis.ElementAt(i).Key);
            return angle;
        }
        private double LengthVector(Vector v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }
        private double DistBetweeenPoints(Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y));
        }

        public void UndoLines()
        {
            foreach (var v in horiz_lines_on_plane)
                v.Visibility = Visibility.Hidden;
            foreach (var v in vert_lines_on_plane)
                v.Visibility = Visibility.Hidden;
            foreach (var v in OrthogonalLines)
                v.Visibility = Visibility.Hidden;
        }
        public void DoLines()
        {
            foreach (var v in horiz_lines_on_plane)
                v.Visibility = Visibility.Visible;
            foreach (var v in vert_lines_on_plane)
                v.Visibility = Visibility.Visible;
            foreach (var v in OrthogonalLines)
                v.Visibility = Visibility.Visible;
        }

        private List<Line> StraightLines_up(double rast, double sdvig)
        {
            List<Line> res = new List<Line>();
            for (double i = sdvig - g.Width * rast; i < g.Width + 10000; i += rast)
            {
                res.Add(Create_line(i, 0, i, 10000, 0.5));
            }
            return res;
        }
        private List<Line> StraightLines_down(double rast, double sdvig)
        {
            List<Line> res = new List<Line>();
            for (double i = sdvig - g.Width * rast; i < g.Width + g.Width * 1000; i += rast)
            {
                res.Add(Create_line(i, g.Height, i, -2000, 0.5));
            }
            return res;
        }
        private List<Line> StraightOrthogonalLines(double sdvig_vertical, double sdvig_horizontal)
        {
            List<Line> res = new List<Line>();
            if (basis.ElementAt(0).Value[0].X == basis.ElementAt(0).Value[1].X)
            {
                for (double i = basis.ElementAt(0).Value[0].X - 50 * sdvig_vertical; i < g.Width + 5000; i += sdvig_vertical)
                {
                    res.Add(Create_line(i, g.Height, i, -2000, 0.5));
                }
                for (double i = basis.ElementAt(0).Value[0].Y - 50 * sdvig_horizontal; i < g.Height + 5000; i += sdvig_horizontal)
                {
                    res.Add(Create_line(0, i, 2000, i, 0.5));
                }
            }
            else
            {
                for (double i = basis.ElementAt(0).Value[0].X - 50 * sdvig_horizontal; i < g.Width + 5000; i += sdvig_horizontal)
                {
                    res.Add(Create_line(i, g.Height, i, -2000, 0.5));
                }
                for (double i = basis.ElementAt(0).Value[0].Y - 50 * sdvig_vertical; i < g.Height + 5000; i += sdvig_vertical)
                {
                    res.Add(Create_line(0, i, 2000, i, 0.5));
                }
            }
            return res;
        }
        private List<Line> CreateOrthogonalLines(double start_point, double height_vector)
        {
            List<Line> res = new List<Line>();
            for (double i = start_point - g.Height * height_vector; i < g.Width + 5000; i += height_vector)
            {
                res.Add(Create_line(0, i, g.Width * 2, i, 0.5));
            }
            return res;
        }
        private List<Line> AngleLines(double angle, ref List<Line> lines)
        {
            List<Line> res = new List<Line>();

            for (int i = 0; i < lines.Count; i++)
            {
                RotateTransform alpha = new RotateTransform();
                alpha.CenterX = lines[i].X1;
                alpha.CenterY = lines[i].Y1;
                alpha.Angle = angle;
                Canvas.SetTop(lines[i], 0);
                Canvas.SetLeft(lines[i], 0);
                lines[i].RenderTransform = alpha;
            }
            return lines;
        }
        private Ellipse AddPoint(double x, double y, double thickness)
        {

            Point point = new Point(x, y);
            Ellipse elipse = new Ellipse();

            elipse.Width = thickness;
            elipse.Height = thickness;

            elipse.StrokeThickness = thickness / 2;
            elipse.Stroke = Brushes.Black;
            elipse.Margin = new Thickness(point.X - thickness / 2, point.Y - thickness / 2, 0, 0);
            return elipse;
        }
        private double GetDegrees(Vector v1, Vector v2)
        {
            return Math.Min(Math.Acos((v1 * v2) / (v1.Length * v2.Length)) * (180 / Math.PI), Math.Acos((-v1 * v2) / (v1.Length * v2.Length)) * (180 / Math.PI));
        }
        private Point Intersect(Line a, Line b)
        {
            double A1 = a.Y2 - a.Y1;
            double B1 = a.X1 - a.X2;
            double C1 = A1 * a.X1 + B1 * a.Y1;

            double A2 = b.Y2 - b.Y1;
            double B2 = b.X1 - b.X2;
            double C2 = A2 * b.X1 + B2 * b.Y1;

            double numitor = A1 * B2 - A2 * B1;
            if (numitor == 0) return new Point(0, 0);
            else
            {
                double x = (B2 * C1 - B1 * C2) / numitor;
                double y = (A1 * C2 - A2 * C1) / numitor;
                return new Point(x, y);
            }
        }
        private Point GetForthPoint(Point a, Point b, Point d)
        {
            double x0 = (b.X + d.X) * 0.5;
            double y0 = (b.Y + d.Y) * 0.5;
            return new Point(2 * x0 - a.X, 2 * y0 - a.Y);
        }
        private double GetDistanceBetweenPoints(Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }
        private double GetDistUp(Line line, Line line2, Line border, Point fourth_point)
        {
            Point p = Intersect(Create_line(line.X2, line.Y2, fourth_point.X, fourth_point.Y, 1), border);
            Point p2 = Intersect(line2, border);
            return GetDistanceBetweenPoints(p, p2);
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
    }
}
