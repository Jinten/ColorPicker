using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorPicker.Controls
{
    internal static class ControlLayout
    {
        public const double SVPickAreaWidth = 255.0;
        public const double SVPickAreaHeight = 255.0;
        public const double HPickAreaWidth = 24.0;
        public const double HPickAreaHeight = 255.0;

        public const double SVPickerWidth = 16;
        public const double SVPickerHeight = 16;

        public const double HPickerWidth = 24;
        public const double HPickerHeight = 4;
    }

    internal class Picker<T> where T : FrameworkElement
    {
        public T Element { get; }
        public TranslateTransform Position { get; } = new TranslateTransform();
        public double HalfWidth => Element.ActualWidth * 0.5;
        public double HalfHeight => Element.ActualHeight * 0.5;

        public Picker(T element)
        {
            Element = element;

            Position.X = 128 - Element.ActualWidth * 0.5;
            Position.Y = 128 - Element.ActualHeight * 0.5;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(Position);

            element.RenderTransform = transformGroup;
        }
    }

    internal struct RGB
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public RGB(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    internal struct HSV
    {
        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }

        public HSV(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    internal static class ColorConverter
    {
        internal static RGB HSVtoRGB(HSV hsv)
        {
            var rgb = new RGB(hsv.V, hsv.V, hsv.V);

            double h = hsv.H * 6.0;
            int index = (int)(h);
            double f = h - index;

            switch (index)
            {
                case 6:
                case 0:
                    rgb.G *= 1 - hsv.S * (1 - f);
                    rgb.B *= 1 - hsv.S;
                    break;
                case 1:
                    rgb.R *= 1 - hsv.S * f;
                    rgb.B *= 1 - hsv.S;
                    break;
                case 2:
                    rgb.R *= 1 - hsv.S;
                    rgb.B *= 1 - hsv.S * (1 - f);
                    break;
                case 3:
                    rgb.R *= 1 - hsv.S;
                    rgb.G *= 1 - hsv.S * f;
                    break;
                case 4:
                    rgb.R *= 1 - hsv.S * (1 - f);
                    rgb.G *= 1 - hsv.S;
                    break;
                case 5:
                    rgb.G *= 1 - hsv.S;
                    rgb.B *= 1 - hsv.S * f;
                    break;
                default:
                    throw new ArgumentException();
            }

            return rgb;
        }

        internal static HSV RGBtoHSV(RGB rgb)
        {
            double max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            double min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            double h = max - min;

            if (h == 0.0)
            {
                h = 0;
            }
            else if (max == rgb.R)
            {
                h = (rgb.R - rgb.B) / h;
                if (h < 0.0)
                {
                    h += 6.0;
                }
            }
            else if (max == rgb.G)
            {
                h = 2.0 + (rgb.B - rgb.R) / h;
            }
            else
            {
                h = 4.0 + (rgb.R - rgb.G) / h;
            }

            h /= 6.0;

            double s = (max - min);
            if (max > 0.0f)
            {
                s /= max;
            }

            return new HSV(h, s, max);
        }
    }

    internal class ColorPickerWindow : Window
    {
        SolidColorBrush BaseColor
        {
            get => (SolidColorBrush)GetValue(BaseColorProperty);
            set => SetValue(BaseColorProperty, value);
        }
        public static readonly DependencyProperty BaseColorProperty = DependencyProperty.Register(
            nameof(BaseColor), typeof(SolidColorBrush), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(Brushes.Red));

        public double Hue { get; private set; } = 1.0;
        public double Saturate { get; private set; } = 0.0;
        public double Value { get; private set; } = 1.0;

        public Action<HSV> HSVPropertyChanged { get; } = null;

        Rectangle _SVPickArea = null;
        Rectangle _HPickArea = null;
        Picker<Ellipse> _SVPicker = null;
        Picker<Rectangle> _HPicker = null;

        bool _IsCapturedSVPicker = false;
        bool _IsCapturedHPicker = false;

        public ColorPickerWindow(HSV initHSV, Action<HSV> hsvPropertyChanged)
        {
            Hue = initHSV.H;
            Saturate = initHSV.S;
            Value = initHSV.V;

            HSVPropertyChanged = hsvPropertyChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _SVPickArea = GetTemplateChild("__SVPickArea__") as Rectangle;
            _HPickArea = GetTemplateChild("__HPickArea__") as Rectangle;

            _SVPicker = new Picker<Ellipse>(GetTemplateChild("__SVPicker__") as Ellipse);
            _SVPicker.Position.X = ControlLayout.SVPickAreaWidth * 0.5 - ControlLayout.SVPickerWidth * 0.5;
            _SVPicker.Position.Y = ControlLayout.SVPickAreaHeight * 0.5 - ControlLayout.SVPickerHeight * 0.5;

            _HPicker = new Picker<Rectangle>(GetTemplateChild("__HPicker__") as Rectangle);
            _HPicker.Position.X = 0;
            _HPicker.Position.Y = 0;

            _SVPickArea.MouseLeftButtonDown += SVPickerArea_MouseLeftButtonDown;
            _SVPickArea.MouseLeftButtonUp += SVPickerArea_MouseLeftButtonUp;
            _SVPicker.Element.MouseLeftButtonDown += SVPicker_MouseLeftButtonDown;
            _SVPicker.Element.MouseLeftButtonUp += SVPicker_MouseLeftButtonUp;

            _HPickArea.MouseLeftButtonDown += HPickerArea_MouseLeftButtonDown;
            _HPickArea.MouseLeftButtonUp += HPickerArea_MouseLeftButtonUp;

            _HPicker.Element.MouseLeftButtonDown += HPicker_MouseLeftButtonDown;
            _HPicker.Element.MouseLeftButtonUp += HPicker_MouseLeftButtonUp;

            UpdateSVPickerPosition(Saturate * ControlLayout.SVPickAreaWidth, (1 - Value) * ControlLayout.SVPickAreaHeight);
            UpdateHPickerPosition(Hue * ControlLayout.HPickAreaHeight);
        }

        public void UpdateSV(MouseEventArgs e)
        {
            var svPos = e.GetPosition(_SVPickArea);
            Saturate = Math.Max(0.0, Math.Min(1.0, svPos.X / _SVPickArea.ActualWidth));
            Value = 1 - Math.Max(0.0, Math.Min(1.0, svPos.Y / _SVPickArea.ActualHeight));

            UpdateSVPickerPosition(svPos.X, svPos.Y);

            HSVPropertyChanged(new HSV(Hue, Saturate, Value));
        }

        public void UpdateH(MouseEventArgs e)
        {
            var hPos = e.GetPosition(_HPickArea);
            Hue = Math.Max(0.0, Math.Min(1.0, hPos.Y / _HPickArea.ActualHeight));

            UpdateHPickerPosition(Hue * ControlLayout.HPickAreaHeight);

            var rgb = ColorConverter.HSVtoRGB(new HSV(Hue, 1, 1));
            BaseColor = new SolidColorBrush(Color.FromScRgb(1, (float)rgb.R, (float)rgb.G, (float)rgb.B));
            BaseColor.Freeze();

            HSVPropertyChanged(new HSV(Hue, Saturate, Value));
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (_IsCapturedSVPicker)
            {
                UpdateSV(e);
            }

            if (_IsCapturedHPicker)
            {
                UpdateH(e);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            _IsCapturedSVPicker = false;
            _IsCapturedHPicker = false;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _IsCapturedSVPicker = false;
            _IsCapturedHPicker = false;
        }

        void UpdateSVPickerPosition(double x, double y)
        {
            double center_x = x - _SVPicker.Element.ActualWidth * 0.5;
            double center_y = y - _SVPicker.Element.ActualHeight * 0.5;
            _SVPicker.Position.X = Math.Min(_SVPickArea.ActualWidth - _SVPicker.HalfWidth, Math.Max(-_SVPicker.HalfWidth, center_x));
            _SVPicker.Position.Y = Math.Min(_SVPickArea.ActualHeight - _SVPicker.HalfHeight, Math.Max(-_SVPicker.HalfHeight, center_y));
        }

        void UpdateHPickerPosition(double y)
        {
            _HPicker.Position.Y = y - _HPicker.Element.ActualHeight * 0.5;
            _HPicker.Position.Y = Math.Max(0.0, Math.Min(_HPickArea.ActualHeight - _HPicker.Element.ActualHeight, _HPicker.Position.Y));
        }

        void SVPickerArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = true;
        }

        void SVPickerArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = false;
        }

        void SVPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = true;
        }

        void SVPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = false;
        }

        void HPickerArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = true;
        }

        void HPickerArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = false;
        }

        void HPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = true;
        }

        void HPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = false;
        }
    }

    public class ColorPicker : Control
    {
        public float R
        {
            get => (float)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }
        public static readonly DependencyProperty RProperty = DependencyProperty.Register(
            nameof(R), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float G
        {
            get => (float)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }
        public static readonly DependencyProperty GProperty = DependencyProperty.Register(
            nameof(G), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float B
        {
            get => (float)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }
        public static readonly DependencyProperty BProperty = DependencyProperty.Register(
            nameof(B), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float A
        {
            get => (float)GetValue(AProperty);
            set => SetValue(AProperty, value);
        }
        public static readonly DependencyProperty AProperty = DependencyProperty.Register(
            nameof(A), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float H
        {
            get => (float)GetValue(HProperty);
            set => SetValue(HProperty, value);
        }
        public static readonly DependencyProperty HProperty = DependencyProperty.Register(
            nameof(H), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float S
        {
            get => (float)GetValue(SProperty);
            set => SetValue(SProperty, value);
        }
        public static readonly DependencyProperty SProperty = DependencyProperty.Register(
            nameof(S), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public float V
        {
            get => (float)GetValue(VProperty);
            set => SetValue(VProperty, value);
        }
        public static readonly DependencyProperty VProperty = DependencyProperty.Register(
            nameof(V), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SolidColorBrush PreviewColor => GetPreviewColor();

        bool IsOpenedColorPicker
        {
            get => (bool)GetValue(IsOpenedColorPickerProperty);
            set => SetValue(IsOpenedColorPickerProperty, value);
        }
        public static readonly DependencyProperty IsOpenedColorPickerProperty = DependencyProperty.Register(
            nameof(IsOpenedColorPicker), typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(false));

        LinearGradientBrush PickColor
        {
            get => (LinearGradientBrush)GetValue(PickColorProperty);
            set => SetValue(PickColorProperty, value);
        }
        public static readonly DependencyProperty PickColorProperty = DependencyProperty.Register(
            nameof(PickColor), typeof(LinearGradientBrush), typeof(ColorPicker), new FrameworkPropertyMetadata(null));

        public Style ColorPickerWindowStyle
        {
            get => (Style)GetValue(ColorPickerWindowStyleProperty);
            set => SetValue(ColorPickerWindowStyleProperty, value);
        }
        public static readonly DependencyProperty ColorPickerWindowStyleProperty = DependencyProperty.Register(
            nameof(ColorPickerWindowStyle), typeof(Style), typeof(ColorPicker), new FrameworkPropertyMetadata(null));

        SolidColorBrush _PreviewColor = null;
        ColorPickerWindow _ColorPickerWindow = null;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            Unloaded += ColorPicker_Unloaded;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            IsOpenedColorPicker = true;
            var screenPosition = PointToScreen(e.GetPosition(this));

            var initHSV = ColorConverter.RGBtoHSV(new RGB(R, G, B));
            _ColorPickerWindow = new ColorPickerWindow(initHSV, HSVPropertyChanged)
            {
                DataContext = this,
                Style = ColorPickerWindowStyle,
                ResizeMode = ResizeMode.NoResize,
                Title="Color Picker - proto type.",
                Left = screenPosition.X,
                Top = screenPosition.Y,
            };
            _ColorPickerWindow.Show();
        }

        void HSVPropertyChanged(HSV hsv)
        {
            H = (float)hsv.H;
            S = (float)hsv.S;
            V = (float)hsv.V;

            var rgb = ColorConverter.HSVtoRGB(hsv);
            R = (float)rgb.R;
            G = (float)rgb.G;
            B = (float)rgb.B;

            _PreviewColor.Color = Color.FromScRgb(1, R, G, B);
        }

        void ColorPicker_Unloaded(object sender, RoutedEventArgs e)
        {
            _ColorPickerWindow?.Close();
            _ColorPickerWindow = null;

            IsOpenedColorPicker = false;
        }

        SolidColorBrush GetPreviewColor()
        {
            _PreviewColor = new SolidColorBrush(Color.FromScRgb(A, R, G, B));

            return _PreviewColor;
        }

        void UpdatePickColor()
        {
            PickColor.StartPoint = new Point(0, 0);
        }
    }
}
