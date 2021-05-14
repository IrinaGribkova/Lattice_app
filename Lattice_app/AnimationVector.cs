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
    public class AnimationVector
    {
        public Dictionary<Vector, List<Point>> storage = new Dictionary<Vector, List<Point>>();
        public Dictionary<Vector, List<Point>> new_storage = new Dictionary<Vector, List<Point>>();
        private DispatcherTimer timer_for_rotate = new DispatcherTimer();
        public DispatcherTimer timer_for_move = new DispatcherTimer();
        public Path vector;
        private int angle = 0;
        private double gran_x;
        private double gran_y;
        private double gran_x_x;
        private double gran_y_y;
        private int koef_x = 0;
        private int koef_y = 0;
        private double k_x;
        private double k_y;
        private int k_a = 0;
        private int k_b = 0;
        private double degrees = 0;
        public Canvas g;
        public bool status;
        int n = 0;
        Vector v_0_1 = new Vector();
        ChangeSizeVector change_v1;
        public AnimationVector(Dictionary<Vector, List<Point>> st, Dictionary<Vector, List<Point>> new_st, Canvas canvas, bool stat)
        {
            storage = st;
            new_storage = new_st;
            g = canvas;
            status = stat;

            vector = LineWithArrow(storage.ElementAt(0).Value[0].X, storage.ElementAt(0).Value[0].Y, storage.ElementAt(0).Value[1].X, storage.ElementAt(0).Value[1].Y, arrowType: ArrowType.End);
            vector.Stroke = Brushes.Green;
            g.Children.Add(vector);
            k_x = storage.ElementAt(0).Value[0].X;
            k_y = storage.ElementAt(0).Value[0].Y;
            gran_x = new_storage.ElementAt(0).Value[0].X;
            gran_y = new_storage.ElementAt(0).Value[0].Y;
            v_0_1 = Find_coord(storage.ElementAt(0).Key,new_storage.ElementAt(0).Key);
            Point important = new Point(new_storage.ElementAt(0).Value[1].X - (new_storage.ElementAt(0).Value[0].X - storage.ElementAt(0).Value[0].X), new_storage.ElementAt(0).Value[1].Y - (new_storage.ElementAt(0).Value[0].Y - storage.ElementAt(0).Value[0].Y));
            degrees = GetDegrees(storage.ElementAt(0).Key, new_storage.ElementAt(0).Key);

            if (k_x <= gran_x)
            {
                koef_x = 1;
                gran_x_x = gran_x - k_x;
            }
            else if (k_x > gran_x)
            {
                koef_x = -1;
                gran_x_x = gran_x;
            }
            if (k_y <= gran_y)
            {
                koef_y = 1;
                gran_y_y = gran_y - k_y;
            }
            else { koef_y = -1; gran_y_y = gran_y; }
            timer_for_rotate.Tick += new EventHandler(timer_Degree);
            timer_for_rotate.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer_for_move.Tick += new EventHandler(timer_Move);
            timer_for_move.Interval = new TimeSpan(0, 0, 0, 0, 1);
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
        void timer_Degree(object sender, EventArgs e)
        {
            if (degrees < 0)
            {
                if (angle > degrees)
                {
                    RotateTransform alpha = new RotateTransform();
                    alpha.CenterX = k_x;
                    alpha.CenterY = k_y;
                    alpha.Angle = angle;
                    Canvas.SetTop(vector, 0);
                    Canvas.SetLeft(vector, 0);
                    vector.RenderTransform = alpha;
                    angle--;
                }
                else
                {
                    timer_for_rotate.Stop();
                    if (storage.ElementAt(0).Value[0] != new_storage.ElementAt(0).Value[0])
                        timer_for_move.Start();
                    else Change_size();
                }
            }
            else
            {
                if (angle <= degrees)
                {
                    RotateTransform alpha = new RotateTransform();
                    alpha.CenterX = k_x;
                    alpha.CenterY = k_y;
                    alpha.Angle = angle;
                    Canvas.SetTop(vector, 0);
                    Canvas.SetLeft(vector, 0);
                    vector.RenderTransform = alpha;
                    angle++;
                }
                else
                {
                    timer_for_rotate.Stop();
                    if (storage.ElementAt(0).Value[0] != new_storage.ElementAt(0).Value[0])
                        timer_for_move.Start();
                    else Change_size();
                }
            }
        }
        public void HideVector()
        {
            vector.Visibility = Visibility.Hidden;
        }
        public void HideChangedVector()
        {
            change_v1.HideVector();
        }
        public void VisibleVector()
        {
            vector.Visibility = Visibility.Visible;
        }
        void timer_Move(object sender, EventArgs e)
        {
            StartAnimation();
        }
        void Change_size()
        {
            HideVector();
            Vector v_0_1 = Find_coord(new_storage.ElementAt(0).Key,storage.ElementAt(0).Key);
            change_v1 = new ChangeSizeVector(new Dictionary<Vector, List<Point>>() {{v_0_1, new List<Point>() { new Point(new_storage.ElementAt(0).Value[0].X, new_storage.ElementAt(0).Value[0].Y), new Point(v_0_1.X + new_storage.ElementAt(0).Value[0].X, v_0_1.Y + new_storage.ElementAt(0).Value[0].Y) } } },new_storage, g);
         

            //vector = LineWithArrow(new_storage.ElementAt(0).Value[0].X, new_storage.ElementAt(0).Value[0].Y, new_storage.ElementAt(0).Value[1].X, new_storage.ElementAt(0).Value[1].Y, arrowType: ArrowType.End);
            //vector.Stroke = Brushes.Black;
            ////g.Children.Add(vector);
        }

        private Vector Find_coord(Vector v1, Vector v2)
        {
            Vector v_0 = new Vector();
            double c = v2.Length/v1.Length;
            v_0 = new Vector(c * v1.X, c * v1.Y);
            return v_0;
        }

        enum ArrowType
        {
            None, Start, End, Both
        }
        private static System.Windows.Shapes.Path LineWithArrow(double startX, double startY, double endX, double endY, double len = 25, double width = 5, ArrowType arrowType = ArrowType.End)
        {
            var result = new System.Windows.Shapes.Path();
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
        public double Length(double startX, double startY, double endX, double endY)
        {
            return Math.Sqrt((endX - startX) * (endX - startX) + (endY - startY) * (endY - startY));
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
            if (-(180 / Math.PI) * Math.Atan2(v1.X * v2.Y - v1.Y * v2.X, v1.X * v2.X + v1.Y * v2.Y) < 0)
                return (double)((360 - (-(180 / Math.PI) * Math.Atan2(v1.X * v2.Y - v1.Y * v2.X, v1.X * v2.X + v1.Y * v2.Y)))) % 360;
            else
                return -(-(180 / Math.PI) * Math.Atan2(v1.X * v2.Y - v1.Y * v2.X, v1.X * v2.X + v1.Y * v2.Y));
        }

        public void PrepareParameters()
        {
            timer_for_rotate.Start();
        }
        private void StartAnimation()
        {
            if (n==2)
            { timer_for_move.Stop(); if (status is true) { Change_size(); vector.Visibility = Visibility.Hidden; } }//
            if (koef_x == 1)
            {
                if (k_x <= gran_x)
                {
                    Canvas.SetLeft(vector, k_a);
                    k_a++;
                    k_x += koef_x;
                }
                if (k_x <= gran_x && k_x > gran_x_x)
                {
                    k_a++;
                    k_x += koef_x;
                }
                if (k_x == gran_x)
                    n++;
            }
            else if (koef_x == -1)
            {
                if (k_x > gran_x_x)
                {
                    Canvas.SetLeft(vector, k_a);
                    k_a--;
                    k_x += koef_x;
                }
                if (k_x >= gran_x && k_x < gran_x_x)
                {
                    k_a--;
                    k_x += koef_x;
                }
                if (k_x == gran_x)
                    n++;
            }
            if (koef_y == 1)
            {
                if (k_y <= gran_y)
                {
                    Canvas.SetTop(vector, k_b);
                    k_b++;
                    k_y += koef_y;
                }
                if(k_y == gran_y)
                    n++;
                
            }
            else if (koef_y == -1)
            {
                if (k_y >= gran_y)
                {
                    Canvas.SetTop(vector, k_b);
                    k_b--;
                    k_y += koef_y;
                }
                if (k_y == gran_y)
                    n++;
            }
        }
    }
}
