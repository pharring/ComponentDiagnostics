using System.ComponentModel;
using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class StatusIndicatorHost : WorkerThreadElementContainer
    {
        public StatusIndicatorHost()
        {
        }

        protected override string DispatcherGroup
        {
            get
            {
                return "OleStatusIndicatorThread";
            }
        }

        protected override System.Windows.UIElement CreateRootUIElement()
        {
            return new OleComponentStatusIndicator();
        }

        protected override int StackSize
        {
            get
            {
                return 256 * 1024; // 256KB
            }
        }

        protected override bool ShouldForwardPropertyChange(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TagProperty)
            {
                return true;
            }

            return base.ShouldForwardPropertyChange(e);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Short-circuit the normal size computation which involves
            // a round-trip to the contained element on the worker thread.
            return new Size(this.Width, this.Height);
        }
    }
}
