using System;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Data model for an entry in the component diagnostics providers list
    /// </summary>
    abstract class Provider : GelDependencyObject
    {
        internal const string ProviderProp = "Provider";    // GUID
        internal const string NameProp = "Name";        // String
        internal const string PackageProp = "Package";      // GUID
        internal const string VersionProp = "Version";     // DWord
        internal const string ViewFactoryProp = "ViewFactory"; // GUID
        internal const string ViewProp = "View";        // DWord
        internal const string ModelProp = "Model";       // object

        static readonly GelProperty ProviderProperty    = GelProperty.RegisterGuidProperty<Provider>(ProviderProp);
        static readonly GelProperty NameProperty        = GelProperty.RegisterStringProperty<Provider>(NameProp);
        static readonly GelProperty PackageProperty     = GelProperty.RegisterGuidProperty<Provider>(PackageProp);
        static readonly GelProperty ViewFactoryProperty = GelProperty.RegisterGuidProperty<Provider>(ViewFactoryProp);
        static readonly GelProperty ViewProperty        = GelProperty.RegisterDwordProperty<Provider>(ViewProp);

        public Provider(
            Guid providerGuid,
            string name,
            Guid package,
            VsUIElementDescriptor viewDescriptor)
        {
            SetValue(ProviderProperty, providerGuid);
            SetValue(NameProperty, VsShellUtilities.LookupPackageString(package, name));
            SetValue(PackageProperty, package);
            SetValue(ViewFactoryProperty, viewDescriptor.Factory);
            SetValue(ViewProperty, viewDescriptor.ElementID);
        }

        public Guid ProviderGuid
        {
            get
            {
                return GetValue<Guid>(ProviderProperty);
            }
        }

        protected Guid Package
        {
            get
            {
                return GetValue<Guid>(PackageProperty);
            }
        }
    }
}
