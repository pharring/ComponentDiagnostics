using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class LoadReasonConverter : ValueConverter<int, string>
    {
        // Package load reason codes used by the shell
        // These values are passed into IVsShell5::LoadPackageWithContext 
        private enum LoadPackageReasonPrivate
        {
            LR_Unknown = -1,   // Direct call to IVsShell::LoadPackage
            LR_Preload = -2,   // Pre-load mechanism as used by vslog (obfuscated and undocumented)
            LR_Autoload = -3,   // Autoload through a UI context activation
            LR_QueryService = -4,   // IServiceProvider::QueryService
            LR_EditorFactory = -5,   // Creating an editor
            LR_ProjectFactory = -6,   // Creating a project system
            LR_Toolwindow = -7,   // Creating a tool window
            LR_ExecCmd = -8,   // IOleCommandTarget::ExecCmd
            LR_ExtensionPoint = -9,   // In order to find an extension point (export)
            LR_UIFactory = -10,  // UI factory
            LR_DataSourceFactory = -11,  // Datasource factory
            LR_Toolbox = -12,  // Toolbox
            LR_Automation = -13,  // Automation (GetAutomationObject)
            LR_HelpAbout = -14,  // Help/About information
            LR_AddStandardPreviewer = -15,  // AddStandardPreviewer (browser) support
            LR_ComponentPicker = -16,  // Component picker (IVsComponentSelectorProvider)
            LR_SolutionPersistence = -17,  // IVsSolutionProps
            LR_FontsAndColors = -18,  // QueryService call for IVsTextMarkerTypeProvider.
            LR_CommandLineSwitch = -19,  // DemandLoad specified on AppCommandLoad
            LR_DataConverter = -20,  // UIDataConverter
            LR_ToolsOptions = -21,  // A page in Tools/Options
            LR_ImportExportSettings = -22,  // Import/Export settings
        };

        protected override string Convert(int value, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((LoadPackageReasonPrivate)value) switch
            {
                LoadPackageReasonPrivate.LR_AddStandardPreviewer => "Add Standard Previewer",
                LoadPackageReasonPrivate.LR_Autoload => "AutoLoad",
                LoadPackageReasonPrivate.LR_Automation => "Automation",
                LoadPackageReasonPrivate.LR_CommandLineSwitch => "Command Line Switch",
                LoadPackageReasonPrivate.LR_ComponentPicker => "Component Picker",
                LoadPackageReasonPrivate.LR_DataConverter => "Data Converter",
                LoadPackageReasonPrivate.LR_DataSourceFactory => "DataSource Factory",
                LoadPackageReasonPrivate.LR_EditorFactory => "Editor Factory",
                LoadPackageReasonPrivate.LR_ExecCmd => "Command Execution",
                LoadPackageReasonPrivate.LR_ExtensionPoint => "Package Extension Point",
                LoadPackageReasonPrivate.LR_FontsAndColors => "Fonts & Colors",
                LoadPackageReasonPrivate.LR_HelpAbout => "Help/About",
                LoadPackageReasonPrivate.LR_ImportExportSettings => "Import/Export Settings",
                LoadPackageReasonPrivate.LR_Preload => "Preload",
                LoadPackageReasonPrivate.LR_ProjectFactory => "Project Factory",
                LoadPackageReasonPrivate.LR_QueryService => "QueryService",
                LoadPackageReasonPrivate.LR_SolutionPersistence => "Solution Persistence",
                LoadPackageReasonPrivate.LR_Toolbox => "Toolbox",
                LoadPackageReasonPrivate.LR_ToolsOptions => "Tools/Options",
                LoadPackageReasonPrivate.LR_Toolwindow => "Toolwindow",
                LoadPackageReasonPrivate.LR_UIFactory => "UI Factory",
                LoadPackageReasonPrivate.LR_Unknown => "Unknown (direct LoadPackage)",
                _ => string.Format("Reason code {0}", value),
            };
        }
    }
}
