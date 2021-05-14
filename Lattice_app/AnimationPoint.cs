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
    public class AnimationPoint
    {
        public Point first_point = new Point();
        public Point second_point = new Point();
        private DispatcherTimer timet_for_move = new DispatcherTimer();
        private Ellipse main_point;
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
        public Canvas g;

        public AnimationPoint(Point first, Point second, Canvas canvas)
        {
            first_point = first;
            second_point = second;
            g = canvas;
            main_point = CreatePoint(first.X, first.Y, 8, Brushes.Green);
            g.Children.Add(main_point);
            k_x = first_point.X;
            k_y = first_point.Y;
            gran_x = second_point.X;
            gran_y = second_point.Y;

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
            timet_for_move.Tick += new EventHandler(timer_Move);
            timet_for_move.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timet_for_move.Start();
        }

        void timer_Move(object sender, EventArgs e)
        {
            StartAnimation();
        }

        private Ellipse CreatePoint(double x, double y, double thickness, Brush colour)
        {
            Point point = new Point(x, y);
            Ellipse elipse = new Ellipse();

            elipse.Width = thickness;
            elipse.Height = thickness;

            elipse.StrokeThickness = thickness / 2;
            elipse.Stroke = colour;
            elipse.Margin = new Thickness(point.X - thickness / 2, point.Y - thickness / 2, 0, 0);
            return elipse;
        }

        public void HidePoint()
        {
            main_point.Visibility = Visibility.Hidden;
        }
        public void VisiblePoint()
        {
            main_point.Visibility = Visibility.Visible;
        }
        public void StartAnimation()
        {
            if (koef_x == 1)
            {
                if (k_x <= gran_x)
                {
                    Canvas.SetLeft(main_point, k_a);
                    k_a++;
                    k_x += koef_x;
                }
                if (k_x <= gran_x && k_x > gran_x_x)
                {
                    k_a++;
                    k_x += koef_x;
                }
            }
            else if (koef_x == -1)
            {
                if (k_x > gran_x_x)
                {
                    Canvas.SetLeft(main_point, k_a);
                    k_a--;
                    k_x += koef_x;
                }
                if (k_x >= gran_x && k_x < gran_x_x)
                {
                    k_a--;
                    k_x += koef_x;
                }
            }
            if (koef_y == 1)
            {

                if (k_y <= gran_y)
                {
                    Canvas.SetTop(main_point, k_b);
                    k_b++;
                    k_y += koef_y;
                }
            }
            else if (koef_y == -1)
            {
                if (k_y >= gran_y)
                {
                    Canvas.SetTop(main_point, k_b);
                    k_b--;
                    k_y += koef_y;
                }
            }
        }
    }
}
