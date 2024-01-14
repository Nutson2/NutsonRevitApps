using System;
using System.Windows.Data;

namespace NRPUtils.ValueConverters
{
    public class IllegalCharactersToUnderscoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return value;
            return RemoveIllegalCharacters(value.ToString());
        }

        private object RemoveIllegalCharacters(string stringValue)
        {
            return stringValue.Replace(',', '.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return value;
            return RemoveIllegalCharacters(value.ToString());
        }
    }
}
