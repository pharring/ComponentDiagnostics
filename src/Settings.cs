using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Reads the list of pre-registered diagnostics providers and their associated view descriptors
    /// from the settings store.
    /// 
    /// Pre-registering a diagnostics provider allows for shipping updated views out-of-band.
    /// 
    /// The following diagram explains the layout in the settings store:
    /// 
    /// [DiagnosticsProviders]
    ///  +--[{ComponentOneProvider}]                    ; GUID of 1st diagnostics provider
    ///  |  |   (Default)        "Component One"        ; Display name of 1st provider
    ///  |  |                                           ; May also be a resource ID (e.g. #1001)
    ///  |  |   Package          {ComponentOnePackage}  ; GUID of the ComponentOne package which
    ///  |  |                                           ; implements a component diagnostics
    ///  |  |                                           ; extension point.
    ///  |  |
    ///  |  +--[Views]                                  ; Optional list of registered views
    ///  |     |
    ///  |     +--[1]                                   ; Version of data model in decimal
    ///  |     |   ViewFactory    "{ViewFactoryGuid1}"  ; GUID of the UI factory for view version 1
    ///  |     |   View           0x00000100 (DWORD)    ; Element ID of the view for view version 1
    ///  |     |
    ///  |     +--[2]                                   ; Version of the data model in decimal
    ///  |         ViewFactory    "{ViewFactoryGuid2}"  ; GUID of the UI factory for view version 2
    ///  |         View           0xFECBA987 (DWORD)    ; Element ID of the view for view version 2
    ///  | 
    ///  +--[{ComponentTwoProvider}]                    ; GUID of 2nd diagnostics provider
    ///     |   (Default)        "Widget Manager"       ; Readable name of 2nd provider
    ///     |   Package          {WidgetPackage}        ; GUID of the Widget package which
    ///     |                                           ; implements a component diagnostics
    ///     |                                           ; extension point.
    ///     |
    ///  |  +--[Views]                                  ; Optional list of registered views
    ///        |
    ///         +--[8]                                  ; Version of the data model in decimal
    ///             ViewFactory   "{ViewFactoryGuid3}"  ; GUID of the UI factory for view version 8
    ///             View          0x00000001 (DWORD)    ; Element ID of the view for view version 8
    ///  
    /// In this example, two diagnostics providers are registered. The first one defines view for both 
    /// version 1 and version 2 of its data model. The second provider defines a view only for version
    /// 8. All three of these views are registered and the most appropriate view will be selected based
    /// on the version of the provider's data model. If a view for the requested version is not available
    /// then the default view (a property grid) will be used instead.
    /// 
    /// The Views list is optional. If the sub-key doesn't exist, then a default view will be supplied
    /// which will match version "0" of the data model.
    /// 
    /// The tree of versions is flattened so that we end up with a simple collection of "versioned providers"
    /// </summary>
    class Settings
    {
        const string RegisteredProvidersCollectionName = "DiagnosticsProviders"; // collection in the settings store for pre-registered providers

        static IVsSettingsStore ReadOnlyConfigSettings
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                IVsSettingsManager manager = ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsManager)) as IVsSettingsManager;
                Assumes.Present(manager);

                ErrorHandler.ThrowOnFailure(manager.GetReadOnlySettingsStore((uint)__VsSettingsScope.SettingsScope_Configuration, out IVsSettingsStore store));
                return store;
            }
        }

        IVsSettingsStore Store { get; set; }

        // Private constructor
        private Settings()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.Store = ReadOnlyConfigSettings;
        }

        /// <summary>
        /// Given a path into the settings store, enumerate all its sub-keys
        /// </summary>
        /// <param name="store">The settings store</param>
        /// <param name="path">The path</param>
        /// <returns>Enumeration of subkey names</returns>
        IEnumerable<string> GetSubKeys(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int hr = this.Store.GetSubCollectionCount(path, out var subCollectionCount);
            if (hr == VSConstants.E_INVALIDARG)
            {
                // Return an empty collection if the sub-collection is missing entirely
                yield break;
            }
            else
            {
                ErrorHandler.ThrowOnFailure(hr);
            }

            for (uint i = 0; i != subCollectionCount; ++i)
            {
                ErrorHandler.ThrowOnFailure(this.Store.GetSubCollectionName(path, i, out var subCollectionName));

                yield return subCollectionName;
            }
        }

        /// <summary>
        /// Try to parse view information for a specific version of a diagnostics provider.
        /// </summary>
        /// <param name="viewsPath">Full path to the Views subkey under a single diagnostics provider</param>
        /// <param name="versionName">A subkey beneath the viewsPath</param>
        /// <param name="provider">The provider registration which will be filled in by this.</param>
        /// <returns>true if the view information was parsed successfully</returns>
        bool TryReadVersionedProvider(string viewsPath, string versionName, ref ProviderRegistration provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!uint.TryParse(versionName, out provider.version))
            {
                Debug.WriteLine("WARNING: Diagnostics provider '{0}' has a versioned View '{1}' which is not a DWORD", viewsPath, versionName);
                return false;
            }

            string versionedViewPath = viewsPath + "\\" + versionName;

            if (ErrorHandler.Failed(this.Store.GetString(versionedViewPath, "ViewFactory", out var viewFactory)))
            {
                // ViewFactory is optional (we'll use a generic view), so this isn't an error.
            }
            else
            {
                if (!Guid.TryParseExact(viewFactory, "B", out provider.viewDescriptor.Factory))
                {
                    Debug.WriteLine("WARNING: Diagnostics provider '{0}' has a ViewFactory which is not a GUID in the correct format", versionedViewPath);
                }
                else
                {
                    // Only if we have a valid view factory do we try to read the view's DWORD
                    if (ErrorHandler.Failed(this.Store.GetUnsignedInt(versionedViewPath, "View", out provider.viewDescriptor.ElementID)))
                    {
                        Debug.WriteLine("WARNING: Diagnostics provider '{0}' has a valid ViewFactory but an invalid or missing View DWORD", versionedViewPath);

                        // Continue, but without a view factory
                        provider.viewDescriptor.Factory = Guid.Empty;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Given a diagnostics provider path, create registration records for all the views associated with that provider.
        /// </summary>
        /// <param name="store">The settings store</param>
        /// <param name="providerPath">Path into the settings store representing a diagnostics provider (i.e. a subkey
        /// under the DiagnosticsProvider key)</param>
        /// <returns>Enumerator for a flattened list of ProviderRegistration records corresponding to the views for
        /// the given provider.</returns>
        IEnumerable<ProviderRegistration> GetRegisteredViews(string providerPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProviderRegistration provider = new ProviderRegistration();

            // Check that the provider is a GUID and that it has a sensible default value (name)
            if (!Guid.TryParseExact(providerPath, "B", out provider.guid))
            {
                Debug.WriteLine("WARNING: Diagnostics provider '{0}' is not a valid GUID", providerPath);
                yield break;
            }

            string fullProviderPath = RegisteredProvidersCollectionName + '\\' + providerPath;

            if (ErrorHandler.Failed(this.Store.GetString(fullProviderPath, String.Empty, out provider.name)))
            {
                Debug.WriteLine("WARNING: Diagnostics provider '{0}' has an invalid or missing name", providerPath);
                yield break;
            }

            if (ErrorHandler.Failed(this.Store.GetString(fullProviderPath, "Package", out var packageString)))
            {
                Debug.WriteLine("WARNING: Diagnostics provider '{0}' has an invalid or missing package", providerPath);
                yield break;
            }

            if (!Guid.TryParseExact(packageString, "B", out provider.package))
            {
                Debug.WriteLine("WARNING: Diagnostics provider '{0}' specifies a package, {1}, which is not a valid GUID", packageString);
                yield break;
            }

            string viewsSubKey = fullProviderPath + "\\Views";

            // Enumerate the subkeys under "Views" looking for version-specific views
            bool foundAtLeastOneVersionedView = false;
            foreach (string versionName in GetSubKeys(viewsSubKey))
            {
                if (TryReadVersionedProvider(viewsSubKey, versionName, ref provider))
                {
                    foundAtLeastOneVersionedView = true;
                    yield return provider;
                }
            }

            // If there is no pre-registered view, then create one for the default view
            if (!foundAtLeastOneVersionedView)
            {
                provider.viewDescriptor.Factory = Guid.Empty;
                provider.viewDescriptor.ElementID = 0u;
                yield return provider;
            }
        }

        /// <summary>
        /// Read the list of pre-registered providers from the settings store.
        /// </summary>
        public static IEnumerable<ProviderRegistration> RegisteredProviders
        {
            get
            {
                Shell.ThreadHelper.ThrowIfNotOnUIThread();
                Settings settings = new Settings();

                // Loop over all providers under the DiagnosticsProviders key
                foreach (string subKeyName in settings.GetSubKeys(RegisteredProvidersCollectionName))
                {
                    // Loop over all the registered views for that provider
                    foreach (ProviderRegistration provider in settings.GetRegisteredViews(subKeyName))
                    {
                        yield return provider;
                    }
                }
            }
        }
    }
}
