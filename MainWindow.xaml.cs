using System;
using System.Reflection;
using System.Runtime.InteropServices;
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
        List<ChargeObject> Charges = new();
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
                line.Y1 = points[0].Y /a - 130;
                line.X2 = points[1].X /a;
                line.Y2 = points[1].Y /a - 130;
                line.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                GridField.Children.Add(line);
            }
        }
        
        bool OnLine(double x1, double x2, double y1, double y2,double h, double v, out Point p)
        {
            double min = Math.Min(func(x1, y1), func(x2, y2));
            double max = Math.Max(func(x1, y1), func(x2, y2));
            if(x1 != x2)
                if (func(x1,y1) < func(x2,y2))
                    p.X = x1 + h *(v - min)/(max - min);
                else
                    p.X = x1 + h * ( 1 - (v - min) / (max - min));
            else
                p.X = x1;

            if(y2 != y1)
                if (func(x1, y1) > func(x2, y2))
                    p.Y= y1 + h *(1 - (v - min) / (max - min));
                else
                    p.Y = y1 + h * ( (v - min) / (max - min));

            else
                p.Y= y1;
                return (v <= max && v > min);
        }

        double func(double x, double y)
        {
            double Field = 0;
            foreach(ChargeObject charge in Charges)
            {
                Field += charge.GetField(x, y);
            }
            return Field;
        }

        double _func(double x, double y)
        {
            return x - 2 * y;
        }

        private void GridField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mouse = e.GetPosition((IInputElement)this);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                MessageBox.Show("ПКМ");
                double value = func(mouse.X, mouse.Y);
                if (!double.TryParse(TbxEps.Text, out double h))
                    return;
                Rastr(mouse.X, mouse.Y, h, value);
            }
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

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
            if (Charges.Count == 0) return;

            //Мусорка для старых линий
            List<UIElement> LinesToRemove = new List<UIElement>();

            //Ещем все объекты, которые линии
            foreach (UIElement element in GridField.Children)
            {
                if (element is Line)
                {
                    LinesToRemove.Add(element as Line);
                }
            }

            //Удаляем прошлые линии
            foreach (UIElement Line in LinesToRemove)
            {
                GridField.Children.Remove(Line);
            }

            List<double> values = new();

            if (!double.TryParse(TbxMinFieldValue.Text, out double MinField))
                return;

            if (!double.TryParse(TbxMaxFieldValue.Text, out double MaxField))
                return;

            if (!double.TryParse(TbxLInesCount.Text, out double LinesCount))
                return;

            for (int i = 0; i < LinesCount; i++ )// (MaxField / LinesCount))
            {
                //MessageBox.Show($"{i}");
                values.Add(MaxField / LinesCount * i);
            }
            if(!double.TryParse(TbxEps.Text,out double h))
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
            GridField.Children.Remove(asd[asd.Count - 1]);
        }

        private void TbnClear_Click(object sender, RoutedEventArgs e)
        {
            Charges.Clear();
            GridField.Children.Clear();
        }
    }

    public abstract class ChargeObject
    {
        protected ChargeObject(Point position, double charge)
        {
            Position = position;
            Charge = charge;
        }

        public double Charge { get; set; }
        public Point Position { get; set; }

        public abstract double GetField(double x, double y);
    }
    public class PointCharge : ChargeObject
    {

        public PointCharge(Point position, double charge) : base(position, charge)
        {
            base.Position = position;
            base.Charge = charge;
        }

        public override double GetField(double x, double y)
        {
            double k = 8.987551787e9;
            double r = Math.Sqrt(Math.Pow((x - Position.X), 2) + Math.Pow((y - Position.Y), 2));
            return Charge * k / (r * r);
        }
    }

}