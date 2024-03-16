using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Линии
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum Objects { point, line }

    public partial class MainWindow : Window
    {
        
        Objects CurBrush;
        List<Object> particls = new();
        List<PointCharge> Charges = new();
        public MainWindow()
        {
            InitializeComponent();
            CbxObjectType.ItemsSource = Enum.GetValues(typeof(Objects));
            CurBrush = (Objects)CbxObjectType.SelectedItem;

        }

        void Rastr(double x, double y, double h, double val)
        {
            List<Point> points = new List<Point>();

            Point point = new Point();

            if (OnLine(x, x + h, y, y, h, val, out point))
            {
                points.Add(point);
            }

            if (OnLine(x + h, x + h, y, y + h, h, val, out point))
            {
                points.Add(point);
            }

            if (OnLine(x, x, y, y + h, h, val, out point))
            {
                points.Add(point);
            }
            if (OnLine(x, x + h, y + h, y + h, h, val, out point))
            {
                points.Add(point);
            }
            if (points.Count == 2)
            {
                Line line = new Line();
                double a = 1;
                line.X1 = points[0].X /a;
                line.Y1 = points[0].Y /a - 150;
                line.X2 = points[1].X /a;
                line.Y2 = points[1].Y /a - 150;
                line.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                GridField.Children.Add(line);
            }
        }
        
        bool OnLine(double x1, double x2, double y1, double y2,double h, double v, out Point p)
        {
            double min = Math.Min(func(x1, y1), func(x2, y2));
            double max = Math.Max(func(x1, y1), func(x2, y2));
            if(x1 != x2)
                p.X = x1 + h *(v - min)/(max - min);
            else
                p.X = x1;
            if(y2 != y1)
                p.Y = y1 + h *(v - min)/(max - min);
            else
                p.Y= y1;
            return (v <= max && v > min);
        }

        double func(double x, double y)
        {
            double k = 8.987551787e9;
            double Field = 0;
            foreach(PointCharge charge in Charges)
            {

                double r = Math.Sqrt(Math.Pow((x - charge.position.X), 2) + Math.Pow((y - charge.position.Y), 2));
                Field += charge.charge * k / (r * r);
            }
            return Field;
        }


        private void GridField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mouse = e.GetPosition((IInputElement)this);
            OutLb.Content = mouse.ToString();
            if (!Double.TryParse(TbxCharge.Text, out double charge))
                return;
            switch (CurBrush)
            {
                case Objects.point:
                    particls.Add(new PointCharge(mouse, charge));
                    Charges.Add(new PointCharge(mouse, charge));
                    Ellipse a = new();
                    a.RenderTransform = new TranslateTransform { X = mouse.X - 600, Y = mouse.Y - 400};
                    a.Margin = new();
                    a.StrokeThickness = 0.5;
                    a.Height = 10;
                    a.Width = 10;

                    if(charge > 0) { a.Fill = Brushes.Red; }
                    else a.Fill = Brushes.Blue;

                    GridField.Children.Add(a);
                        break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<UIElement> ellipsesToRemove = new List<UIElement>();
            foreach (UIElement element in GridField.Children)
            {
                if (element is Line)
                {
                    ellipsesToRemove.Add(element as Line);
                }
            }

            foreach (UIElement ellipse in ellipsesToRemove)
            {
                GridField.Children.Remove(ellipse);
            }

            PointCharge test = Charges[0];
            List<double> values = new();

            if (!double.TryParse(TbxLInesCount.Text, out double CountLines))
                return;

            if (!double.TryParse(TbxLInesCount_Копировать.Text, out double LinesH))
                return;

            for (int i = (int)LinesH; i < CountLines * LinesH; i += (int)LinesH)
                values.Add(func(test.position.X + i, test.position.Y));

            double h = 2;
            if(!double.TryParse(TbxEps.Text,out h))
            {
                return;
            }

            for (double i = 0; i < 1200/h; i++)
                for (double j = 0; j < 800/h; j++)
                {
                    foreach (var val in values)
                    {
                        Rastr(0 + h * i, 0 + h * j, h, val);

                        Rastr(0 + h * i, 0 + h * j, h, -val);
                    }
                }
        }

        private void BtnReMoveLast_Click(object sender, RoutedEventArgs e)
        {
            if(Charges.Count == 0) return;

            Charges.RemoveAt(Charges.Count - 1);

            List<UIElement> asd = new List<UIElement>();
            foreach (var val in GridField.Children)
            {
                if (val is Ellipse)
                    asd.Add(val as Ellipse);
            }
            GridField.Children.Remove(asd[GridField.Children.Count - 1]);
        }

        private void TbnClear_Click(object sender, RoutedEventArgs e)
        {
            Charges.Clear();
            GridField.Children.Clear();
        }
    }

    public struct PointCharge
    {
        public Point position;
        public double charge;

        public PointCharge(Point position, double charge)
        { this.position = position; this.charge = charge;}
    }

}