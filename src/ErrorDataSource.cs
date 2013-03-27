using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// A data source which contains an error message
    /// </summary>
    class ErrorDataSource : GelDependencyObject
    {
        static readonly GelProperty MessageProperty = GelProperty.RegisterStringProperty<ErrorDataSource>("Message");

        public ErrorDataSource(string message)
        {
            SetValue(MessageProperty, message);
        }
    }
}
