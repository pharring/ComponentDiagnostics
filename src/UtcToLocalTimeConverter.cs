using System;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class UtcToLocalTimeConverter : ValueConverter<DateTime,string>
    {
        protected override string Convert(DateTime value, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime localTime = value.ToLocalTime();
            return (!(parameter is string format)) ? localTime.ToString(culture) : localTime.ToString(format, culture);
        }
    }
}
