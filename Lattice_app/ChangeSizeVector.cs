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
    public class ChangeSizeVector
    {
        Path first_vector;
        Path second_vector;
        public Dictionary<Vector, List<Point>> storage = new Dictionary<Vector, List<Point>>();
        public Dictionary<Vector, List<Point>> new_storage = new Dictionary<Vector, List<Point>>();

        private DispatcherTimer timer_for_change = new DispatcherTimer();
        double k = 0;
        public Canvas g;

        public ChangeSizeVector(Dictionary<Vector, List<Point>> st, Dictionary<Vector, List<Point>> new_st, Canvas canvas)
        {
            storage = st;
            new_storage = new_st;
            g = canvas;
           // first_vector = LineWithArrow(storage.ElementAt(0).Value[0].X, storage.ElementAt(0).Value[0].Y, storage.ElementAt(0).Value[1].X, storage.ElementAt(0).Value[1].Y, arrowType: ArrowType.End);
            second_vector = LineWithArrow(new_storage.ElementAt(0).Value[0].X, new_storage.ElementAt(0).Value[0].Y, new_storage.ElementAt(0).Value[1].X, new_storage.ElementAt(0).Value[1].Y, arrowType: ArrowType.End);
            second_vector.Stroke = Brushes.Green;
            g.Children.Add(second_vector);
            second_vector.Visibility = Visibility.Hidden;
            k =storage.ElementAt(0).Key.Length;
            timer_for_change.Tick += new EventHandler(timer_Move);
            timer_for_change.Interval = new TimeSpan(0, 0, 0, 0, 5);
            timer_for_change.Start();
        }
        void timer_Move(object sender, EventArgs e)
        {
            StartAnimation();
        }
        public void StartAnimation()
        {
            if (storage.ElementAt(0).Key.Length > new_storage.ElementAt(0).Key.Length)
            {
                if (k > new_storage.ElementAt(0).Key.Length)
                {
                    second_vector.Visibility = Visibility.Hidden;
                    Vector v_0_1 = Find_coord(storage.ElementAt(0).Key, new_storage.ElementAt(0).Key, k);
                    second_vector = LineWithArrow(new_storage.ElementAt(0).Value[0].X, new_storage.ElementAt(0).Value[0].Y, v_0_1.X + new_storage.ElementAt(0).Value[0].X, v_0_1.Y +new_storage.ElementAt(0).Value[0].Y);
                    second_vector.Stroke = Brushes.Green;
                    g.Children.Add(second_vector);
                    k--;
                    second_vector.Visibility = Visibility.Visible;
                }
                else timer_for_change.Stop();
            }
            else
            {
                if (k <= new_storage.ElementAt(0).Key.Length)
                {
                    second_vector.Visibility = Visibility.Hidden;
                    Vector v_0_1 = Find_coord(storage.ElementAt(0).Key, new_storage.ElementAt(0).Key, k);
                    second_vector = LineWithArrow(new_storage.ElementAt(0).Value[0].X, new_storage.ElementAt(0).Value[0].Y, v_0_1.X + new_storage.ElementAt(0).Value[0].X, v_0_1.Y + new_storage.ElementAt(0).Value[0].Y);
                    second_vector.Stroke = Brushes.Green;
                    g.Children.Add(second_vector);
                    k++;
                    second_vector.Visibility = Visibility.Visible;
                }
                else timer_for_change.Stop();
            }
        }
        public void HideVector()
        {
            second_vector.Visibility = Visibility.Hidden;
        }
        public void VisibleVector()
        {
            second_vector.Visibility = Visibility.Visible;
        }
        private Vector Find_coord(Vector v1, Vector v2, double d)
        {
            Vector v_0 = new Vector();
            double c = d / v1.Length;
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

    }
}
