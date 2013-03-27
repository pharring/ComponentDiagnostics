using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class NavItemToFontWeightConverter : MultiValueConverter<object, object, FontWeight>
    {
        protected override FontWeight Convert(object value1, object value2, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value1 == value2)
                return FontWeights.Bold;

            return FontWeights.Normal;
        }
    }
}
