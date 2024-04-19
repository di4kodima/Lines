﻿using System.Windows;
using System.Windows.Controls;
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
        SolidColorBrush Potentialbrush = Brushes.LightSteelBlue;
        SolidColorBrush ForceLinesBrush = Brushes.Orange;
        double LinesTickness = 1;
        double MinField = 0;

        public double WindowHeight;
        public double WindowWeight;

        private bool isMouseDown = false;
        Ellipse? targetEllipse = null;
        List<Ellipse> ellipses = new List<Ellipse>();

        private bool IsForceLines = false;
        private bool IsEquipotentials = false;

        private bool isForceLines
        {
            get { return IsForceLines; }
            set
            {
                IsForceLines = value;
                updatePopUpButtons();
                return;
            }
        }

        private bool isEquipotentials
        {
            get { return IsEquipotentials; }
            set
            {
                IsEquipotentials = value;
                updatePopUpButtons();
                return;
            }
        }

        Objects CurBrush;
        protected List<ChargeObject> Charges = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        protected void printLine(Vector pointFrom, Vector pointTo, Brush color, float scale = 1)
        {
            Line line = new Line();
            (line.X1, line.Y1) = (pointFrom.X, pointFrom.Y - 130);
            (line.X2, line.Y2) = (pointTo.X, pointTo.Y - 130);
            line.Stroke = color;
            line.StrokeThickness = LinesTickness;

            GridField.Children.Add(line);
        }

        void printEllipse(Vector position, int Width, Brush color)
        {
            Ellipse p = new();
            p.RenderTransform = new TranslateTransform { X = position.X - 600, Y = position.Y - 400 };
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
                printLine(new Vector(points[0].X, points[0].Y), new Vector(points[1].X, points[1].Y), Potentialbrush);
            }
        }

        bool OnLine(double x1, double x2, double y1, double y2, double h, double v, out Point p)
        {
            double f1 = func(x1, y1);
            double f2 = func(x2, y2);
            double min = Math.Min(f1, f2);
            double max = Math.Max(f1, f2);
            if (x1 != x2)
                if (f1 < f2)
                    p.X = x1 + h * (v - min) / (max - min);
                else
                    p.X = x1 + h * (1 - (v - min) / (max - min));
            else
                p.X = x1;

            if (y2 != y1)
                if (f1 > f2)
                    p.Y = y1 + h * (1 - (v - min) / (max - min));
                else
                    p.Y = y1 + h * ((v - min) / (max - min));
            else
                p.Y = y1;
            return (v <= max && v > min);
        }

        double func(double x, double y)
        {
            double Field = 0;
            foreach (ChargeObject charge in Charges)
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

            isMouseDown = false;

            Point mouse = e.GetPosition((IInputElement)this);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                MessageBox.Show("ПКМ");
                double value = func(mouse.X, mouse.Y);
                if (!double.TryParse(TbxEps.Text, out double h))
                {
                    ShowError("Неверно задан размер растра!");
                    return;
                }
                Rastr(mouse.X, mouse.Y, h, value);
            }
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (!Double.TryParse(TbxCharge.Text, out double charge))
                return;
            if (charge == 0)
            {
                ShowError("Заряд не может быть равным нулю!");
                return;
            }
            switch (CurBrush)
            {
                case Objects.point:
                    Charges.Add(new PointCharge(mouse, charge));

                    Brush EclipseColor;

                    EclipseColor = Brushes.Blue;
                    if (charge > 0) { EclipseColor = Brushes.Red; }

                    printEllipse(new Vector(mouse.X, mouse.Y + 5), 10, EclipseColor);
                    updateScene();
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

        private void GridField_removeEllipse(object sender, MouseButtonEventArgs e)
        {
            int countOfCharges = Charges.Count();
            if (countOfCharges == 0) return;

            targetEllipse = getEllipseFromMouse();
            if (targetEllipse is null) return;

            if (countOfCharges == 1) TbnClear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            int indexOfellipse = ellipses.IndexOf(targetEllipse);
            if (indexOfellipse == -1) return;
            GridField.Children.Remove(targetEllipse);
            Charges.RemoveAt(indexOfellipse);
            ellipses.RemoveAt(indexOfellipse);
            targetEllipse = null;
            updateScene();
        }

        private void button_equipotentials__Click(object sender, RoutedEventArgs e)
        {
            if (Charges.Count == 0) return;

            removeLines(Potentialbrush);

            List<double> values = new();

            if (!double.TryParse(TbxMinFieldValue.Text, out double MinField))
            {
                ShowError("Неверно задано начало диапазона!");
                return;
            }

            if (!double.TryParse(TbxMaxFieldValue.Text, out double MaxField))
            {
                ShowError("Неверно задан конец диапазона!");
                return;
            }

            if (!double.TryParse(TbxLInesCount.Text, out double LinesCount))
            {
                ShowError("Неверно задано количество линий поля!");
                return;
            }
            for (int i = 1; i < LinesCount + 1; i++)
            {
                values.Add((MaxField - MinField) / LinesCount * i);
            }
            if (!double.TryParse(TbxEps.Text, out double h))
            {
                ShowError("Неверно задан размер растра!");
                return;
            }
            isEquipotentials = true;
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
            if (Charges.Count == 0) return;

            int countOfCharges = Charges.Count();
            if (countOfCharges == 0) return;
            if (countOfCharges == 1) TbnClear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            int indexOfellipse = ellipses.Count - 1;
            if (indexOfellipse == -1) return;
            GridField.Children.Remove(ellipses[indexOfellipse]);
            Charges.RemoveAt(indexOfellipse);
            ellipses.RemoveAt(indexOfellipse);

            updateScene();
        }

        private void TbnClear_Click(object sender, RoutedEventArgs e)
        {
            Charges.Clear();
            GridField.Children.Clear();
            ellipses.Clear();
            isForceLines = false;
            isEquipotentials = false;
        }


        void removeLines(Brush color)
        {
            //Мусорка для старых линий
            List<UIElement> LinesToRemove = new List<UIElement>();

            //Ещем все объекты, которые линии
            foreach (UIElement element in GridField.Children)
            {
                if (element is Line && ((Line)element).Stroke == color)
                {
                    LinesToRemove.Add(element);
                }
            }

            foreach (var L in LinesToRemove)
            {
                GridField.Children.Remove(L);
            }
        }

        void button_forceLines__Click(object sender, RoutedEventArgs e)
        {
            if (Charges.Count == 0) return;

            removeLines(ForceLinesBrush);

            if (!int.TryParse(input_ForceLinesCount.Text, out int LinesCount))
            {
                ShowError("Неверно указано число силовых линий!");
                return;
            }

            if (!double.TryParse(TbxForceLinesStep.Text, out double h))
            {
                ShowError("Неверно указан силовых линий!");
                return;
            }

            double r = 10;
            IEnumerable<ChargeObject> PositiveCharges = Charges.Where(point => point.Charge > 0);

            if (PositiveCharges.Count() == 0)
            {
                isForceLines = false;
                return;
            }
            isForceLines = true;

            foreach (ChargeObject PosCharge in PositiveCharges)
            {
                for (float a = 0; a < Math.PI * 2; a += (float)Math.PI * 2 / LinesCount)
                {
                    Point point = new(PosCharge.Position.X + Math.Cos(a) * r, PosCharge.Position.Y + Math.Sin(a) * r);
                    ForceLinesPaint(point, h);
                }
            }
        }

        void ForceLinesPaint(Point start, double h, int IterCount = 1000)
        {
            Vector Vres = new();

            IterCount--;
            if (IterCount <= 0)
            {
                return;
            }

            foreach (ChargeObject Charge in Charges)
            {
                double Field = Charge.GetField(start.X, start.Y);

                if (Field < -1000000000) return;
                if (Math.Abs(Field) < 0.00000000001) return;
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
            if (start.X > WindowWeight + h || start.X + h < 0 || start.Y > WindowHeight + h || start.Y < 0)
                a = 100;

            Vres *= a;

            if (!(start.X > WindowWeight || start.X < 0 || start.Y > WindowHeight || start.Y < 0))
                printLine(new Vector(start.X, start.Y), new Vector(start.X + Vres.X, start.Y + Vres.Y), ForceLinesBrush);


            ForceLinesPaint(new Point(start.X + Vres.X, start.Y + Vres.Y), h, IterCount);
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
            ChargeObject sameInCharges = Charges[ellipses.IndexOf(targetEllipse)];
            targetEllipse.RenderTransform = new TranslateTransform { X = mousePos.X, Y = mousePos.Y };
            sameInCharges.Position = new Point() { X = mousePos.X + 600, Y = mousePos.Y + 265 + 130 };

            updateScene();
        }

        private void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title);
            return;
        }

        void updateScene(bool ignoreFlags = false)
        {
            if (isForceLines || ignoreFlags) button_forceLines.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if (isEquipotentials || ignoreFlags) button_equipotentials.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void button_start_Click(object sender, RoutedEventArgs e)
        {
            if (Charges.Count == 0) return;
            updateScene(true);
        }

        private void button_popUp1_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string? buttonText = button.Content.ToString();

            if (buttonText is null)
            {
                ShowError("Ошибка!");
                return;
            }

            buttons_popUpHandler(buttonText);
            updatePopUpButtons();
        }

        private void button_popUp2_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string? buttonText = button.Content.ToString();

            if (buttonText is null)
            {
                ShowError("Ошибка!");
                return;
            }

            buttons_popUpHandler(buttonText);
            updatePopUpButtons();
        }

        void updatePopUpButtons()
        {

            if (!isEquipotentials && !isForceLines)
            {
                button_popUp1.Visibility = Visibility.Hidden;
                button_popUp1.IsEnabled = false;
                button_popUp2.Visibility = Visibility.Hidden;
                button_popUp2.IsEnabled = false;
                return;
            }

            button_popUp1.Visibility = Visibility.Visible;
            button_popUp1.IsEnabled = true;
            if (isEquipotentials && isForceLines)
            {
                button_popUp1.Content = "Очистить линии напряженности";
                button_popUp2.Content = "Очистить силовые линии";
                button_popUp2.Visibility = Visibility.Visible;
                button_popUp2.IsEnabled = true;
                return;
            }

            if (isForceLines)
            {
                button_popUp1.Content = "Очистить силовые линии";
                button_popUp2.Visibility = Visibility.Hidden;
                button_popUp2.IsEnabled = false;
                return;
            }

            if (isEquipotentials)
            {
                button_popUp1.Content = "Очистить линии напряженности";
                button_popUp2.Visibility = Visibility.Hidden;
                button_popUp2.IsEnabled = false;
                return;
            }
        }

        void buttons_popUpHandler(string action)
        {
            switch (action)
            {
                case "Очистить линии напряженности":
                    removeLines(Brushes.LightSteelBlue);
                    isEquipotentials = false;
                    break;
                case "Очистить силовые линии":
                    removeLines(Brushes.Orange);
                    isForceLines = false;
                    break;
            }
        }

        private void SldLinesColor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TbxForceLinesWaveColor.Text = ((int)e.NewValue).ToString();
            WaveConverter.WavelengthToRGB(e.NewValue, out int R, out int G , out int B);
            if (RcgForceLinesColor == null) return;
            removeLines(ForceLinesBrush);
            ForceLinesBrush = new SolidColorBrush(Color.FromArgb(255, (byte)R, (byte)G, (byte)B));
            RcgForceLinesColor.Fill = ForceLinesBrush;
            updateScene();
        }

        private void d_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TbxStrokeTickness.Text = ((int)e.NewValue).ToString();
            if(LineStrokeTickness is null) return;
            LinesTickness = e.NewValue;
            LineStrokeTickness.StrokeThickness = e.NewValue;
            updateScene();
        }

        private void SldPotencialLinesColor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TbxPotencialLinesWaveColor.Text = ((int)e.NewValue).ToString();
            WaveConverter.WavelengthToRGB(e.NewValue, out int R, out int G, out int B);
            if (RcgPotentialLinesColor == null) return;
            Potentialbrush = new SolidColorBrush(Color.FromArgb(255, (byte)R, (byte)G, (byte)B));
            RcgPotentialLinesColor.Fill = Potentialbrush;
            updateScene();
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