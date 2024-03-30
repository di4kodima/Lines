using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        void printLine(Vector pointFrom, Vector pointTo, Brush color, float scale=1)
        {
            Line line = new Line();
            (line.X1, line.Y1) = (pointFrom.X, pointFrom.Y-130);
            (line.X2, line.Y2) = (pointTo.X, pointTo.Y-130);
            line.Stroke = color;

            GridField.Children.Add(line);
        }

        void printEclipse(Vector position, int Width, Brush color)
        {
            Ellipse p = new();
            p.RenderTransform = new TranslateTransform { X = position.X - 600, Y = position.Y - 400 };
            p.Margin = new();
            p.StrokeThickness = 1;
            p.Height = p.Width = Width;
            p.Fill = color;

            GridField.Children.Add(p);
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
                printLine(new Vector(points[0].X, points[0].Y), new Vector(points[1].X, points[1].Y), Brushes.LightSteelBlue);
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
                {
                    showError("Неверно задан размер растра!");
                    return;
                }
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

                    Brush EclipseColor;
                    
                    EclipseColor = Brushes.Blue;
                    if (charge > 0) { EclipseColor = Brushes.Red; }

                    printEclipse(new Vector(mouse.X, mouse.Y), 10, EclipseColor);
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
                    LinesToRemove.Add(element);
                }
            }

            //Удаляем прошлые линии
            foreach (UIElement Line in LinesToRemove)
            {
                GridField.Children.Remove(Line);
            }

            List<double> values = new();

            if (!double.TryParse(TbxMinFieldValue.Text, out double MinField))
            {
                showError("Неверно задано начало диапазона!");
                return;
            }

            if (!double.TryParse(TbxMaxFieldValue.Text, out double MaxField))
            {
                showError("Неверно задан конец диапазона!");
                return;
            }

            if (!double.TryParse(TbxLInesCount.Text, out double LinesCount))
            {
                showError("Неверно задано количество линий поля!");
                return;
            }
            for (int i = 0; i < LinesCount; i++ )
            {
                values.Add(MaxField / LinesCount * i);
            }
            if(!double.TryParse(TbxEps.Text,out double h))
            {
                showError("Неверно задан размер растра!");
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
                    asd.Add((Ellipse)val);
            }
            GridField.Children.Remove(asd[asd.Count - 1]);
        }

        private void TbnClear_Click(object sender, RoutedEventArgs e)
        {
            Charges.Clear();
            GridField.Children.Clear();
        }

        private void showError(string message, string title = "Error")
        {
            MessageBox.Show(message, title);
            return;
        }

        private void button_forceLines__Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(input_ForceLinesCount.Text, out int LinesCount))
            {
                showError("Неверно указано число силовых линий!");
                return;
            }

            float r = 20f; // Хард код
            IEnumerable<ChargeObject> PosCharges = Charges.Where(point => point.Charge > 0);

            foreach (ChargeObject PosCharge in PosCharges) 
            {
                for(float a = 0; a < Math.PI * 2; a += (float)Math.PI * 2 / LinesCount)
                {
                    Point point = new(PosCharge.Position.X + Math.Cos(a) * r, PosCharge.Position.Y + Math.Sin(a) * r);

                    printEclipse(new Vector(point.X, point.Y), 2, Brushes.Orange);
                    Ellipse p = new();

                    ForceLinesPaint(point);
                }
            }
        }
        
        void ForceLinesPaint(Point start, int MaxCount = 1000)
        {
            MaxCount--;
            if(MaxCount <= 0) { return; }

            Vector Vres = new();

            Double h = 5;
            if (start.X > 2000 || start.X < 0 || start.Y > 1000 || start.Y < 0)
                h = 100;

            foreach (ChargeObject Charge in Charges) 
            { 
                if(Charge.GetField(start.X, start.Y) < -1000000000) { return; }

                Vector v = new()
                {
                    X = -Charge.Position.X + start.X,
                    Y = -Charge.Position.Y + start.Y
                };

                v.Normalize();
                v *= Charge.GetField(start.X, start.Y);

                Vres += v;
            }

            Vres.Normalize();
            Vres *= h;

            printLine(new Vector(start.X, start.Y), new Vector(start.X + Vres.X, start.Y + Vres.Y), Brushes.OrangeRed);
            ForceLinesPaint(new Point(start.X + Vres.X, start.Y + Vres.Y), MaxCount);
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