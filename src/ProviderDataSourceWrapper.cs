using System;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Lightweight wrapper around a DataSource for a single diagnostics provider
    /// </summary>
    class ProviderDataSourceWrapper
    {
        IVsUIDataSource ds;

        public ProviderDataSourceWrapper(IVsUIDataSource datasource)
        {
            this.ds = datasource;
        }

        T GetValue<T>(string property)
        {
            return Utilities.QueryTypedValue<T>(this.ds, property);
        }

        object GetValue(string property)
        {
            return Utilities.QueryValue(this.ds, property);
        }

        public VsUIElementDescriptor ViewDescriptor
        {
            get
            {
                return new VsUIElementDescriptor()
                {
                    Factory = GetValue<Guid>(Provider.ViewFactoryProp),
                    ElementID = GetValue<uint>(Provider.ViewProp)
                };
            }
        }

        public object ViewModel
        {
            get
            {
                return GetValue(Provider.ModelProp);
            }
        }

        public string Name
        {
            get
            {
                return GetValue<string>(Provider.NameProp);
            }
        }
    }
}
