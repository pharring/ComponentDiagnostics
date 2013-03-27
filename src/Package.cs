using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    [Guid(GuidList.PackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)] // Matches the name given to the .cto included in VSPackage.resx
    [ProvideUIProvider(GuidList.UiFactoryString, "ComponentDiagnostics", GuidList.PackageString)]
    [ProvideToolWindow(typeof(ToolWindow))]
    [PackageRegistration(UseManagedResourcesOnly=true, RegisterUsing=RegistrationMethod.CodeBase)]
    [Description("Visual Studio Component Diagnostics Package")]
    [ProvideComponentDiagnostics(typeof(RdtDiagnosticsProvider), "Running Document Table", GuidList.UiFactoryString, UIElementIds.RdtView)]
    [ProvideComponentDiagnostics(typeof(UIContextDiagnosticsProvider), "Selection and UIContext", GuidList.UiFactoryString, UIElementIds.UIContextView)]
    [ProvideComponentDiagnostics(typeof(ScrollbarDiagnosticsProvider), "Scrollbar Theming", GuidList.UiFactoryString, UIElementIds.ScrollbarView)]
    public sealed class Package : Microsoft.VisualStudio.Shell.ExtensionPointPackage
    {
        static Package _instance;
        public static Package Instance
        {
            get { return _instance; }
        }

        protected override void Initialize()
        {
            _instance = this;
            base.Initialize();
            UIFactory.CreateAndRegister(this);
            AddCommandHandlers();
        }

        void AddCommandHandlers()
        {
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
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
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(ToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
