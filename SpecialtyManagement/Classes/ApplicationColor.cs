using System.Windows;
using System.Windows.Media;

namespace SpecialtyManagement
{
    public class ApplicationColor
    {
        public static SolidColorBrush ColorPrimary = Application.Current.Resources["ColorPrimary"] as SolidColorBrush;
        public static SolidColorBrush ColorSecondary = Application.Current.Resources["ColorSecondary"] as SolidColorBrush;
        public static SolidColorBrush ColorAccent = Application.Current.Resources["ColorAccent"] as SolidColorBrush;
    }
}