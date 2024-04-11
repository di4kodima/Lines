using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System.Windows.Documents;
using System.Collections;
using System.Windows.Controls;

namespace Линии
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum Objects { point, line }

    public partial class MainWindow : Window
    {
<<<<<<< HEAD
        double WindowHeight;
        double WindowWeight;
=======
        private bool isMouseDown = false;
        Ellipse? targetEllipse = null;
        List<Ellipse> ellipses = new List<Ellipse>();
>>>>>>> fe3bfef7314eb5dbd5e6c52f0f5adfa3ac1a54a7

        Objects CurBrush;
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
            (line.X1, line.Y1) = (pointFrom.X, pointFrom.Y - 130);
            (line.X2, line.Y2) = (pointTo.X, pointTo.Y - 130);
            line.Stroke = color;

            GridField.Children.Add(line);
        }

        void printEclipse(Vector position, int Width, Brush color)
        {
            Ellipse p = new();
            p.RenderTransform = new TranslateTransform { X = position.X - 600, Y = position.Y - 400};
            p.Margin = new();
            p.StrokeThickness = 1;
            p.Height = p.Width = Width;
            p.Fill = color;

            GridField.Children.Add(p);
            ellipses.Add(p);
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
            isMouseDown = true;

            if (!(getEllipseFromMouse() is null)) return;

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
                    Charges.Add(new PointCharge(mouse, charge));

                    Brush EclipseColor;

                    EclipseColor = Brushes.Blue;
                    if (charge > 0) { EclipseColor = Brushes.Red; }

                    printEclipse(new Vector(mouse.X, mouse.Y + 5), 10, EclipseColor);
                    break;
            }
        }

        Ellipse? getEllipseFromMouse()
        {
            if (ellipses.Count <= 0) return null;

            foreach (Ellipse ellipse in ellipses)
            {
                //MessageBox.Show(ellipses.Count.ToString());
                //if (ellipse.RenderedGeometry.FillContains(mousePos, 300, ToleranceType.Relative))
                if (ellipse.IsMouseOver)
                {
                    return ellipse;
                }
            }
            return null;
        }

        private void GridField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            targetEllipse = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Charges.Count == 0) return;

            //Мусорка для старых линий
            List<UIElement> LinesToRemove = new List<UIElement>();

            //Ещем все объекты, которые линии
            foreach (UIElement element in GridField.Children)
            {
                if (element is Line && ((Line)element).Stroke == Brushes.LightSteelBlue)
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
            for (int i = 1; i < LinesCount + 1; i++ )
            {
                values.Add((MaxField - MinField) / LinesCount * i);
            }
            if(!double.TryParse(TbxEps.Text,out double h))
            {
                showError("Неверно задан размер растра!");
                return;
            }

            for (double i = 0; i < 1200 / h; i++)
            {
                for (double j = 0; j < 800 / h; j++)
                {
                    foreach (var val in values)
                    {
                        Rastr(0 + h * i, 0 + h * j, h, val);
                        Rastr(0 + h * i, 0 + h * j, h, -val);
                    }
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

        private void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title);
            return;
        }

        private void button_forceLines__Click(object sender, RoutedEventArgs e)
        {
            //Мусорка для старых линий
            List<UIElement> LinesToRemove = new List<UIElement>();

            //Ещем все объекты, которые линии
            foreach (UIElement element in GridField.Children)
            {
                if (element is Line && ((Line)element).Stroke == Brushes.Orange)
                {
                    LinesToRemove.Add(element);
                }
            }

            foreach (var L in LinesToRemove) 
            { 
                GridField.Children.Remove(L);
            }

            if (!int.TryParse(input_ForceLinesCount.Text, out int LinesCount))
            {
                showError("Неверно указано число силовых линий!");
                return;
            }

            if(!double.TryParse(TbxForceLinesStep.Text, out double h))
            {
                showError("Неверно указан силовых линий!");
                return;
            }

            double r = 1;
            IEnumerable<ChargeObject> PosCharges = Charges.Where(point => point.Charge > 0);

            foreach (ChargeObject PosCharge in PosCharges) 
            {
                for(float a = 0; a < Math.PI * 2; a += (float)Math.PI * 2 / LinesCount)
                {
                    Point point = new(PosCharge.Position.X + Math.Cos(a) * r, PosCharge.Position.Y + Math.Sin(a) * r);

                    //printEclipse(new Vector(point.X, point.Y), 2, Brushes.Orange);

                    ForceLinesPaint(point, h);
                }
            }
        }
        
        void ForceLinesPaint(Point start,double h , int IterCount = 1000)
        {
            Vector Vres = new();

            IterCount--;
            if(IterCount <= 0 ) 
            {
                return;
            }

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

            double a = h;

            Vres.Normalize();
            if (start.X > WindowWeight + h || start.X + h < 0 || start.Y > WindowHeight + h|| start.Y < 0)
                a = 100;
            Vres *= a;

            if (!(start.X > WindowWeight || start.X < 0 || start.Y > WindowHeight || start.Y < 0))
                printLine(new Vector(start.X, start.Y), new Vector(start.X + Vres.X, start.Y + Vres.Y), Brushes.Orange);


            ForceLinesPaint(new Point(start.X + Vres.X, start.Y + Vres.Y),h, IterCount);
        }

        public void showError(string message)
        {
            MessageBox.Show(message);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowHeight = e.NewSize.Height;
            WindowWeight = e.NewSize.Width;
        }

        private void Move_point(object sender, MouseEventArgs e)
        {
            if (!isMouseDown || ellipses.Count <= 0) return;
            var mousePos = e.GetPosition(GridField);
            mousePos.X -= 600;
            mousePos.Y -= 265;

            if (targetEllipse is null) targetEllipse = getEllipseFromMouse();
            if (targetEllipse is null) return;
            //Point targetEllipse__position = targetEllipse.RenderTransform.Transform(new Point());
            //targetEllipse__position.X += 600;
            //targetEllipse__position.Y += 400;
            //ChargeObject? sameInCharges = Charges.FirstOrDefault(charge => charge.Position == targetEllipse__position);
            //if (sameInCharges is null) return;
            ChargeObject sameInCharges = Charges[ellipses.IndexOf(targetEllipse)];
            targetEllipse.RenderTransform = new TranslateTransform {X = mousePos.X, Y = mousePos.Y};
            sameInCharges.Position = new Point() { X = mousePos.X + 600, Y = mousePos.Y + 265+130 };



            button_forceLines.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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