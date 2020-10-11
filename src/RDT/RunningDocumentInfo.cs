using System;
using System.Runtime.InteropServices;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public struct RunningDocumentInfo
    {
        private readonly IVsRunningDocumentTable _rdt;
        private readonly IVsRunningDocumentTable4 _rdt4;
        private uint _flags;
        private uint _readLocks;
        private uint _editLocks;
        private string _moniker;
        private IVsHierarchy _hierarchy;
        private uint _itemId;
        private object _docData;
        private Guid _projectGuid;

        private bool _shouldRequestHierarchyItem;
        private bool _shouldRequestDocData;

        public RunningDocumentInfo(IVsRunningDocumentTable rdt, uint docCookie)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Validate.IsNotNull(rdt, "rdt");
            _rdt = rdt;
            _rdt4 = rdt as IVsRunningDocumentTable4;

            DocCookie = docCookie;
            _flags = 0;
            _readLocks = 0;
            _editLocks = 0;
            _moniker = null;
            _hierarchy = null;
            _itemId = 0;
            _docData = null;
            _projectGuid = Guid.Empty;
            _shouldRequestHierarchyItem = false;
            _shouldRequestDocData = false;

            if ((_rdt4 != null) && _rdt4.IsCookieValid(docCookie))
            {
                _flags = _rdt4.GetDocumentFlags(docCookie);
                _readLocks = _rdt4.GetDocumentReadLockCount(docCookie);
                _editLocks = _rdt4.GetDocumentEditLockCount(docCookie);
                _moniker = _rdt4.GetDocumentMoniker(docCookie);
                _projectGuid = _rdt4.GetDocumentProjectGuid(docCookie);

                // if the hierarchy has been initialized we can get the hierarchy/itemId now; 
                // otherwise wait until they're needed so we don't prematurely load the project
                if (IsHierarchyInitialized)
                    _rdt4.GetDocumentHierarchyItem(DocCookie, out _hierarchy, out _itemId);
                else
                    _shouldRequestHierarchyItem = true;

                // if the document has been initialized we can get the and docdata now; 
                // otherwise wait until it's needed so we don't prematurely initialize the document
                if (IsDocumentInitialized)
                    _docData = _rdt4.GetDocumentData(DocCookie);
                else
                    _shouldRequestDocData = true;
            }
            else
            {
                IntPtr docData = IntPtr.Zero;

                try
                {
                    _rdt.GetDocumentInfo(docCookie, out _flags, out _readLocks, out _editLocks,
                                         out _moniker, out _hierarchy, out _itemId, out docData);

                    if (docData != IntPtr.Zero)
                        _docData = Marshal.GetObjectForIUnknown(docData);
                }
                finally
                {
                    if (docData != IntPtr.Zero)
                        Marshal.Release(docData);
                }
            }
        }

        public uint DocCookie { get; set; }

        public uint Flags
        {
            get => _flags;
            set => _flags = value;
        }

        public uint ReadLocks
        {
            get => _readLocks;
            set => _readLocks = value;
        }

        public uint EditLocks
        {
            get => _editLocks;
            set => _editLocks = value;
        }

        public string Moniker
        {
            get => _moniker;
            set => _moniker = value;
        }

        public Guid ProjectGuid
        {
            get => _projectGuid;
            set => _projectGuid = value;
        }

        public object DocData
        {
            get
            {
                Shell.ThreadHelper.ThrowIfNotOnUIThread();
                if (_shouldRequestDocData)
                {
                    _docData = _rdt4.GetDocumentData(DocCookie);
                    _shouldRequestDocData = false;
                }

                return _docData;
            }
            set
            {
                _docData = value;
                _shouldRequestDocData = false;
            }
        }

        public IVsHierarchy Hierarchy
        {
            get
            {
                Shell.ThreadHelper.ThrowIfNotOnUIThread();
                if (_shouldRequestHierarchyItem)
                {
                    _rdt4.GetDocumentHierarchyItem(DocCookie, out _hierarchy, out _itemId);
                    _shouldRequestHierarchyItem = false;
                }

                return _hierarchy;
            }
            set
            {
                _hierarchy = value;
                _shouldRequestHierarchyItem = false;
            }
        }

        public uint ItemId
        {
            get
            {
                Shell.ThreadHelper.ThrowIfNotOnUIThread();

                if (_shouldRequestHierarchyItem)
                {
                    _rdt4.GetDocumentHierarchyItem(DocCookie, out _hierarchy, out _itemId);
                    _shouldRequestHierarchyItem = false;
                }

                return _itemId;
            }
            set
            {
                _itemId = value;
                _shouldRequestHierarchyItem = false;
            }
        }

        public bool IsDocumentInitialized => (Flags & (uint)_VSRDTFLAGS4.RDT_PendingInitialization) == 0;

        public bool IsHierarchyInitialized => (Flags & (uint)_VSRDTFLAGS4.RDT_PendingHierarchyInitialization) == 0;
    }
}
