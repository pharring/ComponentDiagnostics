using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class RegisteredProvider : Provider
    {
        static readonly GelProperty ModelProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(Provider.ModelProp, VsUIType.Unknown, __VSUIDATAFORMAT.VSDF_BUILTIN, GetModel);
        static readonly GelProperty VersionProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(Provider.VersionProp, VsUIType.DWord, __VSUIDATAFORMAT.VSDF_BUILTIN, GetVersion);

        private IVsDiagnosticsProvider lazyDiagnosticsProvider;

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
                return DiagnosticsProvider.DataModel;
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
                return DiagnosticsProvider.Version;
            }
        }

        private IVsDiagnosticsProvider DiagnosticsProvider
        {
            get
            {
                return this.lazyDiagnosticsProvider ?? (this.lazyDiagnosticsProvider = VsShellUtilities.GetPackageExtensionPoint<IVsDiagnosticsProvider, IVsDiagnosticsProvider>(base.Package, base.ProviderGuid));
            }
        }
    }

}
