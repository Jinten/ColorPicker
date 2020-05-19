using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ColorPicker.Converters
{
    internal class ColorValueInputConverter : DependencyObject, IValueConverter
    {
        public bool DecimalPointEnable
        {
            get => (bool)GetValue(DecimalPointEnableProperty);
            set => SetValue(DecimalPointEnableProperty, value);
        }

        public static readonly DependencyProperty DecimalPointEnableProperty =
            DependencyProperty.Register(nameof(DecimalPointEnable), typeof(bool), typeof(ColorValueInputConverter), new UIPropertyMetadata(false));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = 0;
            if(double.TryParse(parameter as string, out v))
            {
                switch(value)
                {
                    case byte b:
                        return (b * v).ToString();
                    case sbyte sb:
                        return (sb * v).ToString();
                    case int i:
                        return (i * v).ToString();
                    case uint ui:
                        return (ui * v).ToString();
                    case long l:
                        return (l * v).ToString();
                    case ulong ul:
                        return (ul * v).ToString();
                    case short s:
                        return (s * v).ToString();
                    case ushort us:
                        return (us * v).ToString();
                    case float f:
                        return DecimalPointEnable ? (f * v).ToString("F3") : (f * v).ToString("F0");
                    case double d:
                        return DecimalPointEnable ? (d * v).ToString("F6") : (d * v).ToString("F6");
                    case decimal dcm:
                        return DecimalPointEnable ? (dcm * (decimal)v).ToString("F9") : (dcm * (decimal)v).ToString("F0");
                }
            }

            switch (value)
            {
                case byte b:
                    return b.ToString();
                case sbyte sb:
                    return sb.ToString();
                case int i:
                    return i.ToString();
                case uint ui:
                    return ui.ToString();
                case long l:
                    return l.ToString();
                case ulong ul:
                    return ul.ToString();
                case short s:
                    return s.ToString();
                case ushort us:
                    return us.ToString();
                case float f:
                    return f.ToString("F3");
                case double d:
                    return d.ToString("F6");
                case decimal dcm:
                    return dcm.ToString("F9");
            }

            throw new InvalidCastException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string) == true)
            {
                return 0.0;
            }

            double v = 0;
            if (double.TryParse(value as string, out v))
            {
                double div = 0;
                if (double.TryParse(parameter as string, out div))
                {
                    v /= div;
                    return Math.Min(1.0, Math.Max(0, v));
                }
                else
                {
                    return v;
                }
            }
            return 0.0;
        }
    }
}
