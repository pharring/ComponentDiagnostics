using System;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class UIFactory : WpfUIFactory
    {
        public UIFactory(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public static void CreateAndRegister(IServiceProvider serviceProvider)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            IVsUIFactory factory = new UIFactory(serviceProvider);

            IVsRegisterUIFactories registry = (IVsRegisterUIFactories)serviceProvider.GetService(typeof(SVsUIFactory));
            Assumes.Present(registry);

            ErrorHandler.ThrowOnFailure(registry.RegisterUIFactory(GuidList.UiFactory, factory));
        }

        protected override WpfUIFactoryElement[] GetFactoryElements()
        {
            return new WpfUIFactoryElement[]
            {
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.DefaultProvider,         typeof(DefaultProviderView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.ToolWindowView,          typeof(View)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.ExceptionView,           typeof(ExceptionView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.PackageManagerView,      typeof(PackageManagerView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.OleComponentManagerView, typeof(OleComponentView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.FileChangeServiceView,   typeof(FileChangeServiceView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.NavigationHistoryView,   typeof(NavigationHistoryView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.TaskSchedulerView,       typeof(TaskSchedulerServiceView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.RdtView,                 typeof(RdtView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.UIContextView,           typeof(UIContextView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.ScrollbarView,           typeof(ScrollbarView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.PackageCostView,         typeof(PackageCostTrackerView)),
                new WpfUIFactoryElement(GuidList.UiFactory, UIElementIds.WindowFramesView,        typeof(WindowFramesView)),
            };
        }
    }
}
