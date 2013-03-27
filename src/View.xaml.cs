using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Messages = Microsoft.VisualStudio.ComponentDiagnostics.Resources;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : UserControl
    {
        VsUIElementDescriptor currentViewDescriptor;
        IVsUIElement currentViewElement;

        public View()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the selection changes in the providers list, view on the right needs to change.
        /// The view presenter's DataContext is bound to the selected item. Select the appropriate
        /// view based on the "ViewFactory" and "View" fields in that DataContext and update the
        /// content. The content's own DataContext is set to the "Model" property.
        /// </summary>
        /// <param name="sender">The ContentControl for which the DataContext is changing</param>
        /// <param name="e">Event arguments containing the old and new DataContexts</param>
        private void viewPresenter_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Schedule the change for the next idle instead of changing immediately.

            // This is to work around a problem with the Document Tab Well's diagnostics provider
            // which throws an exception if it happens to be selected when the Component Diagnostics
            // tool window is first shown. Showing the tool window changes an invariant in the diagnostics
            // document tab well's diagnostics provider.
            this.Dispatcher.BeginInvoke(
                new Action(() => SelectNewViewPresenter(e.NewValue as IVsUIDataSource, (ContentControl)sender)),
                DispatcherPriority.ApplicationIdle
                );
        }

        private void SelectNewViewPresenter(IVsUIDataSource dataContext, ContentControl contentHost)
        {
            try
            {
                ProviderDataSourceWrapper providerModel = new ProviderDataSourceWrapper(dataContext);

                VsUIElementDescriptor newViewDescriptor = GetAppropriateViewDescriptor(providerModel);

                // Decide if the view should change, or if we can just update the datasource on the existing view.
                if (ShouldChangeView(newViewDescriptor))
                {
                    // Update the new content and Dispose of the old
                    using (IDisposable oldContent = contentHost.Content as IDisposable)
                    {
                        SetNewView(contentHost, newViewDescriptor, providerModel.ViewModel);
                    }
                }
                else
                {
                    // Use the same view, just update the data context.
                    UpdateCurrentViewElementModel(contentHost, providerModel.ViewModel);
                }
            }
            catch (ViewCreationException ex)
            {
                HandleViewCreationException(contentHost, ex);
            }
            catch (UIFactoryException ex)
            {
                HandleViewCreationException(contentHost, ex);
            }
        }

        private void UpdateCurrentViewElementModel(ContentControl contentHost, object viewModel)
        {
            // TODO: IVsUIElement.put_DataSource should accept any object, not just Gel datamodels.
            //       At which point, this reduces to just "currentViewElement.put_DataSource(viewModel)"

            IVsUISimpleDataSource gelModel = viewModel as IVsUISimpleDataSource;

            if (gelModel != null)
            {
                ErrorHandler.ThrowOnFailure(this.currentViewElement.put_DataSource(gelModel));
            }
            else
            {
                // Not a Gel model, so try to set the data context directly.

                // First clear the existing data model via put_DataSource.
                // This is so that any subsequent changes will get past
                // the check in WpfUIElement's DataSource setter to see if
                // the value has actually changed.
                this.currentViewElement.put_DataSource(new EmptyDataModel());

                FrameworkElement view = contentHost.Content as FrameworkElement;
                if (view != null)
                {
                    view.DataContext = viewModel;
                }
            }
        }

        private void HandleViewCreationException(ContentControl contentHost, Exception ex)
        {
            this.currentViewElement = null;
            contentHost.Content = ViewHelper.CreateErrorMessageView(ex.Message);
        }

        private void SetNewView(ContentControl contentHost, VsUIElementDescriptor viewDescriptor, object viewModel)
        {
            IVsUIElement uiElement = UIFactoryHelper.GetViewFromUIFactory(viewDescriptor);

            contentHost.Content =
                (uiElement == null)
                ? this.EmptyView
                : UIFactoryHelper.ResolveWpfViewAndSetDataContext(uiElement, viewModel);

            this.currentViewDescriptor = viewDescriptor;
            this.currentViewElement = uiElement;
        }

        /// <summary>
        /// When the ContentControl is unloaded, dispose of its Content
        /// </summary>
        private void viewPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            // Dispose of the Content if it's IDisposable
            ContentControl contentHost = (ContentControl)sender;
            using (IDisposable oldContent = contentHost.Content as IDisposable)
            {
                contentHost.Content = null;
            }

            // Subscribe to Loaded event in case the control is reloaded.
            // In VS this often happens when toolwindows are manipulated
            // e.g. hidden, then shown; docked then floated.
            contentHost.Loaded += viewPresenter_Reloaded;
        }

        /// <summary>
        /// When the ContentControl is reloaded, recreate the current view
        /// </summary>
        void viewPresenter_Reloaded(object sender, RoutedEventArgs e)
        {
            ContentControl contentHost = (ContentControl)sender;
            contentHost.Loaded -= viewPresenter_Reloaded;

            try
            {
                ProviderDataSourceWrapper providerModel = new ProviderDataSourceWrapper(contentHost.DataContext as IVsUIDataSource);
                VsUIElementDescriptor viewDescriptor = GetAppropriateViewDescriptor(providerModel);
                SetNewView(contentHost, viewDescriptor, providerModel.ViewModel);
            }
            catch (ViewCreationException ex)
            {
                HandleViewCreationException(contentHost, ex);
            }
            catch (UIFactoryException ex)
            {
                HandleViewCreationException(contentHost, ex);
            }
        }

        /// <summary>
        /// An empty Gel object to pass to an IVsUIElement in order to clear
        /// the current data source.
        /// </summary>
        class EmptyDataModel : IVsUISimpleDataSource
        {
            public int Close()
            {
                return VSConstants.E_NOTIMPL;
            }

            public int EnumVerbs(out IVsUIEnumDataSourceVerbs ppEnum)
            {
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            public int Invoke(string verb, object pvaIn, out object pvaOut)
            {
                pvaOut = null;
                return VSConstants.E_NOTIMPL;
            }
        }

        /// <summary>
        /// A view representing an unselected provider.
        /// </summary>
        FrameworkElement EmptyView
        {
            get
            {
                return this.FindResource("noProviderView") as FrameworkElement;
            }
        }

        /// <summary>
        /// The view descriptor for the default view (Property Grid)
        /// </summary>
        static VsUIElementDescriptor DefaultViewDescriptor
        {
            get
            {
                return new VsUIElementDescriptor()
                {
                    Factory = GuidList.UiFactory,
                    ElementID = UIElementIds.DefaultProvider
                };
            }
        }

        /// <summary>
        /// Given the diagnostics data model of a single Component Diagnostics provider,
        /// select the appropriate view. If the "ViewFactory" property contains a Guid
        /// then use that, along with the "View" field to create a new UI element from
        /// a UI factory.
        /// </summary>
        /// <param name="provider">Data model of the selected provider</param>
        /// <returns>A descriptor of the view to display</returns>
        /// <exception cref="ArgumentNullException">The provider is null</exception>
        /// <exception cref="ViewCreationException">The view could not be created for the
        /// reason given in the exception message</exception>
        static VsUIElementDescriptor GetAppropriateViewDescriptor(ProviderDataSourceWrapper provider)
        {
            if (provider == null)
            {
                throw new ViewCreationException(Messages.ProviderIsNull);
            }

            VsUIElementDescriptor viewDescriptor = provider.ViewDescriptor;

            // If no factory was specified, then use the default view
            if (viewDescriptor.Factory == Guid.Empty)
            {
                viewDescriptor = DefaultViewDescriptor;
            }

            return viewDescriptor;
        }

        /// <summary>
        /// Determine if the view needs to change.
        /// </summary>
        /// <param name="newViewDescriptor">The new view descriptor determined by calling GetAppropriateViewDescriptor</param>
        /// <returns>true if the view should change, false if the view should stay the same</returns>
        bool ShouldChangeView(VsUIElementDescriptor newViewDescriptor)
        {
            return this.currentViewElement == null || !this.currentViewDescriptor.Equals(newViewDescriptor);
        }
    }
}
