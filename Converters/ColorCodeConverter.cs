using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorPicker.Converters
{
    internal class ColorCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return "#00000000";
            }

            switch(value)
            {
                case SolidColorBrush brush:
                    {
                        var c = brush.Color;
                        return string.Format("#{0:X}{1:X}{2:X}{3:X}", c.A, c.R, c.G, c.B);
                    }
            }

            throw new InvalidCastException($"Not supported color type. Type = {value?.GetType()}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
