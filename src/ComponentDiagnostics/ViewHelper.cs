using System;
using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Helper methods for working with UI Factories.
    /// </summary>
    static class ViewHelper
    {
        /// <summary>
        /// Create an element from a UI factory and bind it to the given data model.
        /// If an error occurs, then create a suitable view to display the error message.
        /// </summary>
        /// <param name="factory">GUID part of the UI Element identifier</param>
        /// <param name="elementId">DWORD part of the UI Element identifier</param>
        /// <param name="model">The new view's model</param>
        /// <returns>A WPF FrameworkElement that is bound to the given model, or view displaying
        /// an error message.</returns>
        public static FrameworkElement CreateModelBoundViewOrErrorMessage(Guid factory, uint elementId, object model)
        {
            try
            {
                IVsUIElement uiElement = UIFactoryHelper.GetViewFromUIFactory(factory, elementId);
                return UIFactoryHelper.ResolveWpfViewAndSetDataContext(uiElement, model);
            }
            catch (UIFactoryException ex)
            {
                Telemetry.Client.TrackException(ex);
                return CreateErrorMessageView(ex.Message);
            }
        }

        /// <summary>
        /// Create a special view to display an error message
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <returns>The error view</returns>
        public static FrameworkElement CreateErrorMessageView(string message)
        {
            IVsUIElement viewElement = UIFactoryHelper.GetViewFromUIFactory(GuidList.UiFactory, UIElementIds.ExceptionView);
            return UIFactoryHelper.ResolveWpfViewAndSetDataContext(viewElement, new ErrorDataSource(message));
        }
    }
}
