using System;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class LoadReasonConverter : ValueConverter<int, string>
    {
        protected override string Convert(int value, object parameter, System.Globalization.CultureInfo culture)
        {
            switch((LoadPackageReasonPrivate)value)
            {
                case LoadPackageReasonPrivate.LR_AddStandardPreviewer:
                    return "Add Standard Previewer";

                case LoadPackageReasonPrivate.LR_Autoload:
                    return "AutoLoad";

                case LoadPackageReasonPrivate.LR_Automation:
                    return "Automation";

                case LoadPackageReasonPrivate.LR_CommandLineSwitch:
                    return "Command Line Switch";

                case LoadPackageReasonPrivate.LR_ComponentPicker:
                    return "Component Picker";

                case LoadPackageReasonPrivate.LR_DataConverter:
                    return "Data Converter";

                case LoadPackageReasonPrivate.LR_DataSourceFactory:
                    return "DataSource Factory";

                case LoadPackageReasonPrivate.LR_EditorFactory:
                    return "Editor Factory";

                case LoadPackageReasonPrivate.LR_ExecCmd:
                    return "Command Execution";

                case LoadPackageReasonPrivate.LR_ExtensionPoint:
                    return "Package Extension Point";

                case LoadPackageReasonPrivate.LR_FontsAndColors:
                    return "Fonts & Colors";

                case LoadPackageReasonPrivate.LR_HelpAbout:
                    return "Help/About";

                case LoadPackageReasonPrivate.LR_ImportExportSettings:
                    return "Import/Export Settings";

                case LoadPackageReasonPrivate.LR_Preload:
                    return "Preload";

                case LoadPackageReasonPrivate.LR_ProjectFactory:
                    return "Project Factory";

                case LoadPackageReasonPrivate.LR_QueryService:
                    return "QueryService";

                case LoadPackageReasonPrivate.LR_SolutionPersistence:
                    return "Solution Persistence";

                case LoadPackageReasonPrivate.LR_Toolbox:
                    return "Toolbox";

                case LoadPackageReasonPrivate.LR_ToolsOptions:
                    return "Tools/Options";

                case LoadPackageReasonPrivate.LR_Toolwindow:
                    return "Toolwindow";

                case LoadPackageReasonPrivate.LR_UIFactory:
                    return "UI Factory";

                case LoadPackageReasonPrivate.LR_Unknown:
                    return "Unknown (direct LoadPackage)";

                default:
                    return string.Format("Reason code {0}", value);
            }
        }
    }
}
