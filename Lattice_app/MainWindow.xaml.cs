using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Numerics;

namespace Lattice_app
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static double dist = 30;
        static double thickness_line = 0.4;

        Point point_start_v1;
        Point point_end_v1;

        Brush color_vector;

        //private static Dictionary<double, Point> points_X = new Dictionary<double, Point>();
        //private static Dictionary<double, Point> points_Y = new Dictionary<double, Point>();
        bool flag = false;

        private static List<Vector> vectors = new List<Vector>();
        private static List<System.Windows.Shapes.Path> vect_on_plane = new List<System.Windows.Shapes.Path>();
        private static List<Vector> digit_vectors = new List<Vector>();
        private static List<System.Windows.Shapes.Path> result_vectors = new List<System.Windows.Shapes.Path>();
        private static List<Ellipse> point_on_plane = new List<Ellipse>();
        private static List<Point> digit_point = new List<Point>();
        private static Dictionary<Vector, List<Point>> storage = new Dictionary<Vector, List<Point>>();
        private static Dictionary<Vector, List<Point>> moved_basis = new Dictionary<Vector, List<Point>>();

        CoordinatePlane main_coordinate_plane;
        CoordinatePlane lattice_plane;
        CreateLattice ex;
        Step_by_step steps;
        Step_by_step_Babai steps_babai;
        AnimationVector first_vector;
        AnimationVector second_vector;
        string result = "";
        private static List<Ellipse> ellipse_on_plane = new List<Ellipse>();

        public static List<PublicKey> publicKeys = new List<PublicKey>();
        public static List<BigInteger> get_primes(int n)
        {

            bool[] is_prime = new bool[n + 1];
            for (int i = 2; i <= n; ++i)
                is_prime[i] = true;

            List<BigInteger> primes = new List<BigInteger>();

            for (int i = 2; i <= n; ++i)
                if (is_prime[i])
                {
                    primes.Add((BigInteger)i);
                    if (i * i <= n)
                        for (int j = i * i; j <= n; j += i)
                            is_prime[j] = false;
                }

            return primes;
        }

        static BigInteger modInverse(BigInteger a, BigInteger m)
        {
            a = a % m;
            for (int x = 1; x < m; x++)
                if ((a * x) % m == 1)
                    return x;
            return 1;
        }
        public MainWindow()
        {
            InitializeComponent();
            CreateCoordinatePlane();
            Center_y1.SelectionChanged += OnSelectionChanged;
            Center_x1.SelectionChanged += OnSelectionChanged;
            Dimension.SelectionChanged += OnSelectionDimChanged;
            Dimension_Babai.SelectionChanged += OnSelectionDimChanged_Babai;
            Dimension.Items.Add(2);
            Dimension.Items.Add(3);
            Dimension.SelectedItem = 2;
            Dimension_Babai.Items.Add(2);
            Dimension_Babai.Items.Add(3);
            Dimension_Babai.SelectedItem = 2;
            Draw_Lattice.IsHitTestVisible = false;
            Draw_Points.IsChecked = true;
            Draw_CoordinatePlane.IsChecked = true;
            Draw_Coordinate_grid.IsChecked = true;
            foreach (var i in main_coordinate_plane.points_Y)
            {
                y1.Items.Add(i.Key);
                y2.Items.Add(i.Key);
                Point_y_Babai.Items.Add(i.Key);
                Center_y1.Items.Add(i.Key);
            }
            foreach (var i in main_coordinate_plane.points_X)
            {
                x1.Items.Add(i.Key);
                Point_x_Babai.Items.Add(i.Key);
                Center_x1.Items.Add(i.Key);
                x2.Items.Add(i.Key);
            }

        }
        #region static Methods
        enum ArrowType
        {
            None, Start, End, Both
        }
        private void CreateCoordinatePlane()
        {
            main_coordinate_plane = new CoordinatePlane(g, dist, thickness_line);
            main_coordinate_plane.CreateLattice();
            main_coordinate_plane.VisibleLattice();
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
        static Ellipse Create_point(double x, double y, Brush color, double thickness)
        {
            Point point = new Point();
            Ellipse elipse = new Ellipse();
            elipse.StrokeThickness = thickness/2;
            elipse.Stroke = color;
            point.X = x;
            point.Y = y;
            elipse.Width = thickness;
            elipse.Height = thickness;
            elipse.StrokeThickness = thickness / 2;
            elipse.Stroke = color;
            elipse.Margin = new Thickness(point.X - thickness / 2, point.Y - thickness / 2, 0, 0);

            return elipse;
        }

        Ellipse CreateEllipse(double width, double height, double desiredCenterX, double desiredCenterY)
        {
            Ellipse ellipse = new Ellipse { Width = width, Height = height };
            double left = desiredCenterX - (width / 2);
            double top = desiredCenterY - (height / 2);

            ellipse.Margin = new Thickness(left, top, 0, 0);
            ellipse.StrokeThickness = 2;
            ellipse.Stroke = Brushes.Red;
            ellipse_on_plane.Add(ellipse);
            return ellipse;
        }


        public void Add_vector(double startX, double startY, double endX, double endY, Brush brushes, bool add, bool file)
        {
            Vector v = new Vector(endX - startX, endY - startY);
            var vector = LineWithArrow(startX, startY, endX, endY, arrowType: ArrowType.End);
            vector.Stroke = brushes;
            if (!vectors.Contains(v))
            {
                //if (vectors.Count <= 1)
                    g.Children.Add(vector);
                if(brushes == Brushes.Green)
                {
                    result_vectors.Add(vector);
                }
                if (add)
                {
                    if (vectors.Count <= 1)
                    {
                        vectors.Add(v);
                        storage.Add(v, new List<Point>() { new Point(startX, startY), new Point(endX, endY) });
                        if (file is false)
                        digit_vectors.Add(new Vector(Convert.ToDouble(x2.SelectedValue.ToString()) - Convert.ToDouble(x1.SelectedValue.ToString()), Convert.ToDouble(y2.SelectedValue.ToString()) - Convert.ToDouble(y1.SelectedValue.ToString())));
                        vect_on_plane.Add(vector);
                        if (vectors.Count == 2)
                            Draw_Lattice.IsHitTestVisible = true;
                    }
                    else {                      
                        MessageBox.Show("You have already introduced the basis!");
                    }
                    //else
                    //if (vectors.Count == 1 )//&& Check_linear_independence(new int[2, 2] { { (int)vectors[0].X, (int)vectors[0].Y }, { (int)v.X, (int)v.Y } })
                    //{
                    //    vectors.Add(v);
                    //    vect_on_plane.Add(vector);
                    //}
                }
            }  else MessageBox.Show("This vector already exists on coordinate plane!");     
        }
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Center_x1.SelectedItem != null)
            {
                Width_e.Items.Clear();
                for (double i = 1; i < g.Width / dist / 2 - Math.Abs(Convert.ToDouble(Center_x1.SelectedValue.ToString())); i++)
                {
                    Width_e.Items.Add(i * 2);
                }
            }
            if (Center_y1.SelectedItem != null)
            {
                Height_e.Items.Clear();
                for (double i = 1; i < g.Height / dist / 2 - Math.Abs(Convert.ToDouble(Center_y1.SelectedValue.ToString())); i++)
                {
                    Height_e.Items.Add(i * 2);
                }
            }
        }
        private void OnSelectionDimChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Dimension.SelectedItem.ToString() == "2")
            {
                dim3.Visibility = Visibility.Hidden;
                dim2.Visibility = Visibility.Visible;
            }
            if (Dimension.SelectedItem.ToString() == "3")
            {
                dim2.Visibility = Visibility.Hidden;
                dim3.Visibility = Visibility.Visible;
            }
        }

        private void OnSelectionDimChanged_Babai(object sender, SelectionChangedEventArgs e)
        {
            if (Dimension_Babai.SelectedItem.ToString() == "2")
            {
                dim3_Babai.Visibility = Visibility.Hidden;
                dim2_Babai.Visibility = Visibility.Visible;
            }
            if (Dimension_Babai.SelectedItem.ToString() == "3")
            {
                dim2_Babai.Visibility = Visibility.Hidden;
                dim3_Babai.Visibility = Visibility.Visible;
            }
        }

        private void Draw_Coordinate_grid_Checked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.VisibleLattice();
        }
        private void Draw_Coordinate_grid_Unchecked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.HideLattice();
        }
        private void Draw_Points_Unchecked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.HidePoints();
        }
        private void Draw_Points_Checked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.VisiblePoints();
        }
        void MoveBasis()
        {
            Point point_v1 = new Point(storage.ElementAt(0).Value[1].X - (storage.ElementAt(0).Value[0].X - main_coordinate_plane.points_X[0].X), storage.ElementAt(0).Value[1].Y - (storage.ElementAt(0).Value[0].Y - main_coordinate_plane.points_Y[0].Y));
            Point point_v2 = new Point(storage.ElementAt(1).Value[1].X - (storage.ElementAt(1).Value[0].X - main_coordinate_plane.points_X[0].X), storage.ElementAt(1).Value[1].Y - (storage.ElementAt(1).Value[0].Y - main_coordinate_plane.points_Y[0].Y));
            first_vector = new AnimationVector(new Dictionary<Vector, List<Point>>() { { storage.ElementAt(0).Key, storage.ElementAt(0).Value } }, new Dictionary<Vector, List<Point>>() { { new Vector(point_v1.X - main_coordinate_plane.points_X[0].X, point_v1.Y - main_coordinate_plane.points_Y[0].Y), new List<Point>() { new Point(main_coordinate_plane.points_X[0].X, main_coordinate_plane.points_Y[0].Y), point_v1 } } }, main_coordinate_plane.g, false);
            first_vector.PrepareParameters();
            second_vector = new AnimationVector(new Dictionary<Vector, List<Point>>() { { storage.ElementAt(1).Key, storage.ElementAt(1).Value } }, new Dictionary<Vector, List<Point>>() { { new Vector(point_v2.X - main_coordinate_plane.points_X[0].X, point_v2.Y - main_coordinate_plane.points_Y[0].Y), new List<Point>() { new Point(main_coordinate_plane.points_X[0].X, main_coordinate_plane.points_Y[0].Y), point_v2 } } }, main_coordinate_plane.g, false);
            second_vector.PrepareParameters();
            foreach (var v in vect_on_plane)
                v.Visibility = Visibility.Hidden;
            moved_basis.Add(new Vector(point_v1.X - main_coordinate_plane.points_X[0].X, point_v1.Y - main_coordinate_plane.points_Y[0].Y), new List<Point>() { new Point(main_coordinate_plane.points_X[0].X, main_coordinate_plane.points_Y[0].Y), point_v1 });
            moved_basis.Add(new Vector(point_v2.X - main_coordinate_plane.points_X[0].X, point_v2.Y - main_coordinate_plane.points_Y[0].Y), new List<Point>() { new Point(main_coordinate_plane.points_X[0].X, main_coordinate_plane.points_Y[0].Y), point_v2 });
            ex = new CreateLattice(moved_basis, vect_on_plane, g, Brushes.Blue, false);
            ex.DrawLattice();
            lattice_plane = new CoordinatePlane(g, 100, 1);
            lattice_plane.horizontal_lines = ex.horiz_lines_on_plane;
            lattice_plane.vertical_lines = ex.vert_lines_on_plane;
            flag = true;
            Draw_Lattice.IsChecked = true;
            lattice_plane.VisibleLattice();
            Draw_CoordinatePlane_Unchecked();
            //LLL_Calculate.Visibility = Visibility.Hidden;
            //Next_step.Visibility = Visibility.Visible;
        }
        private void Draw_Lattices_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (storage.Count == 2 && flag is false)
                {
                    MoveBasis();                 
                }
                if (storage.Count == 2 && flag is true)
                { lattice_plane.VisibleLattice();
                    Draw_CoordinatePlane_Unchecked();
                    first_vector.VisibleVector();
                    second_vector.VisibleVector();
                }
            }
            catch
            {
                MessageBox.Show("Input basis, please!");
            }
           
        }
        private void Draw_Lattices_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (storage.Count == 2)
                {
                    lattice_plane.HideLattice();
                    Draw_Lattice.IsHitTestVisible = true;
                    first_vector.HideVector();
                    second_vector.HideVector();
                }
                else
                {
                    MessageBox.Show("Input basis, please!");
                }             
            }
            catch
            {
                MessageBox.Show("Input basis, please!");
            }
        }

        private void Draw_CoordinatePlane_Checked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.VisibleCoordinatePlane();
            Draw_Points.IsChecked = true;
            Draw_Coordinate_grid.IsChecked = true;
        }
        private void Draw_CoordinatePlane_Unchecked(object sender, RoutedEventArgs e)
        {
            main_coordinate_plane.HideCoordinatePlane();
            Draw_Points.IsChecked = false;
            Draw_Coordinate_grid.IsChecked = false;
        }

        private void Draw_CoordinatePlane_Unchecked()
        {
            main_coordinate_plane.HideCoordinatePlane();
            Draw_Points.IsChecked = false;
            Draw_Coordinate_grid.IsChecked = false;
            Draw_CoordinatePlane.IsChecked = false;
        }

        public void ClearAll()
        {
            foreach (var v in vect_on_plane)
            {
                g.Children.Remove(v);
            }
            foreach(var v in result_vectors)
            {
                g.Children.Remove(v);
            }
            foreach (var p in point_on_plane)
            {
                g.Children.Remove(p);
            }
            Draw_Lattice.IsHitTestVisible = false;
            Draw_Points.IsChecked = true;
            Draw_CoordinatePlane.IsChecked = true;
            Draw_Coordinate_grid.IsChecked = true;
            Next_step.Visibility = Visibility.Hidden;
            if (!Load_points_for_Babai.IsVisible)
            {
                LLL_Calculate.Visibility = Visibility.Visible;
                Next_step.Visibility = Visibility.Hidden;
            }
            else { 
            Babai_Calculate.Visibility = Visibility.Visible;
            Next_step_Babai.Visibility = Visibility.Hidden;}
            main_coordinate_plane.VisibleLattice();
            Text_results.Content = "Open results";
            result ="";
            flag = false;
            vectors.Clear();
            point_on_plane.Clear();
            storage.Clear();
            result_vectors.Clear();
            vect_on_plane.Clear();
            digit_vectors.Clear();
            digit_point.Clear();
            Result.Clear();
            Result_Babai.Clear();
            //Hastad_result.Clear();
            //Hastad_message.Clear();
            moved_basis.Clear();
            e1.Clear();
        }

        #endregion  


        #region Click Events  
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
           Choose_algorithm.Visibility = Visibility.Visible;
           Graph.Visibility = Visibility.Hidden;
           Babai_point.Visibility = Visibility.Hidden;
           LLL_Calculate.Visibility = Visibility.Hidden;
           Load_points_for_LLL.Visibility = Visibility.Hidden;
           Text_results.Visibility = Visibility.Hidden;
           Input_vectors.Visibility = Visibility.Hidden;
           ClearAll();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            g.Children.Clear();
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                point_start_v1 = new Point(Convert.ToDouble(x1.SelectedValue.ToString()), Convert.ToDouble(y1.SelectedValue.ToString()));
                point_end_v1 = new Point(Convert.ToDouble(x2.SelectedValue.ToString()), Convert.ToDouble(y2.SelectedValue.ToString()));
                Add_vector(main_coordinate_plane.points_X[point_start_v1.X].X, main_coordinate_plane.points_Y[point_start_v1.Y].Y, main_coordinate_plane.points_X[point_end_v1.X].X, main_coordinate_plane.points_Y[point_end_v1.Y].Y, Brushes.Red, true, false);
            }
            catch
            {
                MessageBox.Show("Input coordinates, please!");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                g.Children.Remove(vect_on_plane[vect_on_plane.Count - 1]);
                vect_on_plane.Remove(vect_on_plane[vect_on_plane.Count - 1]);
                vectors.Remove(vectors[vectors.Count - 1]);
                digit_vectors.Remove(digit_vectors[digit_vectors.Count - 1]);
                storage.Remove(storage.ElementAt(storage.Count - 1).Key);
                Draw_Lattice.IsHitTestVisible = false;
            }
            catch
            {
                MessageBox.Show("Input at least one vector, please!");
            }
        }

        private void Delete_Ellipse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                g.Children.Remove(ellipse_on_plane[ellipse_on_plane.Count - 1]);
                ellipse_on_plane.Remove(ellipse_on_plane[ellipse_on_plane.Count - 1]);
            }
            catch
            {
                MessageBox.Show("Input at least one ellipse, please!");
            }
        }
        private void Draw_Ellipse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double center_x = main_coordinate_plane.points_X[Convert.ToDouble(Center_x1.SelectedValue.ToString())].X;
                double center_y = main_coordinate_plane.points_Y[Convert.ToDouble(Center_y1.SelectedValue.ToString())].Y;
                double width = Convert.ToDouble(Width_e.SelectedValue.ToString()) * dist;
                double height = Convert.ToDouble(Height_e.SelectedValue.ToString()) * dist;
                g.Children.Add(CreateEllipse(width, height, center_x, center_y));
            }
            catch
            {
                MessageBox.Show("Input data for ellipse, please!");
            }
        }
        private void Visual_app_Click(object sender, RoutedEventArgs e)
        {
            Menu.Visibility = Visibility.Hidden;
            Choose_algorithm.Visibility = Visibility.Visible;
        }

        private void LLL_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            g.Children.Clear();
            CreateCoordinatePlane();
            Graph.Visibility = Visibility.Visible;
            LLL_Calculate.Visibility = Visibility.Visible;
            Load_points_for_LLL.Visibility = Visibility.Visible;
            Text_results.Visibility = Visibility.Visible;
            Input_vectors.Visibility = Visibility.Visible;
            Choose_algorithm.Visibility = Visibility.Hidden;
            Draw_Lattice.IsHitTestVisible = false;
            Draw_Points.IsChecked = true;
            Draw_CoordinatePlane.IsChecked = true;
            Draw_Coordinate_grid.IsChecked = true;
        }

        private void Back_menu_Click(object sender, RoutedEventArgs e)
        {
            Choose_algorithm.Visibility = Visibility.Hidden;
            Menu.Visibility = Visibility.Visible;
        }

        private void Grid_LLL_calculating_Click(object sender, RoutedEventArgs e)
        {
            Choose_algorithm_for_calculating.Visibility = Visibility.Hidden;
            Calculating_LLL.Visibility = Visibility.Visible;
        }

        private void Grid_Back_menu_Click(object sender, RoutedEventArgs e)
        {
            Choose_algorithm_for_calculating.Visibility = Visibility.Hidden;
            Menu.Visibility = Visibility.Visible;
        }

        private void Digital_Click(object sender, RoutedEventArgs e)
        {
            Menu.Visibility = Visibility.Hidden;
            Choose_algorithm_for_calculating.Visibility = Visibility.Visible;
        }

        private void Back_Choose_alg_Click(object sender, RoutedEventArgs e)
        {
            Calculating_Babai.Visibility = Visibility.Hidden;
            Choose_algorithm_for_calculating.Visibility = Visibility.Visible;
            Calculating_LLL.Visibility = Visibility.Hidden;
            ClearAll();
        }

        private void Clear_result_Click(object sender, RoutedEventArgs e)
        {
            Result.Clear();
            Result_Babai.Clear();
        }

        private void LLL_Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flag is false && storage.Count == 2)
                {
                    MoveBasis();
                }
                List<Vector_n> vectors_n = new List<Vector_n>() { new Vector_n(new List<double>() { digit_vectors[0].X, digit_vectors[0].Y }), new Vector_n(new List<double>() { digit_vectors[1].X, digit_vectors[1].Y }) };
                LLL gram = new LLL(vectors_n);
                bool coord = false;
                var new_basis = gram.prob_LLL_alg(vectors_n, true);
                foreach (var p in new_basis[0])
                    if (main_coordinate_plane.points_X.ContainsKey(p[0]) && main_coordinate_plane.points_Y.ContainsKey(p[1]))
                        coord = true;
                if (coord)
                {
                    result += gram.LLL_alg_str(vectors_n);
                    LLL_Calculate.Visibility = Visibility.Hidden;
                    Next_step.Visibility = Visibility.Visible;
                    steps = new Step_by_step(moved_basis, new_basis[0], main_coordinate_plane.g, dist, thickness_line, vect_on_plane);
                    Next_step.Content = "Next step " + "(" + steps.status.ToString() + "/" + steps.count.ToString() + ")";
                }
                else MessageBox.Show("Result vectors's coordinates in LLL-algorithm  are out of the plane!");
            }
            catch
            {
                MessageBox.Show("Input basis, please!");
            }
         }

        private Vector Find_coord(Vector v1, Vector v2)
        {
            Vector v_0 = new Vector();
            double d = v2.Length;
            double c = d / v1.Length;
            v_0 = new Vector(c * v1.X, c * v1.Y);
            return v_0;
        }

        private void Exit_form_app_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Calculate_LLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Dimension.SelectedItem.ToString() == "2")
                {             
                    List<Vector_n> vectors_2 = new List<Vector_n>() { new Vector_n(new List<double>() { Convert.ToDouble(v1_x.Text), Convert.ToDouble(v1_y.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v2_x.Text), Convert.ToDouble(v2_y.Text) }) };
                    LLL gram = new LLL(vectors_2);
                    var v = gram.LLL_alg(vectors_2, false);
                    Result.Text += "Input: \n";
                    foreach (var vec in vectors_2)
                        Result.Text += vec.ToString()+"\n";
                    Result.Text += "Output: \n";
                    foreach (var vec in v[1])
                        Result.Text += vec.ToString() + "\n";
                }
                if (Dimension.SelectedItem.ToString() == "3")
                {                
                    List<Vector_n> vectors_3 = new List<Vector_n>() { new Vector_n(new List<double>() { Convert.ToDouble(v1_x3.Text), Convert.ToDouble(v1_y3.Text), Convert.ToDouble(v1_z3.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v2_x3.Text), Convert.ToDouble(v2_y3.Text), Convert.ToDouble(v2_z3.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v3_x3.Text), Convert.ToDouble(v3_y3.Text), Convert.ToDouble(v3_z3.Text) }) };
                    LLL gram = new LLL(vectors_3);
                    var v = gram.LLL_alg(vectors_3, false);
                    Result.Text += "Input: \n";
                    foreach (var vec in vectors_3)
                        Result.Text += vec.ToString() + "\n";
                    Result.Text += "Output: \n";
                    foreach (var vec in v[1])
                        Result.Text += vec.ToString() + "\n";
                }
            }
            catch
            {
                MessageBox.Show("Input data correctly!");
            }
        }
        #endregion

        private void Calculate_Babai_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Dimension_Babai.SelectedItem.ToString() == "2")
                {
                    List<Vector_n> vectors_2 = new List<Vector_n>() { new Vector_n(new List<double>() { Convert.ToDouble(v1_x_Babai_2.Text), Convert.ToDouble(v1_y_Babai_2.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v2_x_Babai_2.Text), Convert.ToDouble(v2_y_Babai_2.Text) }) };
                    Point_n point = new Point_n(new List<double>() { Convert.ToDouble(point_x_Babai_2.Text), Convert.ToDouble(point_y_Babai_2.Text) });
                    Result_Babai.Text += "Input: \n";
                    foreach (var vec in vectors_2)
                        Result_Babai.Text += vec.ToString() + "\n";
                    Result_Babai.Text += point.ToString() + "\n";
                    Result_Babai.Text += "Output: \n";
                    Result_Babai.Text += new LLL(new List<Vector_n>()).Babai_alg(vectors_2, point).ToString();                  
                }
                if (Dimension_Babai.SelectedItem.ToString() == "3")
                {
                    List<Vector_n> vectors_3 = new List<Vector_n>() { new Vector_n(new List<double>() { Convert.ToDouble(v1_x_Babai.Text), Convert.ToDouble(v1_y_Babai.Text), Convert.ToDouble(v1_z_Babai.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v2_x_Babai.Text), Convert.ToDouble(v2_y_Babai.Text), Convert.ToDouble(v2_z_Babai.Text) }), new Vector_n(new List<double>() { Convert.ToDouble(v3_x_Babai.Text), Convert.ToDouble(v3_y_Babai.Text), Convert.ToDouble(v3_z_Babai.Text) }) };
                    Point_n point = new Point_n(new List<double>() { Convert.ToDouble(point_x_Babai.Text), Convert.ToDouble(point_y_Babai.Text), Convert.ToDouble(point_z_Babai.Text) });
                    Result_Babai.Text += "Input: \n";
                    foreach (var vec in vectors_3)
                        Result_Babai.Text += vec.ToString() + "\n";
                    Result_Babai.Text +="Point: " + point.ToString() + "\n";
                    Result_Babai.Text += "Output: \n";
                    Result_Babai.Text += new LLL(new List<Vector_n>()).Babai_alg(vectors_3, point).ToString();
                }
            }
            catch
            {
                MessageBox.Show("Input data correctly!");
            }
        }

        private void Babai_algorithm_Click(object sender, RoutedEventArgs e)
        {
            Choose_algorithm_for_calculating.Visibility = Visibility.Hidden;
            Calculating_Babai.Visibility = Visibility.Visible;
           
        }

        private void Add_point_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (point_on_plane.Count == 0)
                {
                    g.Children.Add(Create_point(main_coordinate_plane.points_X[Convert.ToDouble(Point_x_Babai.SelectedValue.ToString())].X, main_coordinate_plane.points_Y[Convert.ToDouble(Point_y_Babai.SelectedValue.ToString())].Y, Brushes.Red,6));
                    point_on_plane.Add(Create_point(main_coordinate_plane.points_X[Convert.ToDouble(Point_x_Babai.SelectedValue.ToString())].X, main_coordinate_plane.points_Y[Convert.ToDouble(Point_y_Babai.SelectedValue.ToString())].Y, Brushes.Red,6));
                    digit_point.Add(new Point(Convert.ToDouble(Point_x_Babai.SelectedValue.ToString()), Convert.ToDouble(Point_y_Babai.SelectedValue.ToString())));
                }
                else MessageBox.Show("You have already drawn a point!");
            }
            catch
            {
                MessageBox.Show("Input correct point's coordinates!");
            }
        }

        private void Delete_point_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                g.Children.Remove(point_on_plane[0]);
                point_on_plane.Clear();
            }
            catch
            {
                MessageBox.Show("Add one point, please!");
            }
        }

        private void Babai_alg_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            g.Children.Clear();
            CreateCoordinatePlane();
            Graph.Visibility = Visibility.Visible;
            Next_step_Babai.Visibility = Visibility.Hidden;
            Babai_Calculate.Visibility = Visibility.Visible;
            Input_vectors.Visibility = Visibility.Visible;
            LLL_Calculate.Visibility = Visibility.Hidden;
            Babai_point.Visibility = Visibility.Visible;
            Choose_algorithm.Visibility = Visibility.Hidden;
            Draw_Lattice.IsHitTestVisible = false;
            Draw_Points.IsChecked = true;
            Draw_CoordinatePlane.IsChecked = true;
            Draw_Coordinate_grid.IsChecked = true;
        }

        private void Load_for_LLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                List<Vector_n> vctrs = new List<Vector_n>();
                
                ofd.ShowDialog();
                if (ofd.FileName != "")
                {
                    
                    StreamReader sr = new StreamReader(ofd.FileName);
                    while (!sr.EndOfStream)
                    {
                       int count_vectors = Convert.ToInt32(sr.ReadLine());

                        for (int i = 0; i < count_vectors; i++)
                        {
                            vctrs.Add(new Vector_n(sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList()));
                        }
                    }
                    int count_vector = vctrs.Count;
                    LLL gram = new LLL(vctrs);
                    var v = gram.LLL_alg_str(vctrs);
                    Result.Text += "Input: \n";
                    foreach (var vec in vctrs)
                        Result.Text += vec.ToString() + "\n";
                    Result.Text += "Output: \n";
                    Result.Text += v;
                }
                else MessageBox.Show("You haven't selected a file!");
        }
            catch
            {
                MessageBox.Show("Incorrect input in file!");
            }
}

        private void Load_for_Babai_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                List<Vector_n> vctrs = new List<Vector_n>();
                Point_n point = new Point_n(new List<double>());
                ofd.ShowDialog();
                if (ofd.FileName != "")
                {
                    StreamReader sr = new StreamReader(ofd.FileName);
                    while (!sr.EndOfStream)
                    {
                        int count_vectors = Convert.ToInt32(sr.ReadLine());

                        for (int i = 0; i < count_vectors - 1; i++)
                        {
                            vctrs.Add(new Vector_n(sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList()));
                        }
                        point = new Point_n(sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList());
                    }
                    Result_Babai.Text += "Input: \n";
                    foreach (var vec in vctrs)
                        Result_Babai.Text += vec.ToString() + "\n";
                    Result_Babai.Text += point.ToString() + "\n";
                    Result_Babai.Text += "Output: \n";
                    Result_Babai.Text += new LLL(new List<Vector_n>()).Babai_alg_str(vctrs, point);
                }
                else MessageBox.Show("You haven't selected a file!");
            }
            catch
            {
                MessageBox.Show("Incorrect input in file!");
            }
        }

        private void Babai_Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flag is false && storage.Count == 2)
                {
                    MoveBasis();
                }
                List<Vector_n> vectors_n = new List<Vector_n>() { new Vector_n(new List<double>() { digit_vectors[0].X, digit_vectors[0].Y }), new Vector_n(new List<double>() { digit_vectors[1].X, digit_vectors[1].Y }) };
                Point_n point = new Point_n(new List<double>() { digit_point[0].X, digit_point[0].Y });
                Point input = new Point(digit_point[0].X, digit_point[0].Y);
                LLL gram = new LLL(vectors_n);
                var new_basis = gram.LLL_alg(vectors_n, false);
                bool coord = true;
               
            if (main_coordinate_plane.points_X.ContainsKey(new_basis[0][0].ToList()[0]) && main_coordinate_plane.points_Y.ContainsKey(new_basis[0][0].ToList()[1]) && main_coordinate_plane.points_X.ContainsKey(new_basis[0][1].ToList()[0]) && main_coordinate_plane.points_Y.ContainsKey(new_basis[0][1].ToList()[1]))
            {
                steps = new Step_by_step(moved_basis, new_basis[0], main_coordinate_plane.g, dist, thickness_line, vect_on_plane);
                var v1 = new LLL(new List<Vector_n>()).prob_Babai_alg(vectors_n, point);
                    foreach (var p in v1)
                        if (main_coordinate_plane.points_X.ContainsKey(p[0]) && main_coordinate_plane.points_Y.ContainsKey(p[1]))
                            coord = true;
                        else coord = false;
                result += new LLL(new List<Vector_n>()).Babai_alg_str(vectors_n, point);
                if (coord)
                {
                    Babai_Calculate.Visibility = Visibility.Hidden;
                    Next_step_Babai.Visibility = Visibility.Visible;
                    steps_babai = new Step_by_step_Babai(v1, input, main_coordinate_plane.g, dist, thickness_line, vect_on_plane);
                    Next_step_Babai.Content = "Next step " + "(" + steps_babai.status.ToString() + "/" + steps_babai.count.ToString() + ")";
                }
                else MessageBox.Show("Result points's coordinates are out of the plane!");
            } else MessageBox.Show("Result vectors's coordinates in LLL-algorithm  are out of the plane!");
            }
            catch
            {
                MessageBox.Show("Incorrect input!");
            }
}

        private void Clear_coordinate_plane_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            g.Children.Clear();
            CreateCoordinatePlane();
        }

        private void Execute_Hastad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var publicE = Convert.ToInt32(e1.Text);
                var listPrimes = get_primes(2000);
                listPrimes.Reverse();
                var listP = listPrimes.Skip(0).Take((listPrimes.Count / 2) - 0).ToList();
                var listQ = listPrimes.Skip(listPrimes.Count / 2).Take(listPrimes.Count / 2).ToList();
                int counter = 0;
                for (var i = 0; i < listP.Count; i++)
                {
                    if (((listP[i] - 1) * (listQ[i] - 1)) % publicE != 0)
                    {
                        counter++;
                        publicKeys.Add(new PublicKey(listP[i] * listQ[i], publicE));
                        if (counter == publicE)
                        {
                            break;
                        }
                    }
                }
                var message = BigInteger.Parse(Hastad_message.Text);
                counter = 0;

                foreach (var pk in publicKeys)
                {
                    counter++;
                    pk.C = BigInteger.ModPow(message, pk.e, pk.n);
                    Hastad_result.AppendText($"{counter}.< c= {pk.C}  = m^{pk.e} (mod n={pk.n}); >\n");
                }
                BigInteger commonN = 1;
                foreach (var pk in publicKeys)
                {
                    commonN *= pk.n;
                }
                Hastad_result.Text += "n1*n2*n3 = " + commonN.ToString() + "\n";
                Hastad_result.Text += "\n";
                foreach (var pk in publicKeys)
                {
                    pk.Ni = commonN / pk.n;
                    pk.Reverse_in_the_ring = modInverse(pk.Ni, pk.n);
                    pk.Xi = pk.Ni * pk.Reverse_in_the_ring * pk.C;

                }

                BigInteger Z = 0;
                foreach (var pk in publicKeys)
                {
                    Z += pk.Xi;
                }

                Z = Z % commonN;

                var openMessage = Math.Round(Math.Pow(Math.E, BigInteger.Log(Z) / (int)publicE));
                Hastad_result.Text += "\n" + "Hacked message m: " + openMessage.ToString();
                publicKeys = new List<PublicKey>();
            }
            catch
            {
                MessageBox.Show("Input correct number!");
            }
        }

        private void Exit_Hastad_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            Calculating_Hastad.Visibility = Visibility.Hidden;
            Choose_algorithm_for_calculating.Visibility = Visibility.Visible;
        }

        private void Clear_result_Hastad_Click(object sender, RoutedEventArgs e)
        {
            Hastad_message.Clear();
            e1.Clear();
            Hastad_result.Clear();
        }

        private void Hastad_algorithm_Click(object sender, RoutedEventArgs e)
        {
            Calculating_Hastad.Visibility = Visibility.Visible;
            Choose_algorithm_for_calculating.Visibility = Visibility.Hidden;
        }

        private void Load_points_for_LLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (storage.Count == 0)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    List<Point> points = new List<Point>();
                    ofd.ShowDialog();
                    if (ofd.FileName != "")
                    {
                        StreamReader sr = new StreamReader(ofd.FileName);
                        int count_vectors = Convert.ToInt32(sr.ReadLine());               
                        for (int i = 0; i < count_vectors; i++)
                        {
                            List<double> a = sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList();
                            points.Add(new Point(a[0],a[1]));
                        }                       
                        Add_vector(main_coordinate_plane.points_X[points[0].X].X, main_coordinate_plane.points_Y[points[0].Y].Y, main_coordinate_plane.points_X[points[1].X].X, main_coordinate_plane.points_Y[points[1].Y].Y, Brushes.Red, true, true);
                        digit_vectors.Add(new Vector(points[1].X - points[0].X, points[1].Y - points[0].Y));
                        Add_vector(main_coordinate_plane.points_X[points[2].X].X, main_coordinate_plane.points_Y[points[2].Y].Y, main_coordinate_plane.points_X[points[3].X].X, main_coordinate_plane.points_Y[points[3].Y].Y, Brushes.Red, true, true);
                        digit_vectors.Add(new Vector(points[3].X - points[2].X, points[3].Y - points[2].Y));
                }
                    else MessageBox.Show("You haven't selected a file!");
                }
                else MessageBox.Show("You have already entered the vector(s)!");
            }
            catch
            {
                MessageBox.Show("Incorrect input in file!");
            }
}

        private void Load_points_for_Babai_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (storage.Count == 0)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    List<Point> points = new List<Point>();
                    ofd.ShowDialog();
                    if (ofd.FileName != "")
                    {
                        StreamReader sr = new StreamReader(ofd.FileName);
                        int count_vectors = Convert.ToInt32(sr.ReadLine());
                        for (int i = 0; i < count_vectors-1; i++)
                        {
                            List<double> a = sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList();
                            points.Add(new Point(a[0], a[1]));
                        }
                        List<double> b = sr.ReadLine().Trim().Split(' ').Select(x => double.Parse(x.Trim())).ToList();
                        if (point_on_plane.Count == 0)
                        {
                            Ellipse p = Create_point(main_coordinate_plane.points_X[b[0]].X, main_coordinate_plane.points_Y[b[1]].Y, Brushes.Red, 6);
                            Point point = new Point(b[0], b[1]);
                            g.Children.Add(p);
                            point_on_plane.Add(p);
                            digit_point.Add(point);
                        }
                        else MessageBox.Show("You have already drawn a point!");
                        Add_vector(main_coordinate_plane.points_X[points[0].X].X, main_coordinate_plane.points_Y[points[0].Y].Y, main_coordinate_plane.points_X[points[1].X].X, main_coordinate_plane.points_Y[points[1].Y].Y, Brushes.Red, true, true);
                        digit_vectors.Add(new Vector(points[1].X - points[0].X, points[1].Y - points[0].Y));
                        Add_vector(main_coordinate_plane.points_X[points[2].X].X, main_coordinate_plane.points_Y[points[2].Y].Y, main_coordinate_plane.points_X[points[3].X].X, main_coordinate_plane.points_Y[points[3].Y].Y, Brushes.Red, true, true);
                        digit_vectors.Add(new Vector(points[3].X - points[2].X, points[3].Y - points[2].Y));
                    }
                    else MessageBox.Show("You haven't selected a file!");
                }
                else MessageBox.Show("You have already entered the vector(s)!");
        }
            catch
            {
                MessageBox.Show("Incorrect input in file!");
            }
}

        private void Next_step_Click(object sender, RoutedEventArgs e)
        {
            Draw_Lattice.IsChecked = false;
            first_vector.HideVector();
            second_vector.HideVector();
            if (steps.status < steps.count)
            {              
                ex.HideLattice();
                steps.NextStep();
                Next_step.Content = "Next step " + "(" + steps.status.ToString() + "/" + steps.count.ToString() + ")";
            }
            else MessageBox.Show("LLL-algorithm ended!");
        }

        private void Next_step_Babai_Click(object sender, RoutedEventArgs e)
        {
            Draw_Lattice.IsChecked = false;
            first_vector.HideVector();
            second_vector.HideVector();
            if (steps_babai.status < steps_babai.count)
            {
                if (steps_babai.status == 0)
                {
                    steps.NextStep();
                    ex.HideLattice();
                }
                steps_babai.NextStep();
                Next_step_Babai.Content = "Next step " + "(" + steps_babai.status.ToString() + "/" + steps_babai.count.ToString() + ")";
            }
            else MessageBox.Show("Babai's algorithm ended!");
        }

        private void Text_results_Click(object sender, RoutedEventArgs e)
        {
            //if(Load_points_for_LLL.IsVisible)
            //{
            //if(Text_results.Content == "Open Results")
            //{
            //    Input_vectors.Visibility = Visibility.Hidden;
            //    Text_result.Visibility = Visibility.Visible;
            //    Text_results.Content = "Close Results";
            //}
            //else
            //{
            //    Input_vectors.Visibility = Visibility.Visible;
            //    Text_result.Visibility = Visibility.Hidden;
            //    Text_results.Content = "Open Results";
            //}
            if (result.Count() != 0 )
            MessageBox.Show(result);
            else MessageBox.Show("There is no results yet!");
            //}
            //else 
            //{

            //}
        }
    }
}



