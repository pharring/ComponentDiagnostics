using System;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class KilobytesConverter : ValueConverter<int, string>
    {
        protected override string Convert(int value, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format(culture, "{0:n0}KB", value / 1024);
        }
    }
}
