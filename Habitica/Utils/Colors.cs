using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Habitica.Utils
{
    class Colors
    {
        public static SolidColorBrush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public static SolidColorBrush Gray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));
        public static SolidColorBrush Red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F23035"));
        public static SolidColorBrush Green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#24CC8F"));
        public static SolidColorBrush Purple = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC6459DF"));
        public static SolidColorBrush Yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
    }
}
