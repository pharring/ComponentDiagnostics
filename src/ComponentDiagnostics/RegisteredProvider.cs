using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class RegisteredProvider : Provider
    {
#pragma warning disable IDE0052 // Remove unread private members. Property registration is necessary for Gel interop.
        private static readonly GelProperty ModelProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(ModelProp, VsUIType.Unknown, __VSUIDATAFORMAT.VSDF_BUILTIN, GetModel);
        private static readonly GelProperty VersionProperty = GelProperty.RegisterIndirectProperty<RegisteredProvider>(VersionProp, VsUIType.DWord, __VSUIDATAFORMAT.VSDF_BUILTIN, GetVersion);
#pragma warning restore IDE0052 // Remove unread private members

        private IVsDiagnosticsProvider lazyDiagnosticsProvider;

        public RegisteredProvider(ProviderRegistration registration)
            : base(registration.guid, registration.name, registration.package, registration.viewDescriptor)
        {
        }

        private static object GetModel(GelDependencyObject owner)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RegisteredProvider me = (RegisteredProvider)owner;
            return me.Model;
        }

        public object Model
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return DiagnosticsProvider.DataModel;
            }
        }

        private static object GetVersion(GelDependencyObject owner)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RegisteredProvider me = (RegisteredProvider)owner;
            return me.Version;
        }

        public uint Version
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return DiagnosticsProvider.Version;
            }
        }

        private IVsDiagnosticsProvider DiagnosticsProvider => lazyDiagnosticsProvider ??= VsShellUtilities.GetPackageExtensionPoint<IVsDiagnosticsProvider, IVsDiagnosticsProvider>(Package, ProviderGuid);
    }

}
