using System;
using System.Globalization;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class UtcToLocalTimeConverter : ValueConverter<DateTime,string>
    {
        protected override string Convert(DateTime value, object parameter, CultureInfo culture)
        {
            DateTime localTime = value.ToLocalTime();
            return parameter is string format ? localTime.ToString(format, culture) : localTime.ToString(culture);
        }
    }
}
