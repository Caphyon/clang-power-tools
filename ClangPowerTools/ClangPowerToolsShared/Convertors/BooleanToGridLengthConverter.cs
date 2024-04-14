using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueAsBool && valueAsBool)
            {
                return new GridLength(1, GridUnitType.Auto);
            }
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
