using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    [Guid(GuidList.ToolWindowString)]
    public class ToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindow() :
            base(null)
        {
            this.Caption = Resources.ToolWindowTitle;

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 0;
        }

        /// <summary>
        /// Override the Content property so that it can be set on first access.
        /// </summary>
        public override object Content
        {
            get
            {
                Shell.ThreadHelper.ThrowIfNotOnUIThread();

                // Create on first access
                if (base.Content == null)
                {
                    base.Content = CreateView();
                }

                return base.Content;
            }
            set
            {
                base.Content = value;
            }
        }

        /// <summary>
        /// Creates the Toolwindow's View and binds it to the Component Diagnostics data model.
        /// On failure, a suitable view is created to display the message in the toolwindow.
        /// </summary>
        /// <returns>A view for the toolwindow or error message as appropriate.</returns>
        static FrameworkElement CreateView()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // CONSIDER: It has been suggested that this data model could be exposed as a
            // DataSource (via a DataSource factory) or even a DiagnosticsProvider itself.
            ServiceDataModel model = ServiceDataModel.CreateInstance();
            return ViewHelper.CreateModelBoundViewOrErrorMessage(GuidList.UiFactory, UIElementIds.ToolWindowView, model);
        }
    }
}
