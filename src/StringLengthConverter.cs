using System.Globalization;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class StringLengthConverter : ValueConverter<string, string>
    {
        protected override string Convert(string value, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "0";

            return value.Length.ToString();
        }
    }
}
