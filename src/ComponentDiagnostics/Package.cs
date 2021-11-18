using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    [Guid(GuidList.PackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)] // Matches the name given to the .cto included in VSPackage.resx
    [ProvideUIProvider(GuidList.UiFactoryString, "ComponentDiagnostics", GuidList.PackageString)]
#pragma warning disable VSSDK003 // Support async tool windows
    [ProvideToolWindow(typeof(ToolWindow))]
#pragma warning restore VSSDK003 // Support async tool windows
    [PackageRegistration(UseManagedResourcesOnly=true, RegisterUsing=RegistrationMethod.CodeBase, AllowsBackgroundLoading = true)]
    [Description("Visual Studio Component Diagnostics Package")]
    [ProvideComponentDiagnostics(typeof(RdtDiagnosticsProvider), "Running Document Table", GuidList.UiFactoryString, UIElementIds.RdtView)]
    [ProvideComponentDiagnostics(typeof(UIContextDiagnosticsProvider), "Selection and UIContext", GuidList.UiFactoryString, UIElementIds.UIContextView)]
    [ProvideComponentDiagnostics(typeof(ScrollbarDiagnosticsProvider), "Scrollbar Theming", GuidList.UiFactoryString, UIElementIds.ScrollbarView)]
    [ProvideComponentDiagnostics(typeof(WindowFramesDiagnosticsProvider), "Window Frames", GuidList.UiFactoryString, UIElementIds.WindowFramesView)]
    public sealed class Package : ExtensionPointAsyncPackage
    {
        public static Package Instance { get; private set; }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;

            await base.InitializeAsync(cancellationToken, progress);

            await base.JoinableTaskFactory.SwitchToMainThreadAsync();

            Telemetry.Client.TrackEvent("Package.Initialize", Telemetry.CreateProperties("VSVersion", GetVSVersion()));
            UIFactory.CreateAndRegister(this);
            AddCommandHandlers();
        }

        private string GetVSVersion()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (GetService(typeof(SVsShell)) is IVsShell shell)
            {
                if (ErrorHandler.Succeeded(shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object obj)) && obj != null)
                {
                    return obj.ToString();
                }
            }

            return "Unknown";
        }

        void AddCommandHandlers()
        {
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.CommandSet, (int)PkgCmdID.cmdidViewComponentDiagnosticsToolWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
        }

        /// <summary>
        /// Show our tool window.
        /// </summary>
        void ShowToolWindow(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Telemetry.Client.TrackEvent("ShowToolWindow");

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(ToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
