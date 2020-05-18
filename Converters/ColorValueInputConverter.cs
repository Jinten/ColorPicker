using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ColorPicker.Converters
{
    internal class ColorValueInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((float)value).ToString("F3");
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
                return Math.Min(255, Math.Max(0, v)) / 255.0;
            }
            return 0.0;
        }
    }
}
