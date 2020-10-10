using System;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;

using VSCOOKIE = System.UInt32;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Represents an entry in the Running Document Table
    /// </summary>
    class RdtEntry : DependencyObject
    {
        public static event EventHandler OnFinalUnlock;

        public RdtEntry(RunningDocumentInfo info)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Cookie = info.DocCookie;
            Moniker = info.Moniker;
            ReadLocks = info.ReadLocks;
            EditLocks = info.EditLocks;
            Flags = (RdtFlags)(int)info.Flags;
            ProjectId = info.ProjectGuid;

            if (info.IsDocumentInitialized)
            {
                if (info.DocData is IVsPersistDocData persist)
                {
                    int isDirty;
                    persist.IsDocDataDirty(out isDirty);
                    IsDirty = Convert.ToBoolean(isDirty);

                    int hr = persist.GetGuidEditorType(out Guid editorType);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        ClassId = editorType;
                    }
                }

                if (info.DocData is IVsPersistDocData2 persist2)
                {
                    persist2.IsDocDataReadOnly(out int isReadOnly);
                    IsReadOnly = Convert.ToBoolean(isReadOnly);
                }
            }
        }

        public override string ToString()
        {
            return String.Format("cookie={0}, {1}", Cookie, Moniker);
        }

        #region Dependency properties

        #region Cookie

        public static readonly DependencyProperty CookieProperty = 
            DependencyProperty.Register("Cookie",
                                        typeof(VSCOOKIE),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(VSConstants.VSCOOKIE_NIL));

        public VSCOOKIE Cookie
        {
            get { return (VSCOOKIE) GetValue(CookieProperty); }
            set { SetValue(CookieProperty, value); }
        }

        #endregion Cookie

        #region Moniker

        public static readonly DependencyProperty MonikerProperty = 
            DependencyProperty.Register("Moniker",
                                        typeof(string),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(null));

        public string Moniker
        {
            get { return (string) GetValue(MonikerProperty); }
            set { SetValue(MonikerProperty, value); }
        }

        #endregion Moniker

        #region ReadLocks

        public static readonly DependencyProperty ReadLocksProperty = 
            DependencyProperty.Register("ReadLocks",
                                        typeof(uint),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(0u, OnLockCountChanged));

        public uint ReadLocks
        {
            get { return (uint) GetValue(ReadLocksProperty); }
            set { SetValue(ReadLocksProperty, value); }
        }

        #endregion ReadLocks

        #region EditLocks

        public static readonly DependencyProperty EditLocksProperty = 
            DependencyProperty.Register("EditLocks",
                                        typeof(uint),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(0u, OnLockCountChanged));

        public uint EditLocks
        {
            get { return (uint) GetValue(EditLocksProperty); }
            set { SetValue(EditLocksProperty, value); }
        }

        #endregion EditLocks

        #region Flags

        public static readonly DependencyProperty FlagsProperty = 
            DependencyProperty.Register("Flags",
                                        typeof(RdtFlags),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(RdtFlags.None));

        public RdtFlags Flags
        {
            get { return (RdtFlags) GetValue(FlagsProperty); }
            set { SetValue(FlagsProperty, value); }
        }

        #endregion Flags

        #region IsDirty

        public static readonly DependencyProperty IsDirtyProperty = 
            DependencyProperty.Register("IsDirty",
                                        typeof(bool),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(false));

        public bool IsDirty
        {
            get { return (bool) GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        #endregion IsDirty

        #region IsReadOnly

        public static readonly DependencyProperty IsReadOnlyProperty = 
            DependencyProperty.Register("IsReadOnly",
                                        typeof(bool),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(false));

        public bool IsReadOnly
        {
            get { return (bool) GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        #endregion IsReadOnly

        #region ProjectId

        public static readonly DependencyProperty ProjectIdProperty =
            DependencyProperty.Register("ProjectId",
                                        typeof(Guid),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(null));

        public Guid ProjectId
        {
            get { return (Guid)GetValue(ProjectIdProperty); }
            set { SetValue(ProjectIdProperty, value); }
        }

        #endregion ProjectId

        #region ClassId

        public static readonly DependencyProperty ClassIdProperty =
            DependencyProperty.Register("ClassId",
                                        typeof(Guid),
                                        typeof(RdtEntry),
                                        new UIPropertyMetadata(null));

        public Guid ClassId
        {
            get { return (Guid)GetValue(ClassIdProperty); }
            set { SetValue(ClassIdProperty, value); }
        }

        #endregion ClassId

        #endregion Dependency properties

        private static void OnLockCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RdtEntry) d).OnLockCountChanged();
        }

        private void OnLockCountChanged()
        {
            if ((ReadLocks == 0) && (EditLocks == 0))
            {
                EventHandler onFinalUnlock = OnFinalUnlock;

                if (onFinalUnlock != null)
                    onFinalUnlock(this, EventArgs.Empty);
            }
        }
    }
}
