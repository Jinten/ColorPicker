using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace ColorPicker.Controls
{
    internal static class ControlSize
    {
        public const double HeaderHeight = 20;
        public const double CurrentPreviewWidth = 128;
        public const double CurrentPreviewHeight = 16;

        public const double SVPickAreaWidth = 255.0;
        public const double SVPickAreaHeight = 255.0;
        public const double HPickAreaWidth = 24.0;
        public const double HPickAreaHeight = 255.0;
        public const double APickAreaWidth = 24.0;
        public const double APickAreaHeight = 255.0;

        public const double SVPickerWidth = 16;
        public const double SVPickerHeight = 16;

        public const double HPickerWidth = 24;
        public const double HPickerHeight = 1;
        public const double APickerWidth = 24;
        public const double APickerHeight = 1;

        public const double InputAreaWidth = 255;
        public const double InputAreaHeight = 18;
        public const double InputContentWidth = 48;

        public const double SamplingPreviewWidth = 200;
        public const double SamplingPreviewHeight = 200;

        public const double SamplingPreviewSubWidth = SamplingPreviewWidth * 0.8;
        public const double SamplingPreviewSubHeight = SamplingPreviewHeight * 0.8;

        public const double SamplingPreviewSubOffsetX = (SamplingPreviewWidth - SamplingPreviewSubWidth) * 0.5;
        public const double SamplingPreviewSubOffsetY = (SamplingPreviewHeight - SamplingPreviewSubHeight) * 0.5;
    }

    internal static class MathEx
    {
        internal static double Clamp(double v, double min, double max)
        {
            return Math.Min(max, Math.Max(min, v));
        }
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
                h = (rgb.G - rgb.B) / h;
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
            nameof(BaseColor), typeof(SolidColorBrush), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(null));

        SolidColorBrush CurrentColor
        {
            get => (SolidColorBrush)GetValue(CurrentColorProperty);
            set => SetValue(CurrentColorProperty, value);
        }
        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register(
            nameof(CurrentColor), typeof(SolidColorBrush), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(null));

        double SamplingPreviewOffsetX
        {
            get => (double)GetValue(SamplingPreviewOffsetXProperty);
            set => SetValue(SamplingPreviewOffsetXProperty, value);
        }
        public static readonly DependencyProperty SamplingPreviewOffsetXProperty = DependencyProperty.Register(
            nameof(SamplingPreviewOffsetX), typeof(double), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(0.0));

        double SamplingPreviewOffsetY
        {
            get => (double)GetValue(SamplingPreviewOffsetYProperty);
            set => SetValue(SamplingPreviewOffsetYProperty, value);
        }
        public static readonly DependencyProperty SamplingPreviewOffsetYProperty = DependencyProperty.Register(
            nameof(SamplingPreviewOffsetY), typeof(double), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(0.0));

        bool IsSamplingColorFromScreen
        {
            get => (bool)GetValue(IsSamplingColorFromScreenProperty);
            set => SetValue(IsSamplingColorFromScreenProperty, value);
        }
        public static readonly DependencyProperty IsSamplingColorFromScreenProperty = DependencyProperty.Register(
            nameof(IsSamplingColorFromScreen), typeof(bool), typeof(ColorPickerWindow), new FrameworkPropertyMetadata(false));

        public double Hue { get; private set; } = 1.0;
        public double Saturate { get; private set; } = 0.0;
        public double Value { get; private set; } = 1.0;
        public double Alpha { get; private set; } = 1.0;

        Action<HSV> HSVPropertyChanged { get; set; } = null;
        Action<double> AlphaPropertyChanged { get; set; } = null;
        DispatcherTimer SampleColorDispatcherTimer { get; set; } = null;

        #region win32

        const int VK_LBUTTON = 0x01;
        const int WH_MOUSE_LL = 14;

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X { get; set; }
            public int Y { get; set; }
            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        public delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtkey);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lppoint);

        #endregion

        Rectangle _SVPickArea = null;
        Rectangle _HPickArea = null;
        Rectangle _APickArea = null;
        Picker<Ellipse> _SVPicker = null;
        Picker<Rectangle> _HPicker = null;
        Picker<Rectangle> _APicker = null;
        ContentControl _DropperSymble = null;

        bool _IsCapturedSVPicker = false;
        bool _IsCapturedHPicker = false;
        bool _IsCapturedAPicker = false;
        bool _IsClosing = false;

        public ColorPickerWindow(HSV initHSV, double alpha, Action<HSV> hsvPropertyChanged, Action<double> alphaPropertyChanged)
        {
            Hue = initHSV.H;
            Saturate = initHSV.S;
            Value = initHSV.V;
            Alpha = alpha;

            HSVPropertyChanged = hsvPropertyChanged;
            AlphaPropertyChanged = alphaPropertyChanged;

            SampleColorDispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 16),
            };

            SampleColorDispatcherTimer.Tick += Dispatcher_Tick;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _SVPickArea = GetTemplateChild("__SVPickArea__") as Rectangle;
            _HPickArea = GetTemplateChild("__HPickArea__") as Rectangle;
            _APickArea = GetTemplateChild("__APickArea__") as Rectangle;

            _SVPicker = new Picker<Ellipse>(GetTemplateChild("__SVPicker__") as Ellipse);
            _SVPicker.Position.X = ControlSize.SVPickAreaWidth * 0.5 - ControlSize.SVPickerWidth * 0.5;
            _SVPicker.Position.Y = ControlSize.SVPickAreaHeight * 0.5 - ControlSize.SVPickerHeight * 0.5;

            _HPicker = new Picker<Rectangle>(GetTemplateChild("__HPicker__") as Rectangle);
            _APicker = new Picker<Rectangle>(GetTemplateChild("__APicker__") as Rectangle);

            _SVPickArea.MouseLeftButtonDown += SVPicker_MouseLeftButtonDown;
            _SVPickArea.MouseLeftButtonUp += SVPicker_MouseLeftButtonUp;
            _SVPicker.Element.MouseLeftButtonDown += SVPicker_MouseLeftButtonDown;
            _SVPicker.Element.MouseLeftButtonUp += SVPicker_MouseLeftButtonUp;

            _HPickArea.MouseLeftButtonDown += HPicker_MouseLeftButtonDown;
            _HPickArea.MouseLeftButtonUp += HPicker_MouseLeftButtonUp;
            _HPicker.Element.MouseLeftButtonDown += HPicker_MouseLeftButtonDown;
            _HPicker.Element.MouseLeftButtonUp += HPicker_MouseLeftButtonUp;

            _APickArea.MouseLeftButtonDown += APicker_MouseLeftButtonDown;
            _APickArea.MouseLeftButtonUp += APicker_MouseLeftButtonUp;
            _APicker.Element.MouseLeftButtonDown += APicker_MouseLeftButtonDown;
            _APicker.Element.MouseLeftButtonUp += APicker_MouseLeftButtonUp;

            UpdateColor();

            _DropperSymble = GetTemplateChild("__DropperSymble__") as ContentControl;
            _DropperSymble.MouseLeftButtonDown += SamplingSymble_MouseLeftButtonDown;
        }

        public void UpdateRGB(RGB rgb)
        {
            var hsv = ColorConverter.RGBtoHSV(rgb);

            Hue = hsv.H;
            Saturate = hsv.S;
            Value = hsv.V;

            UpdateColor();
        }

        public void UpdateHSV(HSV hsv)
        {
            Hue = hsv.H;
            Saturate = hsv.S;
            Value = hsv.V;

            UpdateColor();
        }

        public void UpdateAlpha(double alpha)
        {
            Alpha = alpha;

            UpdateColor();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (_IsClosing == false)
            {
                Close();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _IsClosing = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            HSVPropertyChanged = null;
            AlphaPropertyChanged = null;

            SampleColorDispatcherTimer.Stop();
            SampleColorDispatcherTimer.Tick -= Dispatcher_Tick;
            SampleColorDispatcherTimer = null;

            _SVPickArea.MouseLeftButtonDown -= SVPicker_MouseLeftButtonDown;
            _SVPickArea.MouseLeftButtonUp -= SVPicker_MouseLeftButtonUp;
            _SVPickArea = null;

            _SVPicker.Element.MouseLeftButtonDown -= SVPicker_MouseLeftButtonDown;
            _SVPicker.Element.MouseLeftButtonUp -= SVPicker_MouseLeftButtonUp;
            _SVPicker = null;

            _HPickArea.MouseLeftButtonDown -= HPicker_MouseLeftButtonDown;
            _HPickArea.MouseLeftButtonUp -= HPicker_MouseLeftButtonUp;
            _HPickArea = null;

            _HPicker.Element.MouseLeftButtonDown -= HPicker_MouseLeftButtonDown;
            _HPicker.Element.MouseLeftButtonUp -= HPicker_MouseLeftButtonUp;
            _HPicker = null;

            _APickArea.MouseLeftButtonDown -= APicker_MouseLeftButtonDown;
            _APickArea.MouseLeftButtonUp -= APicker_MouseLeftButtonUp;
            _APickArea = null;

            _APicker.Element.MouseLeftButtonDown -= APicker_MouseLeftButtonDown;
            _APicker.Element.MouseLeftButtonUp -= APicker_MouseLeftButtonUp;
            _APicker = null;

            _DropperSymble.MouseLeftButtonDown -= SamplingSymble_MouseLeftButtonDown;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (e.LeftButton == MouseButtonState.Released)
            {
                _IsCapturedSVPicker = false;
                _IsCapturedHPicker = false;
                _IsCapturedAPicker = false;
                IsSamplingColorFromScreen = false;
            }
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

            if (_IsCapturedAPicker)
            {
                UpdateA(e);
            }

            if(IsSamplingColorFromScreen)
            {
                Mouse.SetCursor(Cursors.Cross);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            _IsCapturedSVPicker = false;
            _IsCapturedHPicker = false;
            _IsCapturedAPicker = false;
        }

        void Dispatcher_Tick(object sender, EventArgs e)
        {
            if (GetKeyState(VK_LBUTTON) >= 0)
            {
                IsSamplingColorFromScreen = false;
                SampleColorDispatcherTimer.Stop();
                Mouse.SetCursor(Cursors.Arrow);
                return;
            }

            POINT point;
            GetCursorPos(out point);

            SamplingPreviewOffsetX = point.X - ControlSize.SamplingPreviewWidth * 0.5;
            SamplingPreviewOffsetY = point.Y - ControlSize.SamplingPreviewHeight * 0.5;

            var sampledScreenColor = SamplePixelColor(point.X, point.Y);
            var hsv = ColorConverter.RGBtoHSV(new RGB(sampledScreenColor.ScR, sampledScreenColor.ScG, sampledScreenColor.ScB));

            Hue = hsv.H;
            Saturate = hsv.S;
            Value = hsv.V;

            Alpha = sampledScreenColor.A / 255.0;

            UpdateColor();

            HSVPropertyChanged(new HSV(Hue, Saturate, Value));
            AlphaPropertyChanged(Alpha);

            Mouse.SetCursor(Cursors.Cross);
        }

        void UpdateColor()
        {
            UpdateSVPickerPosition(Saturate * ControlSize.SVPickAreaWidth, (1 - Value) * ControlSize.SVPickAreaHeight);
            UpdateHPickerPosition(Hue * ControlSize.HPickAreaHeight);
            UpdateAPickerPosition((1.0 - Alpha) * ControlSize.APickAreaHeight);
            UpdateBaseColor();
            UpdateCurrentColor();
        }

        void UpdateSV(MouseEventArgs e)
        {
            var svPos = e.GetPosition(_SVPickArea);
            Saturate = Math.Max(0.0, Math.Min(1.0, svPos.X / _SVPickArea.ActualWidth));
            Value = 1 - Math.Max(0.0, Math.Min(1.0, svPos.Y / _SVPickArea.ActualHeight));

            UpdateSVPickerPosition(Saturate * ControlSize.HPickAreaHeight, (1.0 - Value) * ControlSize.HPickAreaHeight);

            UpdateCurrentColor();

            HSVPropertyChanged(new HSV(Hue, Saturate, Value));
        }

        void UpdateH(MouseEventArgs e)
        {
            var hPos = e.GetPosition(_HPickArea);
            Hue = MathEx.Clamp(hPos.Y / ControlSize.HPickAreaHeight, 0.0, 1.0);

            UpdateHPickerPosition(Hue * ControlSize.HPickAreaHeight);

            UpdateBaseColor();
            UpdateCurrentColor();

            HSVPropertyChanged(new HSV(Hue, Saturate, Value));
        }

        void UpdateA(MouseEventArgs e)
        {
            var aPos = e.GetPosition(_APickArea);
            Alpha = 1.0 - MathEx.Clamp(aPos.Y / ControlSize.APickAreaHeight, 0, 1);

            UpdateAPickerPosition((1.0 - Alpha) * ControlSize.APickAreaHeight);

            UpdateCurrentColor();

            AlphaPropertyChanged(Alpha);
        }

        void UpdateBaseColor()
        {
            var rgb = ColorConverter.HSVtoRGB(new HSV(Hue, 1, 1));

            if (BaseColor == null)
            {
                BaseColor = new SolidColorBrush(Color.FromScRgb(1, (float)rgb.R, (float)rgb.G, (float)rgb.B));
            }
            else
            {
                BaseColor.Color = Color.FromScRgb(1, (float)rgb.R, (float)rgb.G, (float)rgb.B);
            }
        }

        void UpdateCurrentColor()
        {
            var rgb = ColorConverter.HSVtoRGB(new HSV(Hue, Saturate, Value));

            if (CurrentColor == null)
            {
                CurrentColor = new SolidColorBrush(Color.FromScRgb((float)Alpha, (float)rgb.R, (float)rgb.G, (float)rgb.B));
            }
            else
            {
                CurrentColor.Color = Color.FromScRgb((float)Alpha, (float)rgb.R, (float)rgb.G, (float)rgb.B);
            }
        }

        void UpdateSVPickerPosition(double x, double y)
        {
            double half_w = ControlSize.SVPickerWidth * 0.5;
            double half_h = ControlSize.SVPickerHeight * 0.5;
            double center_x = x - half_w;
            double center_y = y - half_h;
            _SVPicker.Position.X = MathEx.Clamp(center_x, -half_w, ControlSize.SVPickAreaWidth - half_w);
            _SVPicker.Position.Y = MathEx.Clamp(center_y, -half_h, ControlSize.SVPickAreaHeight - half_h);
        }

        void UpdateHPickerPosition(double y)
        {
            _HPicker.Position.Y = MathEx.Clamp(y, 0, ControlSize.HPickAreaHeight);
        }

        void UpdateAPickerPosition(double y)
        {
            _APicker.Position.Y = MathEx.Clamp(y, 0, ControlSize.APickAreaHeight);
        }

        void SVPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = true;
        }

        void SVPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedSVPicker = false;
        }

        void HPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = true;
        }

        void HPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedHPicker = false;
        }

        void APicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedAPicker = true;
        }

        void APicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsCapturedAPicker = false;
        }

        void SamplingSymble_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSamplingColorFromScreen = true;
            SampleColorDispatcherTimer.Start();

            Mouse.SetCursor(Cursors.Cross);
        }

        Color SamplePixelColor(int x, int y)
        {
            using (var bitmap = new System.Drawing.Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    bmpGraphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));

                    var color = bitmap.GetPixel(0, 0);
                    return Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
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
            nameof(R), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RGBPropertyChanged));

        public float G
        {
            get => (float)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }
        public static readonly DependencyProperty GProperty = DependencyProperty.Register(
            nameof(G), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RGBPropertyChanged));

        public float B
        {
            get => (float)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }
        public static readonly DependencyProperty BProperty = DependencyProperty.Register(
            nameof(B), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RGBPropertyChanged));

        public float H
        {
            get => (float)GetValue(HProperty);
            set => SetValue(HProperty, value);
        }
        public static readonly DependencyProperty HProperty = DependencyProperty.Register(
            nameof(H), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HSVPropertyChanged));

        public float S
        {
            get => (float)GetValue(SProperty);
            set => SetValue(SProperty, value);
        }
        public static readonly DependencyProperty SProperty = DependencyProperty.Register(
            nameof(S), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HSVPropertyChanged));

        public float V
        {
            get => (float)GetValue(VProperty);
            set => SetValue(VProperty, value);
        }
        public static readonly DependencyProperty VProperty = DependencyProperty.Register(
            nameof(V), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HSVPropertyChanged));

        public float A
        {
            get => (float)GetValue(AProperty);
            set => SetValue(AProperty, value);
        }
        public static readonly DependencyProperty AProperty = DependencyProperty.Register(
            nameof(A), typeof(float), typeof(ColorPicker), new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AlphaPropertyChanged));

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

            if(_ColorPickerWindow != null)
            {
                return;
            }

            IsOpenedColorPicker = true;
            var screenPosition = PointToScreen(e.GetPosition(this));

            var initHSV = ColorConverter.RGBtoHSV(new RGB(R, G, B));
            _ColorPickerWindow = new ColorPickerWindow(initHSV, A, HSVPropertyChangedFromPickerWindow, AlphaPropertyChangedFromPickerWidnow)
            {
                DataContext = this,
                Style = ColorPickerWindowStyle,
                ResizeMode = ResizeMode.NoResize,
                Title = "Color Picker - proto type.",
                Left = screenPosition.X,
                Top = screenPosition.Y,
            };
            _ColorPickerWindow.Closed += ColorPickerWindow_Closed;
            _ColorPickerWindow.Show();
        }

        static void RGBPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ColorPicker).UpdateRGB();
        }

        static void HSVPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ColorPicker).UpdateHSV();
        }

        static void AlphaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ColorPicker).UpdateAlpha();
        }

        void ColorPickerWindow_Closed(object sender, EventArgs e)
        {
            _ColorPickerWindow.Closed -= ColorPickerWindow_Closed;
            _ColorPickerWindow = null;
        }

        void UpdateRGB()
        {
            _ColorPickerWindow?.UpdateRGB(new RGB(R, G, B));
            if (_PreviewColor != null)
            {
                _PreviewColor.Color = Color.FromScRgb(A, R, G, B);
            }
        }

        void UpdateHSV()
        {
            var hsv = new HSV(H, S, V);
            _ColorPickerWindow?.UpdateHSV(hsv);

            if (_PreviewColor != null)
            {
                var rgb = ColorConverter.HSVtoRGB(hsv);
                R = (float)rgb.R;
                G = (float)rgb.G;
                B = (float)rgb.B;
                _PreviewColor.Color = Color.FromScRgb(A, R, G, B);
            }
        }

        void UpdateAlpha()
        {
            _ColorPickerWindow?.UpdateAlpha(A);
            if (_PreviewColor != null)
            {
                _PreviewColor.Color = Color.FromScRgb(A, R, G, B);
            }
        }

        void HSVPropertyChangedFromPickerWindow(HSV hsv)
        {
            H = (float)hsv.H;
            S = (float)hsv.S;
            V = (float)hsv.V;

            var rgb = ColorConverter.HSVtoRGB(hsv);
            R = (float)rgb.R;
            G = (float)rgb.G;
            B = (float)rgb.B;

            _PreviewColor.Color = Color.FromScRgb(A, R, G, B);
        }

        void AlphaPropertyChangedFromPickerWidnow(double alpha)
        {
            A = (float)alpha;
            _PreviewColor.Color = Color.FromScRgb(A, R, G, B);
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
    }
}
