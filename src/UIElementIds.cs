namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// UI element identifiers for elements that UIFactory can create
    /// </summary>
    static class UIElementIds
    {
        /// <summary>
        /// The Component Diagnostics tool window
        /// </summary>
        public const uint ToolWindowView = 1u;

        /// <summary>
        /// The default provider view (wraps a WinForms property browser)
        /// </summary>
        public const uint DefaultProvider = 2u;

        /// <summary>
        /// A view which shows an error message
        /// </summary>
        public const uint ExceptionView = 3u;

        /// <summary>
        /// A view for the package manager provider
        /// </summary>
        public const uint PackageManagerView = 4u;

        /// <summary>
        /// A view for the OLE Component Manager
        /// </summary>
        public const uint OleComponentManagerView = 5u;

        /// <summary>
        /// A view for the file change service
        /// </summary>
        public const uint FileChangeServiceView = 6u;

        /// <summary>
        /// A view for navigation history
        /// </summary>
        public const uint NavigationHistoryView = 7u;

        /// <summary>
        /// A view for task scheduler service
        /// </summary>
        public const uint TaskSchedulerView = 8u;

        /// <summary>
        /// A view for Running Document Table service
        /// </summary>
        public const uint RdtView = 9u;

        /// <summary>
        /// A view for UIContext state, selection and history for both
        /// </summary>
        public const uint UIContextView = 10u;

        /// <summary>
        /// A view for native scrollbar theming
        /// </summary>
        public const uint ScrollbarView = 11u;

        
        /// <summary>
        /// A view for package cost monitoring service
        /// </summary>
        public const uint PackageCostView = 12u;
    }
}
