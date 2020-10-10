using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class SelectionData : DependencyObject, IVsSelectionEvents, IDisposable
    {
        const string settingsRoot = "SelectionMonitorTool";
        const string noNameProvided = "No name provided";

        Dictionary<uint, UIContextInformation> contextIDNames = new Dictionary<uint, UIContextInformation>();

        uint selectionEventsCookie = VSConstants.VSCOOKIE_NIL;

        private static readonly bool IsRunningOnDev12 = InitializeIsRunningOnDev12();

        private static bool InitializeIsRunningOnDev12()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Do a QueryService for IVsShell6 which was introduced in Dev12
            object shellService = ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            IntPtr punk = Marshal.GetIUnknownForObject(shellService);
            try
            {
                Guid IID_IVsShell6 = new Guid("D111DB4B-584E-4F93-BCEC-5F7E0990E9E7");
                IntPtr punkShell6;
                if (Marshal.QueryInterface(punk, ref IID_IVsShell6, out punkShell6) == VSConstants.S_OK)
                {
                    Marshal.Release(punkShell6);
                    return true;
                }
            }
            finally
            {
                Marshal.Release(punk);
            }

            return false;
        }


        private ObservableCollection<UIContextInformation> liveContexts = new ObservableCollection<UIContextInformation>();
        public ObservableCollection<UIContextInformation> LiveContexts { get { return liveContexts; } }

        private ObservableCollection<UIContextInformation> favoriteContexts = new ObservableCollection<UIContextInformation>();
        public ObservableCollection<UIContextInformation> FavoriteContexts { get { return favoriteContexts; } }

        private ObservableCollection<UIContextLogItem> uiContextLog = new ObservableCollection<UIContextLogItem>();
        public ObservableCollection<UIContextLogItem> UIContextLog { get { return uiContextLog; } }

        private ObservableCollection<SelectionLogItem> selectionLog = new ObservableCollection<SelectionLogItem>();
        public ObservableCollection<SelectionLogItem> SelectionLog { get { return selectionLog; } }

        private ObservableCollection<SelectionItemInfo> selectionItems = new ObservableCollection<SelectionItemInfo>();
        public ObservableCollection<SelectionItemInfo> SelectionItems { get { return selectionItems; } }


        private IVsMonitorSelection selectionMonitor;
        internal SelectionData()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            selectionMonitor = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            InitializeSelectionItems();
            InitializeContextDictionary();

            IVsSettingsStore settingsStore;
            IVsSettingsManager userSettings = Package.GetGlobalService(typeof(SVsSettingsManager)) as IVsSettingsManager;
            userSettings.GetReadOnlySettingsStore((uint)__VsSettingsScope.SettingsScope_UserSettings, out settingsStore);

            LoadContextIDList(settingsStore, favoriteContexts, "Favorites");

            // at the point where we are created, we don't know what contexts are live so we need to look
            // for all of them that we know about.
            foreach (uint contextID in contextIDNames.Keys)
            {
                int active;
                if (ErrorHandler.Succeeded(selectionMonitor.IsCmdUIContextActive(contextID, out active)) && active != 0)
                {
                    // Add the item to the live contexts

                    LiveContexts.Add(contextIDNames[contextID]);
                    contextIDNames[contextID].Enabled = true;
                }
            }

            selectionMonitor.AdviseSelectionEvents(this, out selectionEventsCookie);
        }

        /// <summary>
        /// Save a list of UIContext items in the settings store.
        /// </summary>
        /// <param name="settingsStore"></param>
        /// <param name="list"></param>
        /// <param name="name"></param>
        private void SaveContextIDList(IVsWritableSettingsStore settingsStore, ObservableCollection<UIContextInformation> list, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string collectionRoot = settingsRoot + "\\" + name;

            // rewrite the collection
            settingsStore.DeleteCollection(collectionRoot);
            settingsStore.CreateCollection(collectionRoot);

            for (int iContext = 0; iContext < list.Count; iContext++)
            {
                string guidString = list[iContext].Guid.ToString();
                settingsStore.SetString(collectionRoot, guidString, list[iContext].Name);
            }
        }

        /// <summary>
        /// Load a list of UIContext items from the settings store.
        /// </summary>
        /// <param name="settingsStore"></param>
        /// <param name="list"></param>
        /// <param name="name"></param>
        private void LoadContextIDList(IVsSettingsStore settingsStore, ObservableCollection<UIContextInformation> list, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string collectionRoot = settingsRoot + "\\" + name;

            int exists;
            settingsStore.CollectionExists(collectionRoot, out exists);
            if (exists != 0)
            {
                uint contextCount;

                settingsStore.GetPropertyCount(collectionRoot, out contextCount);
                for (uint iContext = 0; iContext < contextCount; iContext++)
                {
                    string guidString;
                    if (ErrorHandler.Succeeded(settingsStore.GetPropertyName(collectionRoot, iContext, out guidString)))
                    {
                        Guid contextGuid = new Guid(guidString);
                        uint contextID;
                        if (ErrorHandler.Succeeded(selectionMonitor.GetCmdUIContextCookie(ref contextGuid, out contextID)))
                        {
                            if (contextIDNames.ContainsKey(contextID))
                            {
                                list.Add(contextIDNames[contextID]);
                                if (contextIDNames[contextID].Name.StartsWith("#") ||
                                    contextIDNames[contextID].Name.StartsWith("resource="))
                                {
                                    // if the name is a resource, use the name we found from the last session
                                    string contextName;
                                    if (ErrorHandler.Succeeded(settingsStore.GetString(collectionRoot, guidString, out contextName)))
                                    {
                                        contextIDNames[contextID].Name = contextName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        void AddContextName(string contextGuidString, string contextName)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            AddContextName(contextGuidString, contextName, null);
        }

        void AddContextName(string contextGuidString, string contextName, string packageGuidString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (Guid.TryParse(contextGuidString, out Guid contextGuid) && ErrorHandler.Succeeded(selectionMonitor.GetCmdUIContextCookie(ref contextGuid, out uint guidCookie)))
            {
                contextIDNames.TryGetValue(guidCookie, out UIContextInformation info);
                if (info == null)
                    contextIDNames[guidCookie] = new UIContextInformation(guidCookie, contextName, contextGuid.ToString("B"), packageGuidString);
                else
                {
                    if (info.Name == noNameProvided)
                        info.Name = contextName;
                }
            }
        }

        /// <summary>
        /// Find the current registry root being used by this instance of VS
        /// </summary>
        /// <returns></returns>
        RegistryKey GetRegistryRoot()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ILocalRegistry4 localReg = Package.GetGlobalService(typeof(SLocalRegistry)) as ILocalRegistry4;
            ErrorHandler.ThrowOnFailure(localReg.GetLocalRegistryRootEx((uint)__VsLocalRegistryType.RegType_Configuration, out uint rootHandle, out string rootName));


            RegistryKey rootKey;
            if ((int)rootHandle == (int)__VsLocalRegistryRootHandle.RegHandle_LocalMachine)
                rootKey = Registry.LocalMachine.OpenSubKey(rootName);
            else if ((int)rootHandle == (int)__VsLocalRegistryRootHandle.RegHandle_CurrentUser)
                rootKey = Registry.CurrentUser.OpenSubKey(rootName);
            else
                throw new InvalidOperationException();

            return rootKey;
        }

        /// <summary>
        /// Create the list of selection elements for the selection view.  This includes the core slots as well as any
        /// registered extension provided selection elenents
        /// </summary>
        void InitializeSelectionItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsMonitorSelection selectionMonitor = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            object elementValue;
            string elementName;

            // Initialize the current selection objects
            IntPtr pHierarchy;
            IVsHierarchy hierarchy = null;
            uint itemID;
            IVsMultiItemSelect multiSelect;
            IntPtr pSelectionContainer;
            ISelectionContainer selectionContainer = null;

            selectionMonitor.GetCurrentSelection(out pHierarchy, out itemID, out multiSelect, out pSelectionContainer);
            if (pHierarchy != IntPtr.Zero)
                hierarchy = Marshal.GetObjectForIUnknown(pHierarchy) as IVsHierarchy;
            if (pSelectionContainer != IntPtr.Zero)
                selectionContainer = Marshal.GetObjectForIUnknown(pSelectionContainer) as ISelectionContainer;

            // Sync descriptions with selection by calling OnSelectionChanged
            selectionItems.Add(new SelectionItemInfo((VSConstants.SelectionElement)SelectionItemInfo.SpecialElement.Hierarchy, description:String.Empty, owner:String.Empty));
            selectionItems.Add(new SelectionItemInfo((VSConstants.SelectionElement)SelectionItemInfo.SpecialElement.ItemID, description: String.Empty, owner: String.Empty));
            selectionItems.Add(new SelectionItemInfo((VSConstants.SelectionElement)SelectionItemInfo.SpecialElement.SelectionContainer, description: String.Empty, owner: String.Empty));
            selectionItems.Add(new SelectionItemInfo((VSConstants.SelectionElement)SelectionItemInfo.SpecialElement.MultiItemSelect, description: String.Empty, owner: String.Empty));

            OnSelectionChanged(null, 0, null, null, hierarchy, itemID, multiSelect, selectionContainer);

            // Initialize the current selection data with the element values
            VSConstants.SelectionElement[] elements = {VSConstants.SelectionElement.WindowFrame,
                              VSConstants.SelectionElement.DocumentFrame,
                              VSConstants.SelectionElement.UndoManager,
                              VSConstants.SelectionElement.StartupProject,
                              VSConstants.SelectionElement.UserContext,
                              VSConstants.SelectionElement.PropertyBrowserSID,
                              VSConstants.SelectionElement.ResultList,
                              VSConstants.SelectionElement.LastWindowFrame
                              };

            foreach (VSConstants.SelectionElement element in elements)
            {
                selectionMonitor.GetCurrentElementValue((uint)element, out elementValue);
                object valueOwner = GetOwnerForSelectedElement(element);
                selectionItems.Add(new SelectionItemInfo(element, GetSelectionElementDescription(elementValue), GetSelectionElementDescription(valueOwner)));
            }

            // Read the VSIP registered selection elements

            // Skip this one, it is not supported yet and will always be empty
            Guid surfaceSelectionElement = new Guid("{64db9e55-5614-44b3-93c9-e617b95eeb5f}");

            IVsMonitorSelection2 selectionMonitor2 = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection2;
            using (RegistryKey rootKey = GetRegistryRoot())
            {
                using (RegistryKey elementsKey = rootKey.OpenSubKey("SelectionElements"))
                {
                    if (elementsKey != null)
                    {
                        string[] elementGuids = elementsKey.GetSubKeyNames();
                        foreach (string guidString in elementGuids)
                        {
                            using (RegistryKey elementKey = elementsKey.OpenSubKey(guidString))
                            {
                                Guid elementGuid = new Guid(guidString);
                                uint selElem;
                                selectionMonitor2.GetElementID(ref elementGuid, out selElem);

                                selectionMonitor.GetCurrentElementValue(selElem, out elementValue);
                                elementName = (string)elementKey.GetValue("Name", defaultValue: String.Empty);

                                // Skip this one, it is not supported yet and will always be empty
                                if (elementGuid == surfaceSelectionElement)
                                    continue;

                                object contextOwner = GetOwnerForSelectedElement((VSConstants.SelectionElement)selElem);

                                selectionItems.Add(new SelectionItemInfo((VSConstants.SelectionElement)selElem, elementName, GetSelectionElementDescription(elementValue), GetSelectionElementDescription(contextOwner)));
                            }
                        }
                    }
                }
            }
        }

        bool IsEmptySelectionContext(IVsTrackSelectionEx selCtx)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Validate.IsNotNull(selCtx, "selCtx");
            IVsMonitorSelection2 monitorSelection2 = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection2;
            IVsTrackSelectionEx emptySelCtxt;
            monitorSelection2.GetEmptySelectionContext(out emptySelCtxt);
            return ComUtilities.IsSameComObject(selCtx, emptySelCtxt);
        }

        object GetOwnerForSelectedElement(VSConstants.SelectionElement selElem)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            // Get the selection context object for propagator of the element
            // Enumerate the frames and compare their context to the propagator
            // to find its owner.
            IVsTrackSelectionEx selCtx = null;

            if (IsRunningOnDev12)
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev12.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfElement((uint)selElem, out selCtx);
                }
            }
            else
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev11.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfElement((uint)selElem, out selCtx);
                }
            }

            if (selCtx != null)
                return GetContextOwner(selCtx);

            return null;
        }

        object GetOwnerForSelectedHierarchy()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // Get the selection context object for propagator of the element
            // Enumerate the frames and compare their context to the propagator
            // to find its owner.

            IVsTrackSelectionEx hierarchySelCtx = null;
            IVsTrackSelectionEx selectionContainerCtx;

            if (IsRunningOnDev12)
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev12.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfSelection(out hierarchySelCtx, out selectionContainerCtx);
                }
            }
            else
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev11.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfSelection(out hierarchySelCtx, out selectionContainerCtx);
                }
            }

            if (hierarchySelCtx != null)
                return GetContextOwner(hierarchySelCtx);

            return null;
        }

        object GetOwnerForSelectedSelectionContainer()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // Get the selection context object for propagator of the element
            // Enumerate the frames and compare their context to the propagator
            // to find its owner.
            IVsTrackSelectionEx hierarchySelCtx;
            IVsTrackSelectionEx selectionContainerCtx = null;

            if (IsRunningOnDev12)
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev12.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfSelection(out hierarchySelCtx, out selectionContainerCtx);
                }
            }
            else
            {
                var selectionMonitorPrivate = (Microsoft.Internal.VisualStudio.Shell.Interop.Dev11.IVsMonitorSelectionExPrivate)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                if (selectionMonitorPrivate != null)
                {
                    selectionMonitorPrivate.GetContextOfSelection(out hierarchySelCtx, out selectionContainerCtx);
                }
            }

            if (selectionContainerCtx != null)
                return GetContextOwner(selectionContainerCtx);

            return null;
        }

        /// <summary>
        /// Enumerate the window frames looking for the owner of the context
        /// </summary>
        /// <param name="selCtx"></param>
        /// <returns></returns>
        object GetContextOwner(IVsTrackSelectionEx selCtx)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selCtx == null)
                return null;

            // No frame owns the empty selection context, just return it as its own owner.
            if (IsEmptySelectionContext(selCtx))
                return selCtx;

            IEnumWindowFrames frameEnum;
            IVsUIShell4 shell4 = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell4;
            
            Guid trackSelectionExServiceGuid = typeof(SVsTrackSelectionEx).GUID;
            Guid trackSelecitonExGuid = typeof(IVsTrackSelectionEx).GUID;

            shell4.GetWindowEnum((uint)__WindowFrameTypeFlags.WINDOWFRAMETYPE_All, out frameEnum);

            foreach (IVsWindowFrame frame in ComUtilities.EnumerableFrom(frameEnum))
            {
                object frameServiceProvider;
                if (ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out frameServiceProvider)) && frameServiceProvider != null)
                {
                    IntPtr pTrackSelection = IntPtr.Zero;
                    try
                    {
                        if (ErrorHandler.Succeeded(((IOleServiceProvider)frameServiceProvider).QueryService(ref trackSelectionExServiceGuid, ref trackSelecitonExGuid, out pTrackSelection)))
                        {
                            IVsTrackSelectionEx frameCtx = Marshal.GetObjectForIUnknown(pTrackSelection) as IVsTrackSelectionEx;
                            if (ComUtilities.IsSameComObject(selCtx, frameCtx))
                                return frame;
                        }
                    }
                    finally
                    {
                        if (pTrackSelection != IntPtr.Zero)
                            Marshal.Release(pTrackSelection);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Add the guids for various known categories of components that are used for UIContexts.  This
        /// includes, stock well known guids, Packages, ToolWindows, Services, Editors, SourceControl providers
        /// Debug engines, DataProviders, DataSources, Project types and KeyBinding tables.
        /// </summary>
        void InitializeContextDictionary()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            using (RegistryKey rootKey = GetRegistryRoot())
            {
                AddStockContexts();
                AddLanguageServices(rootKey);

                AddRegisteredGuids(rootKey, "Packages", "ProductName");
                AddRegisteredGuids(rootKey, "ToolWindows", "Name");
                AddRegisteredGuids(rootKey, "Services", "Name");
                AddRegisteredGuids(rootKey, "Editors", "DisplayName");
                AddRegisteredGuids(rootKey, "SourceControlProviders", null);
                AddRegisteredGuids(rootKey, "AD7Metrics\\Engine", "Name");
                AddRegisteredGuids(rootKey, "AD7Metrics(Debug)\\Engine", "Name");
                AddRegisteredGuids(rootKey, "DataProviders", null);
                AddRegisteredGuids(rootKey, "DataSources", null);
                AddRegisteredGuids(rootKey, "Projects", null);
                AddRegisteredGuids(rootKey, "UIContextRules", null);

                // need package resource lookup?
                AddRegisteredGuids(rootKey, "KeyBindingTables", null);
            }
        }

        /// <summary>
        /// Add ui contexts from all of the guids for language services
        /// </summary>
        /// <param name="rootKey"></param>
        void AddLanguageServices(RegistryKey rootKey)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            using (RegistryKey languagesKey = rootKey.OpenSubKey("Languages\\Language Services"))
            {
                foreach (string keyName in languagesKey.GetSubKeyNames())
                {
                    using (RegistryKey languageService = languagesKey.OpenSubKey(keyName))
                    {
                        string contextGuidString = (string)languageService.GetValue(null);
                        if (string.IsNullOrEmpty(contextGuidString)) continue;

                        string name = keyName;
                        string packageGuidString = String.Empty;

                        if (languageService.GetValueNames().Contains("LangResID"))
                        {
                            packageGuidString = (string)languageService.GetValue("Package");
                            if (!string.IsNullOrEmpty(packageGuidString))
                            {
                                int resID = (int)languageService.GetValue("LangResID");

                                name = "#" + resID.ToString();
                            }
                            else
                            {
                                name = keyName;
                            }
                        }

                        AddContextName(contextGuidString, name, packageGuidString);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate a registry key for the guid values that are used as UIContexts and extract
        /// names for them according to thier particular pattern.   If the name looks like a resource
        /// it will be looked up later.
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="keyList"></param>
        /// <param name="nameKey"></param>
        void AddRegisteredGuids(RegistryKey rootKey, string keyList, string nameKey)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            using (RegistryKey listKey = rootKey.OpenSubKey(keyList))
            {
                if (listKey != null)
                {
                    foreach (string contextGuidString in listKey.GetSubKeyNames())
                    {
                        using (RegistryKey itemKey = listKey.OpenSubKey(contextGuidString))
                        {
                            string contextName = itemKey.GetValue(nameKey) as string;
                            string packageGuidString = itemKey.GetValue("Package") as string;

                            // Try the default value
                            if (string.IsNullOrEmpty(contextName))
                            {
                                contextName = itemKey.GetValue(null) as string;
                            }

                            // No name provided
                            if (string.IsNullOrEmpty(contextName))
                            {
                                contextName = noNameProvided;
                            }

                            try
                            {
                                AddContextName(contextGuidString, contextName, packageGuidString);
                            }
                            catch (FormatException)
                            {
                                // ignore bad guids
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add the stock well known UI contexts
        /// </summary>
        void AddStockContexts()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // Add a few misc guids that are not listed anywhere
            AddContextName("{ 0x874f9ec9, 0x3b99, 0x4d89, { 0xb8, 0xf4, 0x38, 0x25, 0x83, 0xa6, 0xd5, 0x13 } }", "QToolsPackageLoaded");
            AddContextName("{ 0x9dd3bd8f, 0x4dee, 0x4975, { 0x86, 0xeb, 0x17, 0xf2, 0x9d, 0x75, 0xa7, 0x53 } }", "TestUI");
            AddContextName("{7174C6A0-B93D-11D1-9FF4-00A0C911004F}", "Not View Source Mode");
            AddContextName(typeof(ISccManagerLoaded).GUID.ToString(), "Scc Manager Loaded");

            AddContextName(VSConstants.UICONTEXT.CodeWindow_string, "Code Window");
            AddContextName(VSConstants.UICONTEXT.DataSourceWindowAutoVisible_string, "Data Source Window Auto Visible");
            AddContextName(VSConstants.UICONTEXT.DataSourceWizardSuppressed_string, "Data Source Wizard Suppressed");
            AddContextName(VSConstants.UICONTEXT.DataSourceWindowSupported_string, "Data Source Window Supported");
            AddContextName(VSConstants.UICONTEXT.Debugging_string, "Debugging");
            AddContextName(VSConstants.UICONTEXT.DesignMode_string, "Design Mode");
            AddContextName(VSConstants.UICONTEXT.Dragging_string, "Dragging");
            AddContextName(VSConstants.UICONTEXT.EmptySolution_string, "Empty Solution");
            AddContextName(VSConstants.UICONTEXT.FullScreenMode_string, "Full Screen Mode");
            AddContextName(VSConstants.UICONTEXT.HistoricalDebugging_string, "Historical Debugging");
            AddContextName(VSConstants.UICONTEXT.NoSolution_string, "No Solution");
            AddContextName(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string, "Not Building and Not Debugging");
            AddContextName(VSConstants.UICONTEXT.ProjectRetargeting_string, "Project Retargeting");
            AddContextName(VSConstants.UICONTEXT.SolutionBuilding_string, "Solution Building");
            AddContextName(VSConstants.UICONTEXT.SolutionExists_string, "Solution Exists");
            AddContextName(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, "Solution Exists and Fully Loaded");
            AddContextName(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string, "Solution Exists and Not BuildingAndNotDebugging");
            AddContextName(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, "Solution Has Multiple Projects");
            AddContextName(VSConstants.UICONTEXT.SolutionHasSingleProject_string, "Solution Has Single Project");
            AddContextName(VSConstants.UICONTEXT.SolutionOpening_string, "Solution Opening");
            AddContextName(VSConstants.UICONTEXT.SolutionOrProjectUpgrading_string, "Solution or Project Upgrading");
            AddContextName(VSConstants.UICONTEXT.ToolboxInitialized_string, "Toolbox Initialized");
            AddContextName(VSConstants.UICONTEXT.VBProject_string, "VB Project");
            AddContextName(VSConstants.UICONTEXT.CSharpProject_string, "C# Project");
            AddContextName(VSConstants.UICONTEXT.VCProject_string, "VC Project");
            AddContextName(VSConstants.UICONTEXT.FSharpProject_string, "F# Project");
            AddContextName(VSConstants.UICONTEXT.VBCodeAttribute_string, "VB Code Attribute");
            AddContextName(VSConstants.UICONTEXT.VBCodeClass_string, "VB Code Class");
            AddContextName(VSConstants.UICONTEXT.VBCodeDelegate_string, "VB Code Delegate");
            AddContextName(VSConstants.UICONTEXT.VBCodeEnum_string, "VB Code Enum");
            AddContextName(VSConstants.UICONTEXT.VBCodeFunction_string, "VB Code Function");
            AddContextName(VSConstants.UICONTEXT.VBCodeInterface_string, "VB Code Interface");
            AddContextName(VSConstants.UICONTEXT.VBCodeNamespace_string, "VB Code Namespace");
            AddContextName(VSConstants.UICONTEXT.VBCodeParameter_string, "VB Code Parameter");
            AddContextName(VSConstants.UICONTEXT.VBCodeProperty_string, "VB Code Property");
            AddContextName(VSConstants.UICONTEXT.VBCodeStruct_string, "VB Code Struct");
            AddContextName(VSConstants.UICONTEXT.VBCodeVariable_string, "VB Code Variable");
            AddContextName(VSConstants.UICONTEXT.VBProjOpened_string, "VB Project Open");
            AddContextName(VSConstants.UICONTEXT.ApplicationDesigner_string, "Application Designer");
            AddContextName(VSConstants.UICONTEXT.RESXEditor_string, "RESX Editor");
            AddContextName(VSConstants.UICONTEXT.SettingsDesigner_string, "Settings Designer");
            AddContextName(VSConstants.UICONTEXT.PropertyPageDesigner_string, "Property Page Designer");
#pragma warning disable CS0618 // Type or member is obsolete
            AddContextName(VSConstants.UICONTEXT.BackgroundProjectLoad_string, "Background Project Load");
#pragma warning restore CS0618 // Type or member is obsolete
            AddContextName(VSConstants.UICONTEXT.OsWindows8OrHigher_string, "OS Windows 8.0 or Higher");
            AddContextName(/*VSConstants.UICONTEXT.IdeUserSignedIn_string*/ "{67CFF80C-0863-4202-A4E4-CE80FDF8506E}", "IDE User Signed In");
            AddContextName(/*UICONTEXT_SynchronousSolutionOperation*/ "{30315F71-BB05-436B-8CC1-6A62B368C842}", "Synchronous Solution Operation");
        }

        /// <summary>
        /// When the CmdUIContext changes, find the real name of the context.  If it looks like a package resource, look up the real name.
        /// Mark our internal data to be active or inactive and add this event to the log. 
        /// </summary>
        /// <param name="dwCmdUICookie"></param>
        /// <param name="fActive"></param>
        /// <returns></returns>
        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (contextIDNames.ContainsKey(dwCmdUICookie))
            {
                UIContextInformation ci = contextIDNames[dwCmdUICookie];

                if (fActive != 0)
                {
                    // Before we add this guy, check to see if its name starts with a '#'
                    // Then look it up in its package now since it will be loaded
                    if (!string.IsNullOrEmpty(ci.Package))
                    {
                        if (ci.Name.StartsWith("#"))
                        {
                            string newName;
                            Guid packageGuid = new Guid(ci.Package);
                            IVsResourceManager resources = Package.GetGlobalService(typeof(SVsResourceManager)) as IVsResourceManager;
                            if (ErrorHandler.Succeeded(resources.LoadResourceString(ref packageGuid, 0, ci.Name, out newName)))
                                ci.Name = newName;
                            // Also try without the "#" in the name
                            else if (ErrorHandler.Succeeded(resources.LoadResourceString(ref packageGuid, 0, ci.Name.Substring(1), out newName)))
                                ci.Name = newName;
                        }

                    }
                    else if (ci.Name.StartsWith("resource="))
                    {
                        // Ad7 engines
                        StringBuilder newName = new StringBuilder(255);
                        int resourceIDPosition = ci.Name.IndexOf('#');
                        string resourceIDString = ci.Name.Substring(resourceIDPosition);
                        string resourceDLL = ci.Name.Substring("resource=".Length, ci.Name.Length - resourceIDPosition);
                        uint resourceID = 0;
                        if (uint.TryParse(resourceIDString.Substring(1), out resourceID))
                        {
                            if (!string.IsNullOrEmpty(resourceDLL))
                            {
                                IntPtr hModule = IntPtr.Zero;
                                try
                                {
                                    hModule = NativeMethods.LoadLibrary(resourceDLL);
                                    if (hModule != IntPtr.Zero)
                                    {
                                        NativeMethods.LoadString(hModule, resourceID, newName, newName.Capacity + 1);
                                        ci.Name = newName.ToString();
                                    }
                                }
                                finally
                                {
                                    if (hModule != IntPtr.Zero)
                                        NativeMethods.FreeLibrary(hModule);
                                }
                            }
                        }
                    }

                    LiveContexts.Add(ci);
                    ci.Enabled = true;
                }
                else
                {
                    LiveContexts.Remove(ci);
                    ci.Enabled = false;
                }

                uiContextLog.Insert(0, new UIContextLogItem(fActive == 1, ci));
            }
            else
            {
                UIContextInformation ci = new UIContextInformation(dwCmdUICookie, "No name found", "Unknown", String.Empty);
                contextIDNames.Add(dwCmdUICookie, ci);
                uiContextLog.Insert(0, new UIContextLogItem(fActive == 1, ci));
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Find the description of a selection element by examining the object for known types.
        /// These inclued IVsWinodwFrame, IVsUserContext, IVsHierarchy and present something useful.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        string GetSelectionElementDescription(object element)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (element == null)
                return "null";

            IVsDiagnosticsItem diagnosticItem = element as IVsDiagnosticsItem;
            if (diagnosticItem != null)
            {
                return diagnosticItem.DiagnosticsName;
            }
            
            IVsWindowFrame frame = element as IVsWindowFrame;
            if (frame != null)
            {
                object var;
                frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out var);
                return string.Format("IVsWindowFrame: {0}", var as string);
            }

            IVsUserContext userContext = element as IVsUserContext;
            if (userContext != null)
            {
                // Enumerate all attributes looking for a F1 keyword, 
                string keyword = String.Empty;
                int count;
                userContext.CountAttributes("keyword", 1 /*true*/, out count);
                for (int index = 0; index < count; index++)
                {
                    string name;
                    string value;
                    VSUSERCONTEXTATTRIBUTEUSAGE[] usage = new VSUSERCONTEXTATTRIBUTEUSAGE[1];

                    userContext.GetAttribute(index, "keyword", 1 /*true*/, out name, out value);
                    userContext.GetAttrUsage(index, 1 /*true*/, usage);

                    keyword = string.Format("{0}={1}", name, value);

                    if (usage[0] == VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_LookupF1)
                    {
                        break;
                    }
                }
                return string.Format("IVsUserContext: {0}", keyword);
            }

            if (element is string)
            {
                return (string)element;
            }

            IVsHierarchy hierarchy = element as IVsHierarchy;
            if (hierarchy != null)
            {
                object var;
                if (ErrorHandler.Succeeded(hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectName, out var)) && var is string)
                    return string.Format("IVsHierarchy: {0}", var as string);
            }

            IOleUndoManager pUndo = element as IOleUndoManager;
            if (pUndo != null)
            {
                return "IOleUndoManager";
            }

            IVsFindTarget pFind = element as IVsFindTarget;
            if (pFind != null)
            {
                IVsWindowFrame findTargetFrame;
                string caption = String.Empty;

                object varFrame;
                pFind.GetProperty((uint)__VSFTPROPID.VSFTPROPID_WindowFrame, out varFrame);
                findTargetFrame = varFrame as IVsWindowFrame;

                if (frame != null)
                {
                    object varCaption;
                    frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out varCaption);
                    caption = varCaption as string;
                }

                return string.Format("IVsFindTarget: {0}", caption);
            }

            if (element is IVsTrackSelectionEx selCtx)
            {
                if (IsEmptySelectionContext(selCtx))
                    return "Empty Selection Context";
                else
                    return "Unknown Selection Context";
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(element))
            {
                return string.Format("IUnknown: ({0})", element.GetType().GUID.ToString("B"));
            }

            return element.ToString();
        }

        /// <summary>
        /// Whe an element value changes, add the event to the log with a description of the object that is being
        /// added or removed.  For the active selection elements, update the description of the element.
        /// </summary>
        /// <param name="elementid"></param>
        /// <param name="varValueOld"></param>
        /// <param name="varValueNew"></param>
        /// <returns></returns>
        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // If window frame, get the tool window caption, name, persistenc slot etc...
            // If document, get its editor, physical view, logical view, interigate object types, impl etc...

            object valueOwner = GetOwnerForSelectedElement((VSConstants.SelectionElement)elementid);
            string valueOwnerDescription = (valueOwner != null) ? GetSelectionElementDescription(valueOwner) : String.Empty;

            if (varValueOld != null)
                SelectionLog.Insert(0, new SelectionLogItem(false, (VSConstants.SelectionElement)elementid, GetSelectionElementDescription(varValueOld), String.Empty));

            if (varValueNew != null)
                SelectionLog.Insert(0, new SelectionLogItem(true, (VSConstants.SelectionElement)elementid, GetSelectionElementDescription(varValueNew), valueOwnerDescription));

            SelectionItemInfo item = SelectionItems.First(e => e.SelElemID == (VSConstants.SelectionElement)elementid);
            if (item != null)
            {
                item.Description = GetSelectionElementDescription(varValueNew);
                item.ContextOwner = valueOwnerDescription;
            }

            if (elementid == (uint)VSConstants.VSSELELEMID.SEID_WindowFrame)
            {
                // Changes to LastWindowFrame do not cause change notification however it does change every time WinodwFrame
                // changes so we will retrieve the new value here.  We will not know the old value.
                object varLastWindowFrame = null;
                selectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_LastWindowFrame, out varLastWindowFrame);
                if (varLastWindowFrame != null)
                {
                    item = SelectionItems.First(e => e.SelElemID == (VSConstants.SelectionElement)VSConstants.VSSELELEMID.SEID_LastWindowFrame);
                    if (item != null)
                    {
                        item.Description = GetSelectionElementDescription(varLastWindowFrame);
                        item.ContextOwner = String.Empty;  // No one owns this context
                    }
                }
            }

            return VSConstants.S_OK;
        }

        string GetItemIDName(uint itemid)
        {
            switch (itemid)
            {
                case (uint)VSConstants.VSITEMID.Nil:
                    return "Nil";
                case (uint)VSConstants.VSITEMID.Root:
                    return "Root";
                case (uint)VSConstants.VSITEMID.Selection:
                    return "Selection";
                default:
                    return string.Format("0x{0}", itemid.ToString("X"));
            }

        }



        public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            object hierarchyOwner = GetOwnerForSelectedHierarchy();
            object selectionContainerOwner = GetOwnerForSelectedSelectionContainer();
            string hierarchyOwnerDescription = (hierarchyOwner != null) ? GetSelectionElementDescription(hierarchyOwner) : String.Empty;
            string selectionContainerOwnerDescription = (selectionContainerOwner != null) ? GetSelectionElementDescription(selectionContainerOwner) : String.Empty;

            string newItemDescription = GetSelectionElementDescription(pHierNew);
            string oldItemDescription = GetSelectionElementDescription(pHierOld);
            if (pHierOld != pHierNew)
                LogSelectionChange((uint)SelectionItemInfo.SpecialElement.Hierarchy, oldItemDescription, newItemDescription, hierarchyOwnerDescription);

            UpdateSelectionData((uint)SelectionItemInfo.SpecialElement.Hierarchy, newItemDescription, hierarchyOwnerDescription);

            // Get the description of the item from the hierarchy
            object varDescription;
            string oldItemIDName = GetItemIDName(itemidOld);
            if (pHierOld != null)
            {
                pHierOld.GetProperty(itemidOld, (int)__VSHPROPID.VSHPROPID_Caption, out varDescription);
                oldItemDescription = "ItemID (" + oldItemIDName + ") " + varDescription as string;
            }
            else
                oldItemDescription = oldItemIDName;

            string newItemIDName = GetItemIDName(itemidNew);
            if (pHierNew != null)
            {
                pHierNew.GetProperty(itemidNew, (int)__VSHPROPID.VSHPROPID_Caption, out varDescription);
                newItemDescription = "ItemID (" + newItemIDName + ") " + varDescription as string;
            }
            else
                newItemDescription = newItemIDName;

            if (itemidOld != itemidNew || ((pHierOld != pHierNew) && pHierNew == null))
                LogSelectionChange((uint)SelectionItemInfo.SpecialElement.ItemID, oldItemDescription, newItemDescription, hierarchyOwnerDescription);

            UpdateSelectionData((uint)SelectionItemInfo.SpecialElement.ItemID, newItemDescription, hierarchyOwnerDescription);

            // Update multi select every time
            oldItemDescription = null;
            newItemDescription = null;
            if (pMISNew != null)
            {
                uint itemCount;
                int singleHierarchy;
                pMISNew.GetSelectionInfo(out itemCount, out singleHierarchy);

                StringBuilder description = new StringBuilder(string.Format("MultiSelect ({0})", itemCount));
                if (itemCount > 0)
                {
                    VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                    pMISNew.GetSelectedItems(0, itemCount, items);

                    description.Append(" {");
                    for (int item = 0; item < itemCount; item++)
                    {
                        description.Append(GetItemIDName(items[item].itemid));

                        if (items[item].pHier != null)
                        {
                            items[item].pHier.GetProperty(items[item].itemid, (int)__VSHPROPID.VSHPROPID_Caption, out varDescription);

                            description.Append(" \"" + varDescription as string + "\"");
                        }

                        if (item <= itemCount - 1)
                            description.Append(", ");
                    }
                    description.Append("}");
                }
                newItemDescription = description.ToString();

                if (pMISOld != pMISNew)
                    LogSelectionChange((uint)SelectionItemInfo.SpecialElement.MultiItemSelect, oldItemDescription, newItemDescription, hierarchyOwnerDescription);
            }

            UpdateSelectionData((uint)SelectionItemInfo.SpecialElement.MultiItemSelect, newItemDescription, hierarchyOwnerDescription);

            // Update sc select every time
            oldItemDescription = null;
            newItemDescription = null;
            if (pSCNew != null)
            {
                uint itemCount;
                pSCNew.CountObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_ALL, out itemCount);

                StringBuilder description = new StringBuilder(string.Format("Properties ({0})", itemCount));
                if (itemCount > 0)
                {
                    object[] items = new object[itemCount];
                    pSCNew.GetObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_ALL, itemCount, items);

                    description.Append(" {");
                    for (int item = 0; item < itemCount; item++)
                    {
                        description.Append(items[item].ToString());
                        if (item <= itemCount - 1)
                            description.Append(", ");
                    }
                    description.Append("}");
                }

                newItemDescription = description.ToString();

                if (pSCOld != pSCNew)
                    LogSelectionChange((uint)SelectionItemInfo.SpecialElement.SelectionContainer, oldItemDescription, newItemDescription, selectionContainerOwnerDescription);
            }

            UpdateSelectionData((uint)SelectionItemInfo.SpecialElement.SelectionContainer, newItemDescription, selectionContainerOwnerDescription);

            return VSConstants.S_OK;
        }

        void LogSelectionChange(uint selElem, string oldItemDescription, string newItemDescription, string newItemOwnerDescription)
        {
            if (oldItemDescription != null)
                SelectionLog.Insert(0, new SelectionLogItem(false, (VSConstants.SelectionElement)selElem, oldItemDescription, String.Empty));

            SelectionLog.Insert(0, new SelectionLogItem(true, (VSConstants.SelectionElement)selElem, newItemDescription, newItemOwnerDescription));
        }

        void UpdateSelectionData(uint selElem, string newItemDescription, string newItemOwnerDescription)
        {
            SelectionItemInfo selectionItem = SelectionItems.First(e => e.SelElemID == (VSConstants.SelectionElement)selElem);
            selectionItem.Description = newItemDescription;
            selectionItem.ContextOwner = newItemOwnerDescription;
        }

        /// <summary>
        /// When this item is disposed we must release our sync on the Shell selection monitor and
        /// save the favorites to the settings store.
        /// </summary>
        public void Dispose()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            selectionMonitor.UnadviseSelectionEvents(selectionEventsCookie);

            IVsWritableSettingsStore settingsStore;
            IVsSettingsManager userSettings = Package.GetGlobalService(typeof(SVsSettingsManager)) as IVsSettingsManager;
            userSettings.GetWritableSettingsStore((uint)__VsSettingsScope.SettingsScope_UserSettings, out settingsStore);

            SaveContextIDList(settingsStore, favoriteContexts, "Favorites");
        }
    }
}
