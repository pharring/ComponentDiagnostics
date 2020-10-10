using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

using VSCOOKIE = System.UInt32;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Event sink for events raised by the Running Document Table
    /// </summary>
    class RdtEventSink
        : IVsRunningDocTableEvents3
        , IVsRunningDocTableEvents4
        , IVsRunningDocTableEvents5
        , IVsRunningDocTableEvents6
    {
        readonly RdtDiagnosticsDataSource _ds;
        VSCOOKIE _cookie;

        public RdtEventSink(RdtDiagnosticsDataSource ds)
        {
            _ds = ds;
        }

        public void Advise()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IVsRunningDocumentTable rdt = (IVsRunningDocumentTable) Package.GetGlobalService(typeof(SVsRunningDocumentTable));
            rdt.AdviseRunningDocTableEvents (this, out _cookie);
        }

        public void Unadvise()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IVsRunningDocumentTable rdt = (IVsRunningDocumentTable) Package.GetGlobalService(typeof(SVsRunningDocumentTable));
            rdt.UnadviseRunningDocTableEvents (_cookie);
        }

        RdtEvent MakeEvent (VSCOOKIE cookie, string format, params object[] args)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat ("Cookie = {0}: ", cookie);
            builder.AppendFormat (format, args);

            return new RdtEvent (builder.ToString());
        }

        #region IVsRunningDocTableEvents3 Members

        public int OnAfterAttributeChange(VSCOOKIE cookie, uint grfAttribs)
        {
            HandleOnAfterAttributeChange(cookie, grfAttribs, oldName: null, newName: null, eventName: "OnAfterAttributeChange");
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(VSCOOKIE cookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            HandleOnAfterAttributeChange(cookie, grfAttribs, pszMkDocumentOld, pszMkDocumentNew, "OnAfterAttributeChangeEx");
            return VSConstants.S_OK;
        }

        void HandleOnAfterAttributeChange(VSCOOKIE cookie, uint grfAttribs, string oldName, string newName, string eventName)
        {
            RdtAttributes attrs = (RdtAttributes) (int) grfAttribs;

            string eventText = $"{eventName}: attributes={attrs}";
            if (attrs.HasFlag(RdtAttributes.RDTA_MkDocument))
                eventText += $", old={oldName}, new={newName}";

            RdtEvent evt = MakeEvent(cookie, eventText);
            _ds.RecordEvent(evt);

            RdtEntry entry = _ds.FindEntry(cookie);
            if (entry == null)
                return;

            if (attrs.HasFlag(RdtAttributes.RDTA_DocDataIsDirty))
                entry.IsDirty = true;
            else if (attrs.HasFlag(RdtAttributes.RDTA_DocDataIsNotDirty))
                entry.IsDirty = false;

            if (attrs.HasFlag(RdtAttributes.RDTA_DocDataIsReadOnly))
                entry.IsReadOnly = true;
            else if (attrs.HasFlag(RdtAttributes.RDTA_DocDataIsNotReadOnly))
                entry.IsReadOnly = false;

            if (attrs.HasFlag(RdtAttributes.RDTA_MkDocument))
                entry.Moniker = newName;
        }

        public int OnAfterDocumentWindowHide(VSCOOKIE cookie, IVsWindowFrame pFrame)
        {
            RdtEvent evt = MakeEvent (cookie, "OnAfterDocumentWindowHide");
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(VSCOOKIE cookie, uint lockType, uint readLocksRemaining, uint editLocksRemaining)
        {
            RdtFlags flags = (RdtFlags) (int) lockType;
            RdtEvent evt = MakeEvent(cookie, "OnAfterFirstDocumentLock: Lock Type={0}, Read Locks Remaining={1}, Edit Locks Remaining={2}", flags, readLocksRemaining, editLocksRemaining);
            _ds.RecordEvent(evt);

            // add this document to the data source if it's not there yet
            RdtEntry entry = _ds.FindEntry(cookie);
            if (entry == null)
            {
                RunningDocumentTable rdt = new RunningDocumentTable();
                RunningDocumentInfo info = rdt.GetDocumentInfo (cookie);

                entry = new RdtEntry(info);
                _ds.Entries.Add(entry);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterSave(VSCOOKIE cookie)
        {
            RdtEvent evt = MakeEvent(cookie, "OnAfterSave");
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(VSCOOKIE cookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            RdtEvent evt = MakeEvent(cookie, "OnBeforeDocumentWindowShow (first show={0})", (fFirstShow != 0));
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(VSCOOKIE cookie, uint lockType, uint readLocksRemaining, uint editLocksRemaining)
        {
            RdtFlags flags = (RdtFlags) (int) lockType;
            RdtEvent evt = MakeEvent(cookie, "OnBeforeLastDocumentUnlock: Lock Type={0}, Read Locks Remaining={1}, Edit Locks Remaining={2}", flags, readLocksRemaining, editLocksRemaining);
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        public int OnBeforeSave(VSCOOKIE cookie)
        {
            RdtEvent evt = MakeEvent(cookie, "OnBeforeSave");
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsRunningDocTableEvents4 Members

        public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string moniker, int fClosedWithoutSaving)
        {
            RdtEvent evt = new RdtEvent(string.Format("OnAfterLastDocumentUnlock: {0} (fClosedWithoutSaving={1})", moniker, (fClosedWithoutSaving != 0)));
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        public int OnAfterSaveAll()
        {
            RdtEvent evt = new RdtEvent("OnAfterSaveAll");
            _ds.RecordEvent (evt);

            return VSConstants.S_OK;
        }

        public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string moniker)
        {
            RdtEvent evt = new RdtEvent(string.Format ("OnBeforeFirstDocumentLock: {0}", moniker));
            _ds.RecordEvent(evt);

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsRunningDocTableEvents5 Members

        public void OnAfterDocumentLockCountChanged(VSCOOKIE cookie, uint lockType, VSCOOKIE oldCount, VSCOOKIE newCount)
        {
            RdtFlags flags = (RdtFlags)(int) lockType;
            RdtEvent evt = MakeEvent(cookie, "OnAfterDocumentLockCountChanged: Lock Type={0}, Before={1}, After={2}", flags, oldCount, newCount);
            _ds.RecordEvent(evt);

            RdtEntry entry = _ds.FindEntry (cookie);
            if (entry == null)
                return;
            
            if (flags.HasFlag (RdtFlags.RDT_ReadLock))
                entry.ReadLocks = newCount;

            else if (flags.HasFlag (RdtFlags.RDT_EditLock))
                entry.EditLocks = newCount;
        }

        #endregion

        #region IVsRunningDocTableEvents6 Members

        public void OnAfterDocDataChanged(uint cookie, System.IntPtr punkDocDataOld, System.IntPtr punkDocDataNew)
        {
            RdtEvent evt = MakeEvent(cookie, "OnAfterDocDataChanged: DocDataOld={0}, DocDataNew={1}", punkDocDataOld, punkDocDataNew);
            _ds.RecordEvent(evt);
        }

        #endregion
    }
}
