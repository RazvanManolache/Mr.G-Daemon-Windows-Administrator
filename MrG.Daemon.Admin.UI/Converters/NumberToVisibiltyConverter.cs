using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MrG.Daemon.Manage.Converters
{
    public class NumberToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter == null)
                {
                    if (intValue > 0)
                    {
                        return System.Windows.Visibility.Visible;
                    }

                }
                else
                {
                    if (intValue == 0)
                    {
                        return System.Windows.Visibility.Visible;
                    }
                }
                
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
