using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class RegisteredProvider : Provider
    {
        static readonly GelProperty ModelProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(Provider.ModelProp, VsUIType.Unknown, __VSUIDATAFORMAT.VSDF_BUILTIN, GetModel);
        static readonly GelProperty VersionProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(Provider.VersionProp, VsUIType.DWord, __VSUIDATAFORMAT.VSDF_BUILTIN, GetVersion);

        IVsDiagnosticsProvider diagnosticsProvider;

        public RegisteredProvider(ProviderRegistration registration)
            : base(registration.guid, registration.name, registration.package, registration.viewDescriptor)
        {
        }

        private static object GetModel(GelDependencyObject owner)
        {
            RegisteredProvider me = (RegisteredProvider)owner;
            return me.Model;
        }

        public object Model
        {
            get
            {
                EnsureDiagnosticsProvider();
                return this.diagnosticsProvider.DataModel;
            }
        }

        private static object GetVersion(GelDependencyObject owner)
        {
            RegisteredProvider me = (RegisteredProvider)owner;
            return me.Version;
        }

        public uint Version
        {
            get
            {
                EnsureDiagnosticsProvider();
                return this.diagnosticsProvider.Version;
            }
        }

        private void EnsureDiagnosticsProvider()
        {
            if (this.diagnosticsProvider == null)
            {
                this.diagnosticsProvider = VsShellUtilities.GetPackageExtensionPoint<IVsDiagnosticsProvider, IVsDiagnosticsProvider>(base.Package, base.ProviderGuid);
            }
        }
    }

}
