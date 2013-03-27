using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class ServiceDataModel : GelDependencyObject
    {
        static readonly GelProperty VersionProperty = GelProperty.RegisterDwordProperty<ServiceDataModel>("Version");
        static readonly GelProperty ProvidersProperty = GelProperty.RegisterCollectionProperty<ServiceDataModel>("Providers");

        public uint Version
        {
            get { return GetValue<uint>(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public IVsUICollection Providers
        {
            get { return GetValue<IVsUICollection>(ProvidersProperty); }
            set { SetValue(ProvidersProperty, value); }
        }

        internal static ServiceDataModel CreateInstance()
        {
            ServiceDataModel dataModel = new ServiceDataModel();

            UIDataSourceCollection providers = new UIDataSourceCollection();

            // Pre-populate the Providers collection with known providers from
            // the settings store.
            foreach (ProviderRegistration provider in Settings.RegisteredProviders)
            {
                providers.AddItem(new RegisteredProvider(provider));
            }

            dataModel.Providers = providers;

            return dataModel;
        }
    }
}
